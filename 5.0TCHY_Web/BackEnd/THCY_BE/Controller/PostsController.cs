using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using THCY_BE.DataBase;
using THCY_BE.Models.Posts;
using THCY_BE.Models.UserDate;
using System.IO;

namespace THCY_BE.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ILogger<PostsController> _logger;
        private readonly PostsDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        // 本地存储路径配置
        private const string BASE_PHYSICAL_PATH = "/www/wwwroot/bianyuzhou.com/uploads";
        private const string BASE_WEB_PATH = "/uploads";
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

        public PostsController(
            ILogger<PostsController> logger,
            PostsDbContext db,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _logger = logger;
            _db = db;
            _environment = environment;
            _configuration = configuration;
        }

        /// <summary>
        /// 创建帖子并上传图片（一次性操作，不可修改）
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)
        {
            try
            {
                _logger.LogInformation("=== 开始创建帖子 ===");
                _logger.LogInformation("帖子标题: {Title}, 作者ID: {AuthorId}", dto.Title, GetCurrentUserId());

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("数据验证失败: {Errors}", string.Join(", ", errors));
                    return BadRequest(new
                    {
                        success = false,
                        message = "数据验证失败",
                        errors = errors
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "帖子标题不能为空"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Content))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "帖子内容不能为空"
                    });
                }

                var userId = GetCurrentUserId();

                // 检查图片数量限制（可选）
                if (dto.Images != null && dto.Images.Count > 10)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "最多只能上传10张图片"
                    });
                }

                var newPost = new Post
                {
                    post_title = dto.Title.Trim(),
                    content = dto.Content.Trim(),
                    author_id = userId,
                    post_type = dto.PostType,
                    createTime = DateTime.UtcNow,
                    updateTime = DateTime.UtcNow,
                    status = 0, // 正常
                    view_count = 0,
                    like_count = 0,
                    comment_count = 0,
                    report = 0,
                    images = null // 初始化为null
                };

                _db.Posts.Add(newPost);
                await _db.SaveChangesAsync();

                _logger.LogInformation("帖子记录创建成功，ID: {PostId}", newPost.id);

                // 处理图片上传（一次性操作）
                List<ImageUploadResult> imageResults = new List<ImageUploadResult>();
                if (dto.Images != null && dto.Images.Count > 0)
                {
                    foreach (var image in dto.Images)
                    {
                        var uploadResult = await UploadImageToLocalStorageAsync(
                            image,
                            userId,
                            newPost.id,
                            imageResults.Count + 1
                        );

                        if (uploadResult.Success)
                        {
                            imageResults.Add(uploadResult);
                            _logger.LogInformation("图片上传成功: {FileName}", uploadResult.FileName);
                        }
                        else
                        {
                            _logger.LogWarning("图片上传失败: {ErrorMessage}", uploadResult.ErrorMessage);
                            // 单张图片上传失败不影响其他图片
                        }
                    }

                    // 更新帖子图片信息（一次性设置，不可修改）
                    if (imageResults.Any())
                    {
                        newPost.images = string.Join(",", imageResults.Select(img => img.FilePath));
                        newPost.updateTime = DateTime.UtcNow;
                        await _db.SaveChangesAsync();
                    }
                }

                _logger.LogInformation("✅ 帖子创建成功，ID: {Id}", newPost.id);

                return Ok(new
                {
                    success = true,
                    message = "帖子发布成功",
                    data = new
                    {
                        postId = newPost.id,
                        title = newPost.post_title,
                        content = newPost.content,
                        postType = newPost.post_type,
                        authorId = newPost.author_id,
                        imageCount = imageResults.Count,
                        images = imageResults.Select(img => new
                        {
                            sequence = img.Sequence,
                            fileName = img.FileName,
                            relativePath = img.FilePath,
                            fullUrl = BuildLocalImageUrl(img.FilePath),
                            fileSize = img.FileSize
                        }),
                        timestamps = new
                        {
                            createTime = newPost.createTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            updateTime = newPost.updateTime.ToString("yyyy-MM-dd HH:mm:ss")
                        },
                        note = "帖子内容不可修改，请谨慎发布"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 帖子创建失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "服务器内部错误: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 获取帖子列表（分段加载）
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPosts(
            [FromQuery] int? lastId = null,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? postType = null)
        {
            try
            {
                // 验证参数
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                // 构建基础查询
                IQueryable<Post> query = _db.Posts
                    .Where(p => p.status == 0) // 只获取状态正常的帖子
                    .Include(p => p.useraccount)
                    .Include(p => p.userdata);

                // 应用帖子类型筛选
                if (postType.HasValue)
                {
                    query = query.Where(p => p.post_type == postType.Value);
                }

                // 应用分段加载条件
                if (lastId.HasValue)
                {
                    // 获取上一批最后一个帖子的创建时间
                    var lastPostTime = await _db.Posts
                        .Where(p => p.id == lastId.Value)
                        .Select(p => p.createTime)
                        .FirstOrDefaultAsync();

                    if (lastPostTime != default)
                    {
                        // 获取创建时间小于上一批最后一个帖子的记录
                        query = query.Where(p => p.createTime < lastPostTime);
                    }
                }

                // 获取帖子列表（只获取基础数据）
                var posts = await query
                    .OrderByDescending(p => p.createTime)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.id,
                        p.post_title,
                        p.content,
                        p.createTime,
                        p.updateTime,
                        p.view_count,
                        p.like_count,
                        p.comment_count,
                        author = new
                        {
                            id = p.useraccount.Id,
                            username = p.useraccount.username,
                            avatar = p.userdata.logo
                        },
                        p.post_type,
                        p.images // 原始图片路径
                    })
                    .ToListAsync();

                // 在内存中处理图片URL（避免表达式树问题）
                var processedPosts = posts.Select(p => new
                {
                    p.id,
                    p.post_title,
                    excerpt = p.content.Length > 100 ? p.content.Substring(0, 100) + "..." : p.content,
                    p.createTime,
                    p.updateTime,
                    p.view_count,
                    p.like_count,
                    p.comment_count,
                    p.author,
                    p.post_type,
                    // 在内存中处理图片URL
                    images = !string.IsNullOrEmpty(p.images)
                        ? p.images.Split(',').Select(imgPath => BuildLocalImageUrl(imgPath)).ToList()
                        : new List<string>()
                }).ToList();

                // 判断是否还有更多数据
                bool hasMore = false;
                if (posts.Any())
                {
                    var lastPostCreateTime = posts.Last().createTime;
                    hasMore = await query
                        .Where(p => p.createTime < lastPostCreateTime)
                        .AnyAsync();
                }

                return Ok(new
                {
                    success = true,
                    data = processedPosts,
                    pagination = new
                    {
                        lastId = posts.LastOrDefault()?.id,
                        pageSize,
                        hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取帖子列表失败");
                return StatusCode(500, new { success = false, message = "获取帖子列表失败" });
            }
        }

        /// <summary>
        /// 获取单个帖子详情
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPost(int id)
        {
            try
            {
                // 先获取基础数据
                var post = await _db.Posts
                    .Where(p => p.id == id && p.status == 0)
                    .Include(p => p.useraccount)
                    .Include(p => p.userdata)
                    .Select(p => new
                    {
                        p.id,
                        p.post_title,
                        p.content,
                        p.createTime,
                        p.updateTime,
                        p.view_count,
                        p.like_count,
                        p.comment_count,
                        author = new
                        {
                            id = p.useraccount.Id,
                            username = p.useraccount.username,
                            avatar = p.userdata.logo,
                            level = p.userdata.level
                        },
                        p.post_type,
                        p.images, // 原始图片路径
                        p.videos
                    })
                    .FirstOrDefaultAsync();

                if (post == null)
                {
                    return NotFound(new { success = false, message = "帖子不存在或已被删除" });
                }

                // 在内存中处理图片信息（避免表达式树问题）
                var processedPost = new
                {
                    post.id,
                    post.post_title,
                    post.content,
                    post.createTime,
                    post.updateTime,
                    post.view_count,
                    post.like_count,
                    post.comment_count,
                    post.author,
                    post.post_type,
                    // 在内存中处理图片信息
                    images = !string.IsNullOrEmpty(post.images)
                        ? post.images.Split(',').Select(imgPath => new ImageInfo
                        {
                            url = BuildLocalImageUrl(imgPath),
                            fileName = Path.GetFileName(imgPath)
                        }).ToList()
                        : new List<ImageInfo>(),
                    post.videos
                };

                return Ok(new { success = true, data = processedPost });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取帖子详情失败，ID: {id}");
                return StatusCode(500, new { success = false, message = "获取帖子详情失败" });
            }
        }

        #region 图片处理核心方法

        /// <summary>
        /// 上传图片到本地存储（一次性操作）
        /// 格式：uploads/太虚坛板块/帖子图片/{userid}/{帖子id}_{图片序列}_{时间戳}
        /// </summary>
        private async Task<ImageUploadResult> UploadImageToLocalStorageAsync(
            IFormFile file,
            int userId,
            int postId,
            int sequence)
        {
            try
            {
                if (!ValidateImageFile(file))
                {
                    return new ImageUploadResult { Success = false, ErrorMessage = "文件格式不支持或文件过大" };
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // 生成文件名：{帖子id}_{图片序列}_{时间戳}
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"{postId}_{sequence}_{timestamp}{fileExtension}";

                // 构建路径
                var relativePath = Path.Combine("太虚坛板块", "帖子图片", userId.ToString(), fileName)
                    .Replace("\\", "/");

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

                _logger.LogInformation("✅ 图片保存成功: {FileName}", fileName);

                return new ImageUploadResult
                {
                    Success = true,
                    FileName = fileName,
                    FilePath = relativePath,
                    FileSize = file.Length,
                    PhysicalPath = physicalPath,
                    Sequence = sequence
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 本地图片上传失败");
                return new ImageUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// 验证图片文件
        /// </summary>
        private bool ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // 检查文件大小
            if (file.Length > MAX_FILE_SIZE)
                return false;

            // 检查文件类型
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return false;

            return true;
        }

        /// <summary>
        /// 构建本地图片访问URL
        /// </summary>
        private string? BuildLocalImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                _logger.LogWarning("❌ 相对路径为空");
                return null;
            }

            try
            {
                // 开发环境使用本地地址，生产环境使用域名
                var baseUrl = _environment.IsDevelopment()
                    ? _configuration["AppSettings:DevelopmentUrl"] ?? "https://localhost:44359"
                    : _configuration["AppSettings:ProductionUrl"] ?? "https://bianyuzhou.com";

                baseUrl = baseUrl.TrimEnd('/');

                // 清理路径
                var cleanPath = relativePath.TrimStart('/').Trim();

                if (string.IsNullOrEmpty(cleanPath))
                {
                    _logger.LogWarning("❌ 清理后的路径为空");
                    return null;
                }

                // 对路径的每一部分进行URL编码
                var pathSegments = cleanPath.Split('/');
                var encodedSegments = pathSegments.Select(segment =>
                {
                    try
                    {
                        return Uri.EscapeDataString(segment);
                    }
                    catch
                    {
                        // 如果编码失败，使用原始片段
                        return segment;
                    }
                }).ToArray();

                var encodedPath = string.Join("/", encodedSegments);

                // 构建完整URL
                var fullUrl = $"{baseUrl}/{BASE_WEB_PATH.TrimStart('/')}/{encodedPath}";

                // 修正URL格式
                fullUrl = fullUrl.Replace("\\", "/")
                                .Replace("//", "/", StringComparison.Ordinal)
                                .Replace("https:/", "https://", StringComparison.Ordinal)
                                .Replace("http:/", "http://", StringComparison.Ordinal);

                _logger.LogInformation("🔗 图片URL构建: 输入={RelativePath}, 输出={FullUrl}", relativePath, fullUrl);

                return fullUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 构建图片URL失败: {RelativePath}", relativePath);

                // 降级方案：返回未编码的URL
                try
                {
                    var baseUrl = _environment.IsDevelopment()
                        ? _configuration["AppSettings:DevelopmentUrl"] ?? "https://localhost:44359"
                        : _configuration["AppSettings:ProductionUrl"] ?? "https://bianyuzhou.com";

                    var fallbackUrl = $"{baseUrl.TrimEnd('/')}/{BASE_WEB_PATH.TrimStart('/')}/{relativePath.TrimStart('/')}";
                    _logger.LogWarning("🔄 使用降级URL: {FallbackUrl}", fallbackUrl);
                    return fallbackUrl;
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion

        #region 辅助方法

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return userId;
            }

            _logger.LogWarning("无法从token获取用户ID，声明: {UserIdClaim}", userIdClaim);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "未知用户";
            _logger.LogWarning("当前用户: {UserName}", userName);
            return 1; // 默认用户ID
        }

        #endregion
    }

    /// <summary>
    /// 创建帖子DTO
    /// </summary>
    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int PostType { get; set; } = 0;
        public List<IFormFile>? Images { get; set; }
    }

    /// <summary>
    /// 图片上传结果
    /// </summary>
    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public string? PhysicalPath { get; set; }
        public int Sequence { get; set; }
    }

    /// <summary>
    /// 图片信息类
    /// </summary>
    public class ImageInfo
    {
        public string? url { get; set; }
        public string? fileName { get; set; }
    }
}