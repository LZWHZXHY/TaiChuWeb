// Controllers/FileUploadController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THCY_BE.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using THCY_BE.DataBase;
using System.IO;

namespace THCY_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<FileUploadController> _logger;
        private readonly ChaiDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        // 本地存储路径配置
        private const string BASE_PHYSICAL_PATH = "/www/wwwroot/bianyuzhou.com/uploads";
        private const string BASE_WEB_PATH = "/uploads";

        public FileUploadController(
            IFileUploadService fileUploadService,
            ILogger<FileUploadController> logger,
            ChaiDbContext dbContext,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
            _dbContext = dbContext;
            _environment = environment;
            _configuration = configuration;
        }

        /// <summary>
        /// 上传用户头像
        /// </summary>
        [HttpPost("avatar")]
        public async Task<ActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                _logger.LogInformation("开始上传用户头像");

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 验证文件
                if (!ValidateImageFile(file))
                {
                    return BadRequest(new { success = false, message = "文件格式不支持或文件过大" });
                }

                // 从token获取用户ID
                var userId = GetCurrentUserIdFromToken();

                // 使用本地存储上传
                var result = await UploadAvatarToLocalStorageAsync(file, userId);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"头像上传失败: {result.ErrorMessage}"
                    });
                }

                // 更新用户头像路径到数据库
                await UpdateUserAvatarPath(userId, result.FilePath);

                _logger.LogInformation("用户头像上传成功: 用户ID={UserId}, 文件路径={FilePath}", userId, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "头像上传成功",
                    data = new
                    {
                        userId = userId,
                        filePath = result.FilePath,
                        fileUrl = BuildFileUrl(result.FilePath),
                        fileName = result.FileName,
                        fileSize = result.FileSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "头像上传失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 上传OC角色立绘图片（约战场角色）- 修改为包含年龄信息
        /// </summary>
        [HttpPost("oc-battle-image")]
        public async Task<ActionResult> UploadOCBattleImage(IFormFile file, [FromForm] int ocId)
        {
            try
            {
                _logger.LogInformation("开始上传OC角色立绘，OC ID: {OCId}", ocId);

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 验证文件
                if (!ValidateImageFile(file))
                {
                    return BadRequest(new { success = false, message = "文件格式不支持或文件过大" });
                }

                // 验证OC角色是否存在并获取年龄信息
                var ocInfo = await _dbContext.OC_Infos
                    .Where(o => o.id == ocId)
                    .Select(o => new { o.name, o.age, o.authorID, o.OC_image_url })
                    .FirstOrDefaultAsync();

                if (ocInfo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                // 删除旧图片（如果存在）
                if (!string.IsNullOrEmpty(ocInfo.OC_image_url))
                {
                    try
                    {
                        await DeleteLocalFileAsync(ocInfo.OC_image_url);
                        _logger.LogInformation("旧图片删除成功: {FilePath}", ocInfo.OC_image_url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "删除旧图片失败，继续上传新图片: {FilePath}", ocInfo.OC_image_url);
                    }
                }

                // === 修改：使用包含年龄信息的上传方法 ===
                var result = await UploadOCImageToLocalStorageAsync(file, ocInfo.authorID, ocId, ocInfo.age);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"OC角色立绘上传失败: {result.ErrorMessage}"
                    });
                }

                // 更新OC角色的图片路径到数据库
                await UpdateOCImagePath(ocId, result.FilePath);

                _logger.LogInformation("OC角色立绘上传成功: OC ID={OCId}, 文件路径={FilePath}", ocId, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "OC角色立绘上传成功",
                    data = new
                    {
                        ocId = ocId,
                        ocName = ocInfo.name,
                        age = ocInfo.age,
                        fileInfo = new
                        {
                            filePath = result.FilePath,
                            fileName = result.FileName,
                            fileUrl = BuildFileUrl(result.FilePath),
                            fileSize = result.FileSize
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OC角色立绘上传失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 上传OC图库图片（多图上传）- 修改为包含年龄信息
        /// </summary>
        [HttpPost("gallery")]
        public async Task<ActionResult> UploadGallery([FromForm] IFormFile[] files, [FromForm] int ocId)
        {
            try
            {
                _logger.LogInformation("开始上传OC图库图片，OC ID: {OCId}, 文件数量: {FileCount}", ocId, files?.Length ?? 0);

                if (files == null || files.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 验证OC角色是否存在并获取年龄信息
                var ocInfo = await _dbContext.OC_Infos
                    .Where(o => o.id == ocId)
                    .Select(o => new { o.authorID, o.age })
                    .FirstOrDefaultAsync();

                if (ocInfo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                var successUploads = new List<LocalUploadResult>();
                var failedUploads = new List<LocalUploadResult>();

                foreach (var file in files)
                {
                    // 验证文件
                    if (!ValidateImageFile(file))
                    {
                        failedUploads.Add(new LocalUploadResult
                        {
                            Success = false,
                            ErrorMessage = $"文件 {file.FileName} 格式不支持"
                        });
                        continue;
                    }

                    // === 修改：使用包含年龄信息的上传方法 ===
                    var result = await UploadGalleryImageToLocalStorageAsync(file, ocInfo.authorID, ocId, ocInfo.age);

                    if (result.Success)
                    {
                        successUploads.Add(result);
                    }
                    else
                    {
                        failedUploads.Add(result);
                    }
                }

                // 记录成功的文件到数据库
                await RecordGalleryImages(ocId, successUploads);

                _logger.LogInformation("OC图库上传完成: 成功 {SuccessCount} 个, 失败 {FailCount} 个",
                    successUploads.Count, failedUploads.Count);

                return Ok(new
                {
                    success = true,
                    message = $"图库上传完成: 成功 {successUploads.Count} 个, 失败 {failedUploads.Count} 个",
                    data = new
                    {
                        ocId = ocId,
                        successCount = successUploads.Count,
                        failCount = failedUploads.Count,
                        successFiles = successUploads.Select(f => new {
                            fileName = f.FileName,
                            fileUrl = BuildFileUrl(f.FilePath),
                            fileSize = f.FileSize,
                            filePath = f.FilePath
                        }),
                        failedFiles = failedUploads.Select(f => new {
                            fileName = f.FileName,
                            errorMessage = f.ErrorMessage
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OC图库上传失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "图库上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 上传OC主图 - 修改为包含年龄信息
        /// </summary>
        [HttpPost("oc-main-image")]
        public async Task<ActionResult> UploadOCMainImage(IFormFile file, [FromForm] int ocId)
        {
            try
            {
                _logger.LogInformation("开始上传OC主图，OC ID: {OCId}", ocId);

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 验证文件
                if (!ValidateImageFile(file))
                {
                    return BadRequest(new { success = false, message = "文件格式不支持或文件过大" });
                }

                // 验证OC角色是否存在并获取年龄信息
                var ocInfo = await _dbContext.OC_Infos
                    .Where(o => o.id == ocId)
                    .Select(o => new { o.authorID, o.age })
                    .FirstOrDefaultAsync();

                if (ocInfo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                // === 修改：使用包含年龄信息的上传方法 ===
                var result = await UploadOCMainImageToLocalStorageAsync(file, ocInfo.authorID, ocId, ocInfo.age);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"OC主图上传失败: {result.ErrorMessage}"
                    });
                }

                _logger.LogInformation("OC主图上传成功: OC ID={OCId}, 文件路径={FilePath}", ocId, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "OC主图上传成功",
                    data = new
                    {
                        ocId = ocId,
                        fileInfo = new
                        {
                            filePath = result.FilePath,
                            fileName = result.FileName,
                            fileUrl = BuildFileUrl(result.FilePath),
                            fileSize = result.FileSize
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OC主图上传失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 通用文件上传
        /// </summary>
        [HttpPost("general")]
        public async Task<ActionResult> UploadGeneralFile(
            IFormFile file,
            [FromForm] string folder = "uploads",
            [FromForm] int? customId = null)
        {
            try
            {
                _logger.LogInformation("开始通用文件上传，文件夹: {Folder}, 自定义ID: {CustomId}", folder, customId);

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                var userId = GetCurrentUserIdFromToken();
                var subFolder = customId.HasValue ? $"custom_{customId}" : "general";

                var result = await UploadToLocalStorageAsync(file, folder, userId, subFolder, "general");

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"文件上传失败: {result.ErrorMessage}"
                    });
                }

                _logger.LogInformation("通用文件上传成功: 文件夹={Folder}, 文件路径={FilePath}", folder, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "文件上传成功",
                    data = new
                    {
                        folder = folder,
                        fileInfo = new
                        {
                            filePath = result.FilePath,
                            fileName = result.FileName,
                            fileUrl = BuildFileUrl(result.FilePath),
                            fileSize = result.FileSize
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通用文件上传失败，文件夹: {Folder}", folder);
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        [HttpDelete("{*filePath}")]
        public async Task<ActionResult> DeleteFile(string filePath)
        {
            try
            {
                _logger.LogInformation("开始删除文件: {FilePath}", filePath);

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest(new { success = false, message = "文件路径不能为空" });
                }

                var result = await DeleteLocalFileAsync(filePath);

                if (result)
                {
                    _logger.LogInformation("文件删除成功: {FilePath}", filePath);

                    // 同时更新数据库中的相关记录
                    await ClearFileReferenceInDatabase(filePath);

                    return Ok(new
                    {
                        success = true,
                        message = "文件删除成功"
                    });
                }
                else
                {
                    _logger.LogWarning("文件删除失败: {FilePath}", filePath);
                    return BadRequest(new
                    {
                        success = false,
                        message = "文件删除失败"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件删除失败: {FilePath}", filePath);
                return StatusCode(500, new
                {
                    success = false,
                    message = "删除失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 获取文件URL
        /// </summary>
        [HttpGet("url")]
        [AllowAnonymous]
        public ActionResult GetFileUrl([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest(new { success = false, message = "文件路径不能为空" });
                }

                var fileUrl = BuildFileUrl(filePath);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        filePath = filePath,
                        fileUrl = fileUrl
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成文件URL失败: {FilePath}", filePath);
                return StatusCode(500, new
                {
                    success = false,
                    message = "生成URL失败: " + ex.Message
                });
            }
        }

        #region 本地存储核心方法（修改为包含年龄信息）

        /// <summary>
        /// 上传用户头像到本地存储
        /// </summary>
        private async Task<LocalUploadResult> UploadAvatarToLocalStorageAsync(IFormFile file, int userId)
        {
            return await UploadToLocalStorageAsync(file, "avatars", userId, "avatar", "avatar");
        }

        /// <summary>
        /// 上传OC角色立绘到本地存储 - 包含年龄信息
        /// </summary>
        private async Task<LocalUploadResult> UploadOCImageToLocalStorageAsync(IFormFile file, int userId, int ocId, int age)
        {
            return await UploadToLocalStorageAsync(file, "柴圈板块/太初约战场/人设图", userId, $"oc_{ocId}", "oc_image", age, ocId);
        }

        /// <summary>
        /// 上传OC图库图片到本地存储 - 包含年龄信息
        /// </summary>
        private async Task<LocalUploadResult> UploadGalleryImageToLocalStorageAsync(IFormFile file, int userId, int ocId, int age)
        {
            return await UploadToLocalStorageAsync(file, "柴圈板块/太初约战场/图库", userId, $"oc_{ocId}", "gallery", age, ocId);
        }

        /// <summary>
        /// 上传OC主图到本地存储 - 包含年龄信息
        /// </summary>
        private async Task<LocalUploadResult> UploadOCMainImageToLocalStorageAsync(IFormFile file, int userId, int ocId, int age)
        {
            return await UploadToLocalStorageAsync(file, "柴圈板块/太初约战场/主图", userId, $"oc_{ocId}", "oc_main", age, ocId);
        }

        /// <summary>
        /// 通用上传方法 - 支持包含年龄信息
        /// </summary>
        private async Task<LocalUploadResult> UploadToLocalStorageAsync(
            IFormFile file,
            string category,
            int userId,
            string subFolder,
            string fileType = "general",
            int? age = null,
            int? ocId = null)
        {
            try
            {
                if (!ValidateImageFile(file))
                {
                    return new LocalUploadResult { Success = false, ErrorMessage = "文件格式不支持或文件过大" };
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName;

                // === 修改：根据文件类型生成不同的文件名 ===
                switch (fileType.ToLower())
                {
                    case "avatar":
                        // 头像格式：avatar_{userId}_{时间戳}_{随机数}
                        fileName = GenerateFileName("avatar", userId.ToString(), null, fileExtension);
                        break;

                    case "oc_image":
                    case "gallery":
                    case "oc_main":
                        // OC相关图片格式：oc_{ocId}_{年龄}yo_{时间戳}_{随机数}
                        if (ocId.HasValue && age.HasValue)
                        {
                            fileName = GenerateFileName("oc", ocId.Value.ToString(), age.Value, fileExtension);
                        }
                        else
                        {
                            fileName = GenerateFileName("file", null, null, fileExtension);
                        }
                        break;

                    default:
                        // 通用格式：file_{时间戳}_{随机数}
                        fileName = GenerateFileName("file", null, null, fileExtension);
                        break;
                }

                // 构建相对路径
                var relativePath = Path.Combine(category, userId.ToString(), subFolder, fileName).Replace("\\", "/");

                // 构建物理路径
                var physicalPath = Path.Combine(BASE_PHYSICAL_PATH, relativePath);

                // 确保目录存在
                var directory = Path.GetDirectoryName(physicalPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("创建目录: {Directory}", directory);
                }

                // 保存文件
                using var stream = new FileStream(physicalPath, FileMode.Create);
                await file.CopyToAsync(stream);

                _logger.LogInformation("文件保存成功: {FileName}", fileName);
                _logger.LogInformation("文件路径: {PhysicalPath}", physicalPath);

                return new LocalUploadResult
                {
                    Success = true,
                    FileName = fileName,
                    FilePath = relativePath,
                    FileSize = file.Length,
                    PhysicalPath = physicalPath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "本地文件上传失败");
                return new LocalUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// 生成包含年龄信息的文件名
        /// </summary>
        private string GenerateFileName(string prefix, string id, int? age, string extension)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);

            if (!string.IsNullOrEmpty(id) && age.HasValue)
            {
                // OC相关文件：oc_{id}_{年龄}yo_{时间戳}_{随机数}
                return $"{prefix}_{id}_{age}yo_{timestamp}_{random}{extension}";
            }
            else if (!string.IsNullOrEmpty(id))
            {
                // 有ID但无年龄：{prefix}_{id}_{时间戳}_{随机数}
                return $"{prefix}_{id}_{timestamp}_{random}{extension}";
            }
            else
            {
                // 通用文件：{prefix}_{时间戳}_{随机数}
                return $"{prefix}_{timestamp}_{random}{extension}";
            }
        }

        /// <summary>
        /// 删除本地文件
        /// </summary>
        private async Task<bool> DeleteLocalFileAsync(string relativePath)
        {
            try
            {
                if (string.IsNullOrEmpty(relativePath))
                    return false;

                var physicalPath = Path.Combine(BASE_PHYSICAL_PATH, relativePath);

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                    _logger.LogInformation("删除本地文件: {PhysicalPath}", physicalPath);
                    return true;
                }

                _logger.LogWarning("文件不存在: {PhysicalPath}", physicalPath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除本地文件失败: {RelativePath}", relativePath);
                return false;
            }
        }

        /// <summary>
        /// 构建正确的文件访问URL
        /// </summary>
        private string BuildFileUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            // 开发环境使用本地地址，生产环境使用域名
            var baseUrl = _environment.IsDevelopment()
                ? "https://localhost:44359"
                : _configuration["AppSettings:ProductionUrl"] ?? "https://bianyuzhou.com";

            // 移除路径中的baseUrl重复部分
            relativePath = relativePath.Replace(BASE_PHYSICAL_PATH, "")
                                      .Replace("\\", "/")
                                      .TrimStart('/');

            // 构建完整URL
            var fullUrl = $"{baseUrl.TrimEnd('/')}/{BASE_WEB_PATH.TrimStart('/')}/{relativePath.TrimStart('/')}";

            // 确保URL格式正确
            fullUrl = fullUrl.Replace("//", "/")
                            .Replace(":/", "://");

            _logger.LogInformation("构建文件URL: {FullUrl}", fullUrl);
            return fullUrl;
        }

        /// <summary>
        /// 验证图片文件
        /// </summary>
        private bool ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // 检查文件大小（5MB）
            if (file.Length > 5 * 1024 * 1024)
                return false;

            // 检查文件类型
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return false;

            return true;
        }

        #endregion

        #region 辅助方法

        private int GetCurrentUserIdFromToken()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return userId;
            }
            return 1;
        }

        private async Task UpdateUserAvatarPath(int userId, string filePath)
        {
            // 根据你的用户表结构实现
            // 示例：更新用户表的头像字段
            _logger.LogInformation("更新用户头像路径: 用户ID={UserId}, 路径={FilePath}", userId, filePath);
        }

        private async Task UpdateOCImagePath(int ocId, string filePath)
        {
            var ocInfo = await _dbContext.OC_Infos.FindAsync(ocId);
            if (ocInfo != null)
            {
                ocInfo.OC_image_url = filePath;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("更新OC图片路径: OC ID={OCId}, 路径={FilePath}", ocId, filePath);
            }
        }

        private async Task RecordGalleryImages(int ocId, List<LocalUploadResult> uploads)
        {
            // 记录图库图片到数据库
            // 根据你的图库表结构实现
            _logger.LogInformation("记录图库图片: OC ID={OCId}, 图片数量={Count}", ocId, uploads.Count);
        }

        private async Task ClearFileReferenceInDatabase(string filePath)
        {
            // 清除数据库中对该文件的引用
            _logger.LogInformation("清除文件引用: {FilePath}", filePath);
        }

        #endregion
    }

    /// <summary>
    /// 本地上传结果类
    /// </summary>
    public class LocalUploadResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public string? PhysicalPath { get; set; }
    }
}