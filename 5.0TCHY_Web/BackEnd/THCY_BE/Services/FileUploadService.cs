// Services/FileUploadService.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.EntityFrameworkCore;
using THCY_BE.DataBase;
using System.Security.Claims;

namespace THCY_BE.Services
{
    public interface IFileUploadService
    {
        Task<FileUploadResult> UploadFileAsync(IFormFile file, string targetFolder, int? customId = null, NamingStrategy? namingStrategy = null, Dictionary<string, object>? additionalData = null);
        Task<List<FileUploadResult>> UploadFilesAsync(IFormFile[] files, string targetFolder, int? customId = null, NamingStrategy? namingStrategy = null, Dictionary<string, object>? additionalData = null);
        Task<bool> DeleteFileAsync(string filePath);
        string GenerateFileUrl(string relativePath);
    }

    /// <summary>
    /// 文件命名策略枚举
    /// </summary>
    public enum NamingStrategy
    {
        /// <summary>默认策略：{ID}_{时间戳}_{随机数}.扩展名</summary>
        Default,
        /// <summary>用户头像：user_{用户ID}_avatar_{时间戳}.扩展名</summary>
        UserAvatar,
        /// <summary>约战场角色：oc_{角色ID}_{角色名}_{年龄}yo_{时间戳}.扩展名</summary>
        OCBattleCharacter,
        /// <summary>OC图库：oc_{角色ID}_gallery_{时间戳}_{随机数}.扩展名</summary>
        OCGallery,
        /// <summary>OC主图：oc_{角色ID}_main_{时间戳}_{随机数}.扩展名</summary>
        OCMainImage,
        /// <summary>对战记录：battle_{对战ID}_record_{时间戳}.扩展名</summary>
        BattleRecord,
        /// <summary>简单时间戳：{时间戳}.扩展名</summary>
        SimpleTimestamp
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FileUrl { get; set; }
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FileUploadService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IServiceProvider _serviceProvider;

        public FileUploadService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<FileUploadService> logger,
            IWebHostEnvironment environment,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _environment = environment;
            _serviceProvider = serviceProvider;

            _logger.LogInformation("=== FileUploadService 初始化 ===");
            _logger.LogInformation("环境: {Environment}", _environment.EnvironmentName);
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string targetFolder, int? customId = null, NamingStrategy? namingStrategy = null, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                _logger.LogInformation("=== 开始文件上传 ===");
                _logger.LogInformation("环境: {Environment}", _environment.EnvironmentName);
                _logger.LogInformation("目标文件夹: {TargetFolder}, 命名策略: {NamingStrategy}", targetFolder, namingStrategy);

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("文件为空");
                    return new FileUploadResult { Success = false, ErrorMessage = "文件为空" };
                }

                _logger.LogInformation("文件信息: 名称={FileName}, 大小={FileSize} bytes, 类型={ContentType}",
                    file.FileName, file.Length, file.ContentType);

