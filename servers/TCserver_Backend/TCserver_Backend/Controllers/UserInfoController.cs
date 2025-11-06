using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TCserver_Backend.Data;
using WebDav;



namespace TCserver_Backend.Controllers
{
    // 字符串扩展方法类



    public static class StringExtensions
    {
        public static bool EndsWithAny(this string value, IEnumerable<string> suffixes)
        {
            if (value == null) return false;

            foreach (var suffix in suffixes)
            {
                if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UserInfoController : ControllerBase
    {
        private readonly RegisterContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserInfoController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        
        private readonly FunctionDbContext _functionDb;
        private readonly IMemoryCache _memoryCache;



        public UserInfoController(
            RegisterContext db,
            IConfiguration configuration,
            ILogger<UserInfoController> logger,
            IHttpClientFactory httpClientFactory,
            FunctionDbContext functionDb,
            IMemoryCache memoryCache  // 新增
            )
        {
            _db = db;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _functionDb = functionDb;
            _memoryCache = memoryCache;  // 新增
        }


        private string GetNasEndpoint()
        {
            // 优先用配置，环境/本地都一致
            return _configuration["StorageSettings:NasEndpoint"];
        }








        [HttpGet]
        public async Task<IActionResult> GetUserInfo([FromQuery] int userId)
        {
            try
            {
                // 使用 Select 明确只选择需要的列，避免加载 password_hash
                var user = await _db.UserAccounts
                    .Where(u => u.Id == userId)
                    .Select(u => new {
                        u.Id,
                        u.username,
                        u.email_address,
                        u.rank,
                        u.state
                    })
                    .FirstOrDefaultAsync();

                if (user == null) return NotFound(new { message = "用户不存在" });

                var data = await _db.UserDatas.FirstOrDefaultAsync(d => d.id == userId);
                if (data == null) return NotFound(new { message = "用户扩展信息不存在" });

                // 转换图片URL为代理URL
                //string proxyLogo = data.logo != null ? ConvertToProxyUrl(data.logo) : null;
                //string proxyBackground = data.background != null ? ConvertToProxyUrl(data.background) : null;

                // 组合数据返回
                return Ok(new
                {
                    id = user.Id,
                    username = user.username,
                    email = user.email_address,
                    rank = user.rank,
                    level = data.level,
                    exp = data.exp,
                    points = data.points,
                    title = data.title,
                    lastLogin = data.lastlogin,
                    logo = data.logo != null ?
                   $"/api/UserInfo/imageproxy?path={data.logo}" :
                   null,
                    background = "proxyBackground",
                    byd = data.byd   // <--- 新增这一行
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 将NAS URL转换为代理URL
        

        // 添加经验并检查升级
        [HttpPost("AddExp")]
        public async Task<IActionResult> AddExp([FromQuery] int userId, [FromQuery] int expAmount)
        {
            try
            {
                var userData = await _db.UserDatas.FirstOrDefaultAsync(d => d.id == userId);
                if (userData == null) return NotFound(new { message = "用户数据不存在" });

                // 添加经验
                userData.exp += expAmount;

                // 检查并执行多次升级可能性
                bool hasLeveledUp = false;
                while (userData.exp >= GetNextLevelExp(userData.level))
                {
                    int requiredExp = GetNextLevelExp(userData.level);
                    userData.exp -= requiredExp;
                    userData.level += 1;

                    hasLeveledUp = true;
                }

                // 保存更改
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    level = userData.level,
                    exp = userData.exp,
                    points = userData.points,
                    nextLevelExp = GetNextLevelExp(userData.level),
                    hasLeveledUp = hasLeveledUp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 检查用户是否可以升级
        [HttpPost("CheckLevelUp")]
        public async Task<IActionResult> CheckLevelUp([FromQuery] int userId)
        {
            try
            {
                var userData = await _db.UserDatas.FirstOrDefaultAsync(d => d.id == userId);
                if (userData == null) return NotFound(new { message = "用户数据不存在" });

                // 检查是否可以升级
                int nextLevelExp = GetNextLevelExp(userData.level);
                bool canLevelUp = userData.exp >= nextLevelExp;

                // 如果可以升级，执行升级逻辑
                bool hasLeveledUp = false;
                if (canLevelUp)
                {
                    hasLeveledUp = true;
                    userData.exp -= nextLevelExp;
                    userData.level += 1;
                    userData.points += 10; 

                    // 保存更改
                    await _db.SaveChangesAsync();
                }

                return Ok(new
                {
                    level = userData.level,
                    exp = userData.exp,
                    points = userData.points,
                    nextLevelExp = GetNextLevelExp(userData.level),
                    hasLeveledUp = hasLeveledUp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 计算升级所需经验值
        private int GetNextLevelExp(int level)
        {
            if (level <= 0) return 50;
            if (level <= 10) return 50 + level * 10;
            if (level <= 30) return 140 + (level - 10) * 10;
            if (level <= 50) return 210 + (level - 10) * 10;
            return 340 + (level - 30) * 30;
        }





        [HttpPost("UploadAvatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file, [FromQuery] int userId)
        {
            try
            {
                _logger.LogInformation($"开始处理头像上传请求，用户ID: {userId}");

                // 验证文件
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("文件为空或未选择文件");
                    return BadRequest(new { message = "未选择文件或文件为空" });
                }

                // 验证文件类型
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning($"不支持的文件类型: {fileExtension}");
                    return BadRequest(new { message = "不支持的图片格式，请上传 JPG、PNG、GIF 或 WEBP 格式的图片" });
                }

                // 验证文件大小 (最大2MB)
                const int maxFileSize = 2 * 1024 * 1024; // 2MB
                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning($"文件过大: {file.Length}字节");
                    return BadRequest(new { message = "图片大小不能超过 2MB" });
                }

                _logger.LogInformation($"验证通过，文件: {file.FileName} ({file.Length}字节)");

                // 获取用户信息
                var userData = await _db.UserDatas.FirstOrDefaultAsync(d => d.id == userId);
                if (userData == null)
                {
                    _logger.LogWarning($"用户数据不存在: {userId}");
                    return NotFound(new { message = "用户数据不存在" });
                }

                // 生成唯一文件名（使用用户ID）
                var fileName = $"{userId}{fileExtension}";
                var relativePath = $"avatars/{fileName}";

                // 获取NAS设置
                var nasEndpoint = GetNasEndpoint();
                var username = _configuration["NasCredentials:Username"];
                var password = _configuration["NasCredentials:Password"];
                var avatarsUrl = $"{nasEndpoint}/{relativePath}";

                _logger.LogInformation($"准备上传到: {avatarsUrl}");

                // 使用HttpClient直接上传
                using (var httpClient = new HttpClient())
                {
                    // 设置超时时间（60秒）
                    httpClient.Timeout = TimeSpan.FromSeconds(60);

                    // 添加基本认证
                    var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                    const int maxRetryAttempts = 3;
                    const int retryDelayMilliseconds = 1000;
                    int retryCount = 0;
                    bool uploadSuccess = false;

                    while (retryCount < maxRetryAttempts && !uploadSuccess)
                    {
                        try
                        {
                            using (var fileStream = file.OpenReadStream())
                            // 使用较小的缓冲区
                            using (var bufferedStream = new BufferedStream(fileStream, 8192))
                            using (var content = new StreamContent(bufferedStream))
                            {
                                // 设置内容类型
                                content.Headers.ContentType = new MediaTypeHeaderValue("image/" + fileExtension.TrimStart('.'));

                                _logger.LogInformation($"开始上传尝试 #{retryCount + 1}");

                                // 发送PUT请求
                                var response = await httpClient.PutAsync(avatarsUrl, content);

                                if (response.IsSuccessStatusCode)
                                {
                                    uploadSuccess = true;
                                    _logger.LogInformation($"上传成功: {relativePath}");
                                    break;
                                }

                                var responseContent = await response.Content.ReadAsStringAsync();
                                _logger.LogWarning($"上传尝试 #{retryCount + 1} 失败: {response.StatusCode} - {responseContent}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"上传尝试 #{retryCount + 1} 异常: {ex.Message}");

                            if (retryCount == maxRetryAttempts - 1)
                            {
                                _logger.LogError(ex, "所有重试尝试均失败");
                                return StatusCode(500, new
                                {
                                    message = "上传头像失败",
                                    error = ex.Message
                                });
                            }
                        }

                        // 等待后重试
                        await Task.Delay(retryDelayMilliseconds);
                        retryCount++;
                    }

                    if (!uploadSuccess)
                    {
                        return StatusCode(500, new
                        {
                            message = "上传头像失败，所有重试尝试均失败"
                        });
                    }
                }

                // 更新数据库中的头像路径
                userData.logo = relativePath;
                await _db.SaveChangesAsync();
                _logger.LogInformation($"数据库更新成功: {relativePath}");

                // 返回新的头像URL
                return Ok(new
                {
                    FileUrl = $"/api/UserInfo/imageproxy?path={relativePath}",
                    Message = "头像上传成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传头像过程中发生未处理的异常");
                return StatusCode(500, new
                {
                    message = "上传头像过程中发生错误",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("TestConnection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // 使用动态获取的NAS地址
                var nasEndpoint = GetNasEndpoint();
                var username = _configuration["NasCredentials:Username"];
                var password = _configuration["NasCredentials:Password"];

                _logger.LogInformation($"测试连接: {nasEndpoint}");
                _logger.LogInformation($"当前环境: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

                // 创建HttpClient
                using var httpClient = _httpClientFactory.CreateClient();

                // 添加基本认证
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // 设置超时时间
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                // 发送HEAD请求测试连接
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, nasEndpoint));

                return Ok(new
                {
                    statusCode = (int)response.StatusCode,
                    statusMessage = response.StatusCode.ToString(),
                    isSuccess = response.IsSuccessStatusCode,
                    nasEndpoint = nasEndpoint,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    message = response.IsSuccessStatusCode ?
                        "成功连接到NAS服务器" :
                        "无法连接到NAS服务器"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "测试连接时发生错误",
                    error = ex.Message,
                    errorType = ex.GetType().Name,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                });
            }
        }

        [HttpGet("imageproxy")]
        public async Task<IActionResult> ImageProxy([FromQuery] string path)
        {
            try
            {
                // 检查是否为默认头像请求
                if (path == "default-avatar")
                {
                    return await GetDefaultAvatar();
                }

                // ====== 本地内存缓存逻辑 ======
                // 缓存Key可根据业务自定义
                var cacheKey = $"imageproxy:{path}";
                if (_memoryCache.TryGetValue<byte[]>(cacheKey, out var cachedBytes))
                {
                    // 根据后缀猜测 content-type
                    var contentType = Path.GetExtension(path).ToLower() switch
                    {
                        ".webp" => "image/webp",
                        ".png" => "image/png",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        _ => "application/octet-stream"
                    };
                    return File(cachedBytes, contentType);
                }
                // ===========================

                // 使用动态获取的NAS地址
                var nasEndpoint = GetNasEndpoint();
                var username = _configuration["NasCredentials:Username"];
                var password = _configuration["NasCredentials:Password"];

                // 构建完整的图片URL
                string fullUrl = $"{nasEndpoint}/{path}";

                _logger.LogInformation($"图片代理请求: {fullUrl}");

                // 创建HttpClient
                using var httpClient = _httpClientFactory.CreateClient();

                // 添加基本认证
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // 设置超时时间
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // 发送GET请求获取图片
                var response = await httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"无法获取图片: {response.StatusCode}, URL: {fullUrl}");
                    // 如果头像不存在，返回默认头像
                    if (path.StartsWith("avatars/"))
                    {
                        return await GetDefaultAvatar();
                    }

                    return StatusCode((int)response.StatusCode, new
                    {
                        message = "无法获取图片",
                        statusCode = response.StatusCode,
                        path = fullUrl
                    });
                }

                // 读取响应内容
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                // 获取内容类型
                var contentTypeFromResponse = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                // ====== 写入本地缓存 ======
                _memoryCache.Set(cacheKey, fileBytes, TimeSpan.FromMinutes(10)); // 缓存10分钟
                                                                                 // =========================

                // 返回图片文件流
                return File(fileBytes, contentTypeFromResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "图片代理过程中发生错误");
                // 如果头像不存在，返回默认头像
                return await GetDefaultAvatar();
            }
        }

        private async Task<IActionResult> GetDefaultAvatar()
        {
            try
            {
                // 尝试从NAS获取默认头像
                var defaultAvatarPath = $"{GetNasEndpoint()}/avatars/default-avatar.jpg";

                // 使用工厂创建的HttpClient
                using var httpClient = _httpClientFactory.CreateClient();

                // 添加基本认证
                var username = _configuration["NasCredentials:Username"];
                var password = _configuration["NasCredentials:Password"];
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // 设置超时时间
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var response = await httpClient.GetAsync(defaultAvatarPath);

                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(fileBytes, "image/jpeg");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"无法从NAS获取默认头像: {ex.Message}");
            }

            // 使用嵌入式资源作为后备
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "TCserver_Backend.Resources.default-avatar.jpg";

                // 确保资源名称正确
                if (assembly.GetManifestResourceNames().Contains(resourceName))
                {
                    await using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        return File(memoryStream.ToArray(), "image/jpeg");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"加载嵌入式默认头像失败: {ex.Message}");
            }

            // 最终回退方案
            return NotFound("默认头像未找到");
        }

        [HttpGet("TestLocalConnection")]
        public async Task<IActionResult> TestLocalConnection()
        {
            try
            {
                string nasUrl = "http://192.168.50.225:7506/官网资源地址/test.jpg";
                string username = "readonly";
                string password = "ReadOnly";

                using var httpClient = _httpClientFactory.CreateClient();

                // 添加基本认证
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // 只发送HEAD请求测试连接
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, nasUrl));

                return Ok(new
                {
                    statusCode = (int)response.StatusCode,
                    statusMessage = response.StatusCode.ToString(),
                    isSuccess = response.IsSuccessStatusCode,
                    nasUrl = nasUrl,
                    message = response.IsSuccessStatusCode ?
                        "成功连接到NAS服务器" :
                        "无法连接到NAS服务器"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "测试连接时发生错误",
                    error = ex.Message,
                    errorType = ex.GetType().Name
                });
            }
        }


        [HttpGet("LevelRanking")]
        public async Task<IActionResult> GetRanking([FromQuery] string type = "level")
        {
            try
            {
                if (type == "active")
                {
                    // 1. 统计近7天活跃榜（如需总榜，把 since 改为 DateTime.MinValue 即可）
                    var since = DateTime.UtcNow.AddDays(-7);

                    // 2. 聚合活跃分
                    var activeQuery = await _functionDb.UserActivities
                        .Where(a => a.CreateTime >= since)
                        .GroupBy(a => a.UserId)
                        .Select(g => new { UserId = g.Key, Active = g.Sum(x => x.Score) })
                        .OrderByDescending(x => x.Active)
                        .Take(50)
                        .ToListAsync();

                    // 3. 补全用户信息（查主库）
                    var userIds = activeQuery.Select(x => x.UserId).ToList();
                    var userInfos = await _db.UserAccounts
                        .Where(u => userIds.Contains(u.Id))
                        .Join(_db.UserDatas, u => u.Id, d => d.id, (u, d) => new { u.Id, u.username, d.logo })
                        .ToListAsync();

                    var infoMap = userInfos.ToDictionary(x => x.Id);

                    var result = activeQuery.Select(x => new
                    {
                        id = x.UserId,
                        username = infoMap.ContainsKey(x.UserId) ? infoMap[x.UserId].username : "未知用户",
                        logo = infoMap.ContainsKey(x.UserId) && !string.IsNullOrEmpty(infoMap[x.UserId].logo)
                            ? $"/api/UserInfo/imageproxy?path={infoMap[x.UserId].logo}"
                            : "/api/UserInfo/imageproxy?path=default-avatar",
                        active = x.Active
                    }).ToList();

                    return Ok(result);
                }
                else
                {
                    // 等级榜
                    var query = from account in _db.UserAccounts
                                join data in _db.UserDatas on account.Id equals data.id
                                select new
                                {
                                    id = account.Id,
                                    username = account.username,
                                    logo = data.logo != null
                                        ? $"/api/UserInfo/imageproxy?path={data.logo}"
                                        : "/api/UserInfo/imageproxy?path=default-avatar",
                                    level = data.level,
                                    exp = data.exp
                                };

                    var result = await query
                        .OrderByDescending(u => u.level)
                        .ThenByDescending(u => u.exp)
                        .Take(50)
                        .ToListAsync();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("heartbeat")]
        public async Task<IActionResult> Heartbeat()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var user = await _db.UserDatas.FirstOrDefaultAsync(u => u.id == userId);
            if (user != null)
            {
                user.last_active_time = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return Ok();
        }






        [HttpGet("online")]
        public async Task<IActionResult> GetOnlineUsers()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-5); // 5分钟内活跃的认为在线

            var users = await _db.UserDatas
                .Where(u => u.last_active_time != null && u.last_active_time >= cutoff)
                .Join(_db.UserAccounts, d => d.id, a => a.Id, (d, a) => new
                {
                    id = d.id,
                    username = a.username,
                    level = d.level,
                    logo = !string.IsNullOrEmpty(d.logo)
                        ? $"/api/UserInfo/imageproxy?path={d.logo}"
                        : "/api/UserInfo/imageproxy?path=default-avatar"
                })
                .OrderByDescending(u => u.level)
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("fileproxy")]
        public async Task<IActionResult> FileProxy([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return BadRequest(new { message = "path参数不能为空" });

                var nasEndpoint = GetNasEndpoint();
                var username = _configuration["NasCredentials:Username"];
                var password = _configuration["NasCredentials:Password"];
                var fullUrl = $"{nasEndpoint}/{path}";

                _logger.LogInformation($"文件代理请求: {fullUrl}");

                using var httpClient = _httpClientFactory.CreateClient();
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                httpClient.Timeout = TimeSpan.FromSeconds(300); // 增加超时时间

                // 关键：用 ResponseHeadersRead 防止一次性读入大文件
                var response = await httpClient.GetAsync(fullUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"无法获取文件: {response.StatusCode}, URL: {fullUrl}, Body: {errorBody}");
                    return StatusCode((int)response.StatusCode, new { message = "无法获取文件", statusCode = response.StatusCode, path = fullUrl, errorBody });
                }

                var contentTypeFromResponse = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                var stream = await response.Content.ReadAsStreamAsync();
                return File(stream, contentTypeFromResponse, Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件代理过程中发生错误");
                return StatusCode(500, new { message = "文件代理发生错误", error = ex.Message });
            }
        }

        // 根据文件扩展名猜测 Content-Type
        private string GetContentTypeByExtension(string ext)
        {
            switch (ext.ToLower())
            {
                case ".txt": return "text/plain";
                case ".pdf": return "application/pdf";
                case ".zip": return "application/zip";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".webp": return "image/webp";
                case ".mp4": return "video/mp4";
                case ".mp3": return "audio/mpeg";
                // 其他常见类型可继续补充
                default: return "application/octet-stream";
            }
        }



        [HttpGet("SearchId/{id}")]
        public async Task<IActionResult> SearchId(int id)
        {

            _logger.LogError($"调用ID搜索");
            try
            {
                // 查询用户基本信息（不包含敏感信息）
                var user = await _db.UserAccounts
                    .Where(u => u.Id == id)
                    .Select(u => new {
                        u.Id,
                        u.username,
                        u.email_address,
                        u.rank,
                        u.state
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "未找到该用户" });
                }

                // 查询用户扩展数据
                var userData = await _db.UserDatas
                    .Where(d => d.id == id)
                    .Select(d => new {
                        d.level,
                        d.exp,
                        d.points,
                        d.title,
                        d.logo,
                        d.background,
                        d.byd
                    })
                    .FirstOrDefaultAsync();

                if (userData == null)
                {
                    return NotFound(new { message = "未找到用户扩展信息" });
                }

                // 组合返回数据
                return Ok(new
                {
                    id = user.Id,
                    username = user.username,
                    email = user.email_address,
                    rank = user.rank,
                    level = userData.level,
                    exp = userData.exp,
                    points = userData.points,
                    title = userData.title,
                    logo = userData.logo != null
                        ? $"/api/UserInfo/imageproxy?path={userData.logo}"
                        : "/api/UserInfo/imageproxy?path=default-avatar",
                    background = userData.background != null
                        ? $"/api/UserInfo/imageproxy?path={userData.background}"
                        : null,
                    byd = userData.byd
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"搜索用户ID时发生错误: {id}");
                return StatusCode(500, new { message = "搜索用户时发生错误", error = ex.Message });
            }
        }
















    }
}