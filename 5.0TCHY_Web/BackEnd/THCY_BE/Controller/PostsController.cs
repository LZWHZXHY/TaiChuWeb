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

        #region 评论接口（已增强以支持 multipart/form-data 与 application/json）

        // JSON 路由
        [HttpPost("{postId}/comments")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateCommentJson(int postId, [FromBody] CreateCommentDto dto)
            => await CreateCommentInternal(postId, dto, null);

        // Form (x-www-form-urlencoded / multipart/form-data) 路由
        [HttpPost("{postId}/comments")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> CreateCommentForm(int postId)
        {
            // 兼容前端使用 FormData 的情况
            var form = await Request.ReadFormAsync();
            var contentValue = form["Content"].FirstOrDefault() ?? form["content"].FirstOrDefault();
            var parentStr = form["ParentCommentId"].FirstOrDefault() ?? form["parentCommentId"].FirstOrDefault();
            int? parentId = null;
            if (int.TryParse(parentStr, out var pid)) parentId = pid;

            // 收集文件（若前端上传图片，但 Comment 实体不保存图片，先忽略或扩展后端以保存图片）
            var files = form.Files?.Count > 0 ? form.Files : null;

            var dto = new CreateCommentDto
            {
                Content = contentValue ?? string.Empty,
                ParentCommentId = parentId
            };

            return await CreateCommentInternal(postId, dto, files);
        }

        // 兼容 /reply 路径（JSON）
        [HttpPost("{postId}/reply")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateReplyAliasJson(int postId, [FromBody] CreateCommentDto dto)
            => await CreateCommentInternal(postId, dto, null);

        // 兼容 /reply 路径（form）
        [HttpPost("{postId}/reply")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> CreateReplyAliasForm(int postId)
        {
            var form = await Request.ReadFormAsync();
            var contentValue = form["Content"].FirstOrDefault() ?? form["content"].FirstOrDefault();
            var parentStr = form["ParentCommentId"].FirstOrDefault() ?? form["parentCommentId"].FirstOrDefault();
            int? parentId = null;
            if (int.TryParse(parentStr, out var pid)) parentId = pid;
            var files = form.Files?.Count > 0 ? form.Files : null;

            var dto = new CreateCommentDto
            {
                Content = contentValue ?? string.Empty,
                ParentCommentId = parentId
            };

            return await CreateCommentInternal(postId, dto, files);
        }

        // 公共内部实现：把原先 CreateComment 的逻辑移动到这里
        private async Task<IActionResult> CreateCommentInternal(int postId, CreateCommentDto dto, IFormFileCollection? files)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
                    return BadRequest(new { success = false, message = "评论内容不能为空" });

                var post = await _db.Posts.FirstOrDefaultAsync(p => p.id == postId && p.status == 0);
                if (post == null)
                    return NotFound(new { success = false, message = "帖子不存在或不可评论" });

                var userId = GetCurrentUserId();

                var comment = new Comment
                {
                    post_id = postId,
                    user_id = userId,
                    content = dto.Content.Trim(),
                    createTime = DateTime.UtcNow,
                    ParentCommentId = dto.ParentCommentId,
                    status = 0,
                    report_count = 0
                };

                _db.Comments.Add(comment);

                // 同步更新帖子的评论数量
                post.comment_count = (post.comment_count <= 0) ? 1 : post.comment_count + 1;
                post.updateTime = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                // 如果前端上传图片并且你计划把评论/回复支持图片：
                // 这里可以处理 files（保存并把路径存入数据库），目前先记录数量到日志
                if (files != null && files.Count > 0)
                {
                    _logger.LogInformation("收到 {Count} 附件 (未保存): postId={PostId}, commentId={CommentId}", files.Count, postId, comment.id);
                    // TODO: 如需保存评论图片，新增 CommentImages 表或在 Comment 中增加字段并实现保存逻辑
                }

                // 返回创建的评论（包含作者简要信息）
                // 注意：根据你的 PostsDbContext 命名，集合名称可能不同，请确保 _db.UserAccounts 存在或替换为实际名称
                var author = await _db.UserAccounts.Where(u => u.Id == userId)
                    .Select(u => new { u.Id, u.username })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = comment.id,
                        postId = comment.post_id,
                        parentCommentId = comment.ParentCommentId,
                        content = comment.content,
                        createTime = comment.createTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        author
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建评论失败");
                return StatusCode(500, new { success = false, message = "创建评论失败" });
            }
        }

        /// <summary>
        /// 获取指定帖子的评论（分页/分段）
        /// GET api/posts/{postId}/comments?lastId=xxx&pageSize=20
        /// </summary>
        [HttpGet("{postId}/comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int postId, [FromQuery] int? lastId = null, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize < 1 || pageSize > 200) pageSize = 20;

                var baseQuery = _db.Comments
                    .Where(c => c.post_id == postId && c.status == 0)
                    .OrderByDescending(c => c.createTime)
                    .AsQueryable();

                if (lastId.HasValue)
                {
                    var lastTime = await _db.Comments.Where(c => c.id == lastId.Value).Select(c => c.createTime).FirstOrDefaultAsync();
                    if (lastTime != default)
                    {
                        baseQuery = baseQuery.Where(c => c.createTime < lastTime);
                    }
                }

                var items = await baseQuery
                    .Take(pageSize)
                    .Select(c => new
                    {
                        c.id,
                        c.post_id,
                        c.ParentCommentId,
                        c.content,
                        c.createTime,
                        c.report_count,
                        author = new
                        {
                            id = c.useraccount.Id,
                            username = c.useraccount.username,
                            avatar = c.useraccount.userdata != null ? c.useraccount.userdata.logo : null
                        }
                    })
                    .ToListAsync();

                bool hasMore = false;
                if (items.Any())
                {
                    var lastTime = items.Last().createTime;
                    hasMore = await _db.Comments.AnyAsync(c => c.post_id == postId && c.status == 0 && c.createTime < lastTime);
                }

                return Ok(new
                {
                    success = true,
                    data = items,
                    pagination = new
                    {
                        lastId = items.LastOrDefault()?.id,
                        pageSize,
                        hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取评论列表失败");
                return StatusCode(500, new { success = false, message = "获取评论列表失败" });
            }
        }

        /// <summary>
        /// 删除评论（作者或管理员可以删除） - 软删除
        /// DELETE api/posts/{postId}/comments/{commentId}
        /// </summary>
        [HttpDelete("{postId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int postId, int commentId)
        {
            try
            {
                var comment = await _db.Comments.FirstOrDefaultAsync(c => c.id == commentId && c.post_id == postId);
                if (comment == null) return NotFound(new { success = false, message = "评论不存在" });

                var userId = GetCurrentUserId();
                var isAuthor = comment.user_id == userId;
                var isAdmin = User.IsInRole("Admin");

                if (!isAuthor && !isAdmin) return Forbid();

                // 软删除：将 status 标记为 1（已删除）
                comment.status = 1;
                comment.createTime = comment.createTime; // 保留原时间
                // 减少帖子关联的 comment_count
                var post = await _db.Posts.FirstOrDefaultAsync(p => p.id == postId);
                if (post != null && post.comment_count > 0) post.comment_count--;

                await _db.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除评论失败");
                return StatusCode(500, new { success = false, message = "删除评论失败" });
            }
        }

        #endregion

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

        // 将此方法添加/替换到你的 PostsController（评论接口区域）
        [HttpGet("{postId}/replies")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRepliesPaged(int postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 200) pageSize = 20;

                var baseQuery = _db.Comments
                    .Where(c => c.post_id == postId && c.status == 0)
                    .OrderByDescending(c => c.createTime)
                    .AsQueryable();

                var total = await baseQuery.CountAsync();

                var items = await baseQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        id = c.id,
                        authorId = c.user_id,
                        parentCommentId = c.ParentCommentId,   // <-- 返回父评论 ID（可为 null）
                        createTime = c.createTime,
                        content = c.content,
                        images = new List<string>(),
                        author = c.useraccount != null
                            ? new
                            {
                                id = c.useraccount.Id,
                                username = c.useraccount.username,
                                avatar = c.useraccount.userdata != null ? c.useraccount.userdata.logo : null
                            }
                            : null
                    })
                    .ToListAsync();

                var hasMore = page * pageSize < total;

                return Ok(new
                {
                    success = true,
                    data = items,
                    pagination = new
                    {
                        page,
                        pageSize,
                        total,
                        hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分页 replies 失败: postId={PostId}", postId);
                return StatusCode(500, new { success = false, message = "获取回复失败" });
            }
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
    /// 创建评论 DTO
    /// </summary>
    public class CreateCommentDto
    {
        public int? ParentCommentId { get; set; }
        public string Content { get; set; } = string.Empty;
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