                // 验证文件类型
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning("不支持的文件类型: {FileExtension}", fileExtension);
                    return new FileUploadResult { Success = false, ErrorMessage = $"不支持的文件类型: {fileExtension}" };
                }

                // 验证文件大小
                var maxFileSize = _configuration.GetValue<long?>("FileUpload:MaxFileSize") ?? 5L * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning("文件大小超过限制: {FileSize} > {MaxFileSize}", file.Length, maxFileSize);
                    return new FileUploadResult { Success = false, ErrorMessage = $"文件大小超过限制: {maxFileSize} bytes" };
                }

                // 根据目标文件夹智能选择命名策略
                var strategy = namingStrategy ?? GetNamingStrategyByFolder(targetFolder);

                // 生成安全的文件名
                var safeFileName = await GenerateSafeFileNameAsync(file.FileName, customId, strategy, additionalData);
                var relativePath = $"{targetFolder.Trim('/')}/{safeFileName}";

                _logger.LogInformation("生成的文件路径: {RelativePath}", relativePath);

                // 验证路径安全
                if (!ValidateRelativePath(relativePath))
                {
                    _logger.LogWarning("文件路径不安全: {RelativePath}", relativePath);
                    return new FileUploadResult { Success = false, ErrorMessage = "文件路径不安全" };
                }

                // 上传到NAS
                _logger.LogInformation("开始上传到NAS...");
                var uploadResult = await UploadToNasAsync(file.OpenReadStream(), relativePath, fileExtension, file.Length);

                if (uploadResult)
                {
                    var fileUrl = GenerateFileUrl(relativePath);
                    _logger.LogInformation("✅ 文件上传成功: {FileUrl}", fileUrl);

                    return new FileUploadResult
                    {
                        Success = true,
                        FilePath = relativePath,
                        FileName = safeFileName,
                        FileSize = file.Length,
                        FileUrl = fileUrl
                    };
                }

                _logger.LogError("文件上传失败");
                return new FileUploadResult { Success = false, ErrorMessage = "文件上传失败" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件上传失败 - 环境: {Environment}", _environment.EnvironmentName);
                return new FileUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<List<FileUploadResult>> UploadFilesAsync(IFormFile[] files, string targetFolder, int? customId = null, NamingStrategy? namingStrategy = null, Dictionary<string, object>? additionalData = null)
        {
            var results = new List<FileUploadResult>();

            _logger.LogInformation("开始批量上传 {FileCount} 个文件到 {TargetFolder}", files.Length, targetFolder);

            foreach (var file in files)
            {
                var result = await UploadFileAsync(file, targetFolder, customId, namingStrategy, additionalData);
                results.Add(result);
            }

            var successCount = results.Count(r => r.Success);
            var failCount = results.Count(r => !r.Success);
            _logger.LogInformation("批量上传完成: 成功 {SuccessCount} 个, 失败 {FailCount} 个", successCount, failCount);

            return results;
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("开始删除文件: {FilePath}", filePath);

                if (string.IsNullOrWhiteSpace(filePath) || !ValidateRelativePath(filePath))
                {
                    _logger.LogWarning("文件路径无效: {FilePath}", filePath);
                    return false;
                }

                var nasEndpoint = GetNasEndpoint();
                var username = _configuration["NasCredentials:Username"];
                var password = _configuration["NasCredentials:Password"];

                _logger.LogInformation("删除文件配置: 端点={NasEndpoint}, 用户={Username}", nasEndpoint, username);

                var deleteUrl = $"{nasEndpoint}/{filePath.TrimStart('/')}";
                _logger.LogInformation("删除URL: {DeleteUrl}", deleteUrl);

                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                var response = await client.DeleteAsync(deleteUrl);

                _logger.LogInformation("删除响应状态: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ 文件删除成功: {FilePath}", filePath);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ 文件删除失败: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除文件失败: {FilePath}", filePath);
                return false;
            }
        }

        public string GenerateFileUrl(string relativePath)
        {
            var fileUrl = $"/api/File/image?path={Uri.EscapeDataString(relativePath)}";
            _logger.LogInformation("生成文件URL: {FileUrl}", fileUrl);
            return fileUrl;
        }

        /// <summary>
        /// 根据目标文件夹智能选择命名策略
        /// </summary>
        private NamingStrategy GetNamingStrategyByFolder(string targetFolder)
        {
            var folder = targetFolder.ToLowerInvariant().Trim('/');

            return folder switch
            {
                "avatars" or "avatar" => NamingStrategy.UserAvatar,
                "oc_battle_picture" or "oc-battle-picture" or "oc_battle" => NamingStrategy.OCBattleCharacter,
                var f when f.Contains("gallery") => NamingStrategy.OCGallery,
                var f when f.Contains("oc-image") || f.Contains("oc_main") || f.Contains("oc-images") => NamingStrategy.OCMainImage,
                var f when f.Contains("battle") => NamingStrategy.BattleRecord,
                var f when f.Contains("temp") || f.Contains("tmp") => NamingStrategy.SimpleTimestamp,
                _ => NamingStrategy.Default
            };
        }

        /// <summary>
        /// 根据命名策略生成安全的文件名
        /// </summary>
        private async Task<string> GenerateSafeFileNameAsync(string originalFileName, int? customId, NamingStrategy namingStrategy, Dictionary<string, object>? additionalData)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            try
            {
                var safeFileName = namingStrategy switch
                {
                    NamingStrategy.UserAvatar => await GenerateUserAvatarNameAsync(extension, customId, additionalData),
                    NamingStrategy.OCBattleCharacter => await GenerateOCBattleCharacterNameAsync(extension, customId, additionalData),
                    NamingStrategy.OCGallery => GenerateOCGalleryName(extension, customId),
                    NamingStrategy.OCMainImage => GenerateOCMainImageName(extension, customId),
                    NamingStrategy.BattleRecord => GenerateBattleRecordName(extension, customId),
                    NamingStrategy.SimpleTimestamp => GenerateSimpleTimestampName(extension),
                    _ => GenerateDefaultName(extension, customId)
                };

                _logger.LogInformation("生成安全文件名: 策略={Strategy}, 原文件={OriginalFileName}, 生成={SafeFileName}",
                    namingStrategy, originalFileName, safeFileName);
                return safeFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成文件名失败，使用默认策略");
                return GenerateDefaultName(extension, customId);
            }
        }

        /// <summary>
        /// 用户头像命名：user_123_avatar.jpg
        /// </summary>
        private async Task<string> GenerateUserAvatarNameAsync(string extension, int? customId, Dictionary<string, object>? additionalData)
        {
            int userId = 0;

            // 优先从additionalData获取用户ID
            if (additionalData != null && additionalData.ContainsKey("UserId"))
            {
                userId = Convert.ToInt32(additionalData["UserId"]);
            }
            else if (customId.HasValue)
            {
                userId = customId.Value;
            }

            if (userId == 0)
            {
                throw new InvalidOperationException("无法获取用户ID，请确保已登录");
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"user_{userId}_avatar_{timestamp}{extension}";
        }

        /// <summary>
        /// 约战场角色命名：oc_123_角色名_18yo_20231201120000.jpg
        /// </summary>
        private async Task<string> GenerateOCBattleCharacterNameAsync(string extension, int? ocId, Dictionary<string, object>? additionalData)
        {
            if (!ocId.HasValue || ocId.Value == 0)
            {
                // 如果没有提供ocId，尝试从additionalData获取
                if (additionalData != null && additionalData.ContainsKey("OCId"))
                {
                    ocId = Convert.ToInt32(additionalData["OCId"]);
                }
                else
                {
                    throw new ArgumentException("OC角色ID不能为空");
                }
            }

            // 查询数据库获取角色信息
            var (characterAge, characterName) = await GetOCCharacterInfoAsync(ocId.Value);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            // 清理角色名称中的特殊字符
            var safeCharacterName = CleanFileName(characterName);

            return $"oc_{ocId.Value}_{safeCharacterName}_{characterAge}yo_{timestamp}{extension}";
        }

        /// <summary>
        /// OC主图命名：oc_123_main_20231201120000_5678.jpg
        /// </summary>
        private string GenerateOCMainImageName(string extension, int? ocId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"oc_{ocId ?? 0}_main_{timestamp}_{random}{extension}";
        }

        /// <summary>
        /// OC图库命名：oc_123_gallery_20231201120000_1234.jpg
        /// </summary>
        private string GenerateOCGalleryName(string extension, int? ocId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"oc_{ocId ?? 0}_gallery_{timestamp}_{random}{extension}";
        }

        /// <summary>
        /// 对战记录命名：battle_456_record_20231201120000.json
        /// </summary>
        private string GenerateBattleRecordName(string extension, int? battleId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"battle_{battleId ?? 0}_record_{timestamp}{extension}";
        }

        /// <summary>
        /// 简单时间戳命名：20231201120000123.jpg
        /// </summary>
        private string GenerateSimpleTimestampName(string extension)
        {
            return $"{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        }

        /// <summary>
        /// 默认命名策略：123_20231201120000_5678.jpg
        /// </summary>
        private string GenerateDefaultName(string extension, int? customId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            var idPart = customId.HasValue ? $"{customId.Value}_" : "";
            return $"{idPart}{timestamp}_{random}{extension}";
        }

        /// <summary>
        /// 从数据库查询OC角色信息
        /// </summary>
        private async Task<(int age, string name)> GetOCCharacterInfoAsync(int ocId)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChaiDbContext>();

            var ocInfo = await dbContext.OC_Infos
                .Where(o => o.id == ocId)
                .Select(o => new { o.age, o.name })
                .FirstOrDefaultAsync();

            if (ocInfo == null)
            {
                throw new ArgumentException($"未找到ID为 {ocId} 的OC角色");
            }

            _logger.LogInformation("查询到OC角色信息: ID={OCId}, 名称={Name}, 年龄={Age}", ocId, ocInfo.name, ocInfo.age);
            return (ocInfo.age, ocInfo.name);
        }

        /// <summary>
        /// 清理文件名中的非法字符
        /// </summary>
        private string CleanFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "unknown";

            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanName = new string(fileName
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray())
                .Replace(" ", "_")
                .Replace("__", "_")
                .Trim('_');

            if (string.IsNullOrEmpty(cleanName))
                return "unknown";

            // 限制长度
            return cleanName.Length > 30 ? cleanName.Substring(0, 30) : cleanName;
        }

        private async Task<bool> UploadToNasAsync(Stream fileStream, string relativePath, string fileExtension, long fileSize)
        {
            try
            {
                var nasEndpoint = GetNasEndpoint();
                var username = _configuration["NasCredentials:Username"];
                var password = _configuration["NasCredentials:Password"];

                _logger.LogInformation("=== 开始NAS上传 ===");
                _logger.LogInformation("NAS端点: {NasEndpoint}", nasEndpoint);
                _logger.LogInformation("用户名: {Username}", username);
                _logger.LogInformation("相对路径: {RelativePath}", relativePath);
                _logger.LogInformation("文件扩展名: {FileExtension}", fileExtension);
                _logger.LogInformation("文件大小: {FileSize} bytes", fileSize);

                if (string.IsNullOrEmpty(nasEndpoint) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("NAS配置缺失: 端点={NasEndpoint}, 用户={Username}, 密码长度={PasswordLength}",
                        nasEndpoint, username, password?.Length);
                    return false;
                }

                var uploadUrl = $"{nasEndpoint}/{relativePath.TrimStart('/')}";
                _logger.LogInformation("完整上传URL: {UploadUrl}", uploadUrl);

                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(300); // 5分钟超时

                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                if (fileStream.CanSeek)
                {
                    fileStream.Position = 0;
                    _logger.LogInformation("重置文件流位置到0");
                }

                using var content = new StreamContent(fileStream);
                content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileExtension));
                content.Headers.ContentLength = fileSize;

                _logger.LogInformation("开始发送PUT请求到NAS...");
                var response = await client.PutAsync(uploadUrl, content);

                _logger.LogInformation("NAS响应状态: {StatusCode}", response.StatusCode);
                _logger.LogInformation("NAS响应头: {Headers}", response.Headers.ToString());

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ NAS上传成功: {UploadUrl}", uploadUrl);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ NAS上传失败: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 上传到NAS失败 - 环境: {Environment}", _environment.EnvironmentName);
                _logger.LogError("内部异常: {InnerException}", ex.InnerException?.Message);
                return false;
            }
        }

        private string GetNasEndpoint()
        {
            var nasEndpoint = _configuration["StorageSettings:NasEndpoint"]?.TrimEnd('/');

            if (string.IsNullOrEmpty(nasEndpoint))
            {
                _logger.LogError("NasEndpoint未配置");
                throw new InvalidOperationException("NasEndpoint未配置");
            }

            _logger.LogInformation("获取NAS端点: {NasEndpoint}", nasEndpoint);
            return nasEndpoint;
        }

        private string GetContentType(string fileExtension)
        {
            var contentType = fileExtension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            _logger.LogInformation("文件扩展名 {FileExtension} 对应的Content-Type: {ContentType}", fileExtension, contentType);
            return contentType;
        }

        private bool ValidateRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogWarning("路径为空");
                return false;
            }
            if (path.Contains(".."))
            {
                _logger.LogWarning("路径包含非法字符 '..': {Path}", path);
                return false;
            }
            if (path.StartsWith("/") || path.StartsWith("\\"))
            {
                _logger.LogWarning("路径以斜杠开头: {Path}", path);
                return false;
            }
            if (path.Contains("://"))
            {
                _logger.LogWarning("路径包含协议标识: {Path}", path);
                return false;
            }
            if (Path.IsPathRooted(path))
            {
                _logger.LogWarning("路径是绝对路径: {Path}", path);
                return false;
            }

            var invalidChars = Path.GetInvalidPathChars();
            var hasInvalidChars = path.IndexOfAny(invalidChars) >= 0;

            if (hasInvalidChars)
            {
                _logger.LogWarning("路径包含非法字符: {Path}", path);
                return false;
            }

            _logger.LogInformation("路径验证通过: {Path}", path);
            return true;
        }
    }
}