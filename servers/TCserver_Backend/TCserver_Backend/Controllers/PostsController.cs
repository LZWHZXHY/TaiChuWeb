using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TCserver_Backend.Data;
using TCserver_Backend.Dtos;
using TCserver_Backend.Models;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly PostsDbContext _db;
        private readonly ILogger<PostsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly RegisterContext _registerContext;
        private readonly FunctionDbContext _functionDbContext;

        public PostsController(
            RegisterContext registerContext,
            PostsDbContext db,
            ILogger<PostsController> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment env,
            FunctionDbContext functionDbContext)
        {
            _registerContext = registerContext;
            _db = db;
            _logger = logger;
            _configuration = configuration;  
            _httpClientFactory = httpClientFactory;
            _env = env;
            _functionDbContext = functionDbContext;
        }

        // 获取NAS地址的方法
        private string GetNasEndpoint()
        {
            if (_env.IsProduction())
            {
                return "http://100.102.164.127:7506/官网资源地址";
            }
            return _configuration["StorageSettings:NasEndpoint"];
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetPosts(
    string? keyword = null,
    int? postType = null,
    string? sort = "latest")
        {
            int? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            }

            // 1. 构建查询
            var query = _db.Posts
                .Where(p => p.status == 0)
                .Include(p => p.useraccount)
                .Include(p => p.userdata)
                .AsQueryable();

            // 2. 关键词搜索
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.post_title.Contains(keyword) || p.content.Contains(keyword));
            }

            // 3. 类型过滤
            if (postType.HasValue)
            {
                query = query.Where(p => p.post_type == postType.Value);
            }

            // 4. 排序
            if (sort == "likes")
            {
                query = query.OrderByDescending(p => p.like_count).ThenByDescending(p => p.createTime);
            }
            else // 默认最新
            {
                query = query.OrderByDescending(p => p.createTime);
            }

            var posts = await query.ToListAsync();

            // 5. 获取本用户点赞过的帖子ID
            HashSet<int> likedPostIds = new();
            if (currentUserId != null)
            {
                likedPostIds = _registerContext.PostsLike
                    .Where(l => l.UserId == currentUserId.Value)
                    .Select(l => l.PostId)
                    .ToHashSet();
            }

            // 6. 性能优化：一次性查询所有评论数
            var postIds = posts.Select(p => p.id).ToList();
            var commentCounts = _db.Comments
                .Where(c => postIds.Contains(c.post_id))
                .GroupBy(c => c.post_id)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionary(g => g.PostId, g => g.Count);

            // 7. 组装DTO
            var dtoList = posts.Select(post => new PostsViewDto
            {
                Id = post.id,
                Title = post.post_title,
                Content = post.content,
                PostType = post.post_type,
                Images = string.IsNullOrEmpty(post.images)
                    ? new List<string>()
                    : post.images.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Videos = string.IsNullOrEmpty(post.videos)
                    ? new List<string>()
                    : post.videos.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Author = post.useraccount?.username ?? "",
                AuthorAvatar = post.userdata?.logo,
                CreateTime = post.createTime.ToString("o"),
                LikeCount = post.like_count,
                LikedByMe = currentUserId != null && likedPostIds.Contains(post.id),
                CommentCount = commentCounts.ContainsKey(post.id) ? commentCounts[post.id] : 0
                // 可加更多字段
            }).ToList();

            return Ok(dtoList);
        }

        /// <summary>
        /// 修改后的发帖接口：等级、发帖次数、图片数量/大小限制、自动加经验
        /// [FromForm] 支持文件上传
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                // 查用户数据（等级、经验等）
                var user = await _registerContext.UserDatas.FindAsync(currentUserId);
                if (user == null)
                {
                    _logger.LogWarning($"用户不存在: {currentUserId}");
                    return NotFound("用户不存在");
                }

                // 使用北京时间（东八区）
                var chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                var nowInChina = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, chinaTimeZone);
                var todayInChina = nowInChina.Date;
                var tomorrowInChina = todayInChina.AddDays(1);

                int todayPostCount = await _registerContext.Posts
                    .Where(p => p.author_id == currentUserId &&
                                p.createTime >= todayInChina &&
                                p.createTime < tomorrowInChina)
                    .CountAsync();







               

                // 0-1级：每天只能发1贴，每贴只能1张图
                if (user.level >= 0 && user.level <= 1)
                {
                    if (todayPostCount >= 1)
                        return BadRequest("0-1级用户每天只能发1个帖子。");
                    if (dto.Images != null && dto.Images.Count > 1)
                        return BadRequest("0-1级用户每贴最多只能上传1张图片。");
                }
                // 2-4级：每天只能发3贴，每贴最多3张图
                else if (user.level >= 2 && user.level <= 4)
                {
                    if (todayPostCount >= 3)
                        return BadRequest("2-4级用户每天只能发3个帖子。");
                    if (dto.Images != null && dto.Images.Count > 3)
                        return BadRequest("2-4级用户每贴最多只能上传3张图片。");
                }
                // 5级及以上：每天最多发5贴，每贴最多5张图
                else if (user.level >= 5)
                {
                    if (todayPostCount >= 5)
                        return BadRequest("5级及以上用户每天只能发5个帖子。");
                    if (dto.Images != null && dto.Images.Count > 5)
                        return BadRequest("5级及以上用户每贴最多只能上传5张图片。");
                }

                // 图片数量限制
                int maxImages = user.level >= 5 ? 3 : 1;
                if (dto.Images != null && dto.Images.Count > maxImages)
                    return BadRequest($"您的等级最多只能上传{maxImages}张图片。");

                // 新建帖子
                var newPost = new Posts
                {
                    post_title = dto.Title,
                    content = dto.Content,
                    post_type = dto.PostType,
                    author_id = currentUserId,
                    createTime = DateTime.UtcNow,
                    updateTime = DateTime.UtcNow,
                    status = 0,
                    view_count = 0,
                    like_count = 0,
                    comment_count = 0,
                    report = 0,
                    images = dto.Images != null ? string.Join(";", dto.Images) : null,
                    videos = dto.Videos != null ? string.Join(";", dto.Videos) : null
                };
                _registerContext.Posts.Add(newPost);

                // 发帖加经验
                AddExpAndLevelUp(user, 10);
                _logger.LogInformation($"用户{user.id}发帖加经验后，当前经验{user.exp}，等级{user.level}");
          
                await _registerContext.SaveChangesAsync();


                // 发帖加活跃度
                var activity = new UserActivity
                {
                    UserId = currentUserId,
                    ActivityType = (int)ActivityType.Post, // 建议加一个Post类型，或用你自己的类型枚举
                    Score = 3, // 发帖可给3分，分值你可以自定义
                    CreateTime = DateTime.UtcNow
                };
                _functionDbContext.UserActivities.Add(activity);
                await _functionDbContext.SaveChangesAsync();



                return Ok(new
                {
                    postId = newPost.id,
                    message = "发帖成功，已获得10经验！",
                    level = user.level,
                    exp = user.exp,
                    nextLevelExp = GetNextLevelExp(user.level)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"创建帖子失败: {ex}");
                return StatusCode(500, "服务器内部错误");
            }
        }

        private void AddExpAndLevelUp(userdata user, int expAmount)
        {
            user.exp += expAmount;
            while (user.exp >= GetNextLevelExp(user.level))
            {
                user.exp -= GetNextLevelExp(user.level);
                user.level += 1;
                user.points += 10; // 每次升级加10分
            }
        }
        private int GetNextLevelExp(int level)
        {
            if (level <= 0) return 50;
            if (level <= 10) return 50 + level * 10;
            if (level <= 30) return 140 + (level - 10) * 10;
            return 340 + (level - 30) * 30;
        }










        [HttpPost("UploadMedia")]
        [Authorize]
        public async Task<IActionResult> UploadMedia(
    [FromForm] List<IFormFile> images,
    [FromForm] List<IFormFile> videos,
    [FromQuery] int userId)
        {
            var uploadedImages = new List<string>();
            var uploadedVideos = new List<string>();

            // 获取用户等级
            var user = await _registerContext.UserDatas.FindAsync(userId);
            if (user == null)
                return BadRequest("用户不存在");

            // 限制参数，按等级分配
            int maxImages = 1, maxVideos = 0;
            long maxImageSize = 3 * 1024 * 1024; // 默认3MB
            long maxVideoSize = 20 * 1024 * 1024; // 默认20MB

            if (user.level >= 2 && user.level <= 4)
            {
                maxImages = 3;
                maxVideos = 1;
                maxImageSize = 3 * 1024 * 1024;
                maxVideoSize = 20 * 1024 * 1024;
            }
            else if (user.level >= 5)
            {
                maxImages = 5;
                maxVideos = 2;
                maxImageSize = 5 * 1024 * 1024;
                maxVideoSize = 30 * 1024 * 1024;
            }

            // 图片数量限制
            if (images != null && images.Count > maxImages)
                return BadRequest($"您的等级最多每贴只能上传{maxImages}张图片。");

            // 视频数量限制
            if (videos != null && videos.Count > maxVideos)
                return BadRequest($"您的等级最多每贴只能上传{maxVideos}个视频。");

            // 0-1级不能上传视频
            if (user.level <= 1 && videos != null && videos.Count > 0)
                return BadRequest("0~1级用户不能上传视频。");

            // 上传图片
            if (images != null && images.Count > 0)
            {
                int imgSeq = 1;
                foreach (var file in images)
                {
                    if (file.Length > 0)
                    {
                        // 单张图片大小限制
                        if (file.Length > maxImageSize)
                            return BadRequest($"第{imgSeq}张图片超过大小限制（最大{maxImageSize / 1024 / 1024}MB）");

                        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        if (!Array.Exists(allowedExtensions, e => e == ext))
                            return BadRequest($"第{imgSeq}张图片格式不支持，仅支持 JPG/JPEG/PNG/GIF/WEBP");

                        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{imgSeq}{ext}";
                        var relativePath = $"picture/{fileName}";
                        bool ok = await UploadToNasAsync(file, relativePath, "image");
                        if (ok) uploadedImages.Add(relativePath);
                        imgSeq++;
                    }
                }
            }

            // 上传视频
            if (videos != null && videos.Count > 0)
            {
                int vidSeq = 1;
                foreach (var file in videos)
                {
                    if (file.Length > 0)
                    {
                        if (file.Length > maxVideoSize)
                            return BadRequest($"第{vidSeq}个视频超过大小限制（最大{maxVideoSize / 1024 / 1024}MB）");

                        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                        var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".webm" };
                        if (!Array.Exists(allowedExtensions, e => e == ext))
                            return BadRequest($"第{vidSeq}个视频格式不支持，仅支持 MP4/MOV/AVI/WEBM");

                        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{vidSeq}{ext}";
                        var relativePath = $"video/{fileName}";
                        bool ok = await UploadToNasAsync(file, relativePath, "video");
                        if (ok) uploadedVideos.Add(relativePath);
                        vidSeq++;
                    }
                }
            }

            return Ok(new
            {
                images = uploadedImages,
                videos = uploadedVideos
            });
        }

        private async Task<bool> UploadToNasAsync(IFormFile file, string relativePath, string type)
        {
            try
            {
                var nasEndpoint = GetNasEndpoint();
                var username = _configuration["NasCredentials:Username"];
                var password = _configuration["NasCredentials:Password"];
                var fileUrl = $"{nasEndpoint}/{relativePath}";

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(60);

                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                using var fileStream = file.OpenReadStream();
                using var content = new StreamContent(fileStream);

                if (type == "image")
                    content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");
                else if (type == "video")
                    content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "video/mp4");
                else
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await httpClient.PutAsync(fileUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传到NAS失败: " + relativePath);
                return false;
            }
        }

        [HttpPost("like")]
        [Authorize]
        public async Task<IActionResult> LikePost([FromBody] int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // 防止重复点赞
            bool alreadyLiked = await _registerContext.PostsLike
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);
            if (alreadyLiked)
                return BadRequest("您已点赞过该帖子");

            // 新增点赞
            var like = new PostsLike
            {
                PostId = postId,
                UserId = userId,
                CreateTime = DateTime.UtcNow
            };
            _registerContext.PostsLike.Add(like);

            // 更新帖子点赞数
            var post = await _registerContext.Posts.FindAsync(postId);
            if (post != null)
                post.like_count += 1;








            // 点赞成功后插入活跃度记录
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = (int)ActivityType.Like,
                Score = 1, // 点赞可给1分，分值可调整
                CreateTime = DateTime.UtcNow
            };
            _functionDbContext.UserActivities.Add(activity);
            await _registerContext.SaveChangesAsync();





            // 分别保存
            await _registerContext.SaveChangesAsync();         // 保存点赞和帖子
            await _functionDbContext.SaveChangesAsync();       // 保存活跃度

            return Ok(new { likeCount = post?.like_count ?? 0 });
        }

        [HttpPost("unlike")]
        [Authorize]
        public async Task<IActionResult> UnlikePost([FromBody] int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var like = await _registerContext.PostsLike
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null)
                return BadRequest("您还没有点赞该帖子");

            _registerContext.PostsLike.Remove(like);

            var post = await _registerContext.Posts.FindAsync(postId);
            if (post != null && post.like_count > 0)
                post.like_count -= 1;

            await _registerContext.SaveChangesAsync();

            return Ok(new { likeCount = post?.like_count ?? 0 });
        }

        [HttpGet("hasliked")]
        [Authorize]
        public async Task<IActionResult> HasLiked(int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            bool liked = await _registerContext.PostsLike
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);
            return Ok(new { liked });
        }

        // 在 PostsController 类内添加（建议加在 like/unlike 之后）

        [HttpPost("report")]
        [Authorize]
        public async Task<IActionResult> ReportPost([FromBody] ReportRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                // 防止重复举报
                bool alreadyReported = await _registerContext.PostReports
                    .AnyAsync(r => r.PostId == request.PostId && r.UserId == userId);
                if (alreadyReported)
                    return BadRequest("您已举报过该帖子");

                // 写入举报记录
                var report = new PostReport
                {
                    PostId = request.PostId,
                    UserId = userId,
                    Reason = request.Reason,
                    CreateTime = DateTime.UtcNow
                };
                _registerContext.PostReports.Add(report);

                // 统计举报次数
                int reportCount = await _registerContext.PostReports
                    .CountAsync(r => r.PostId == request.PostId);

                // 阈值，比如5次
                int threshold = 10;
                var post = await _registerContext.Posts.FindAsync(request.PostId);
                if (post != null && post.status == 0 && reportCount >= threshold)
                {
                    post.status = 1; // 自动进入审核
                }

                // 可选：更新帖子实体的举报计数字段（如有）
                if (post != null)
                    post.report = reportCount;

                await _registerContext.SaveChangesAsync();
                return Ok(new { reportCount, status = post?.status ?? 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError($"举报帖子失败: {ex}");
                return StatusCode(500, "服务器内部错误");
            }
        }

        // 举报请求体
        public class ReportRequest
        {
            public int PostId { get; set; }
            public string Reason { get; set; }
        }

        [HttpGet("hot")]
        public async Task<IActionResult> GetHotPosts([FromQuery] int count = 10)
        {
            var posts = await _db.Posts
                .Where(p => p.status == 0)
                .Include(p => p.useraccount)
                .Include(p => p.userdata)
                .OrderByDescending(p => p.like_count * 5 + p.comment_count * 3 + p.view_count) // 可调整权重
                .ThenByDescending(p => p.createTime)
                .Take(count)
                .ToListAsync();

            var dtoList = posts.Select(post => new PostsViewDto
            {
                Id = post.id,
                Title = post.post_title,
                Content = post.content,
                PostType = post.post_type,
                Images = string.IsNullOrEmpty(post.images)
                    ? new List<string>()
                    : post.images.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Videos = string.IsNullOrEmpty(post.videos)
                    ? new List<string>()
                    : post.videos.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Author = post.useraccount?.username ?? "",
                AuthorAvatar = post.userdata?.logo,
                CreateTime = post.createTime.ToString("o"),
                LikeCount = post.like_count,
                CommentCount = post.comment_count,         // 新增
                ViewCount = post.view_count,               // 新增
                HotScore = post.like_count * 5 + post.comment_count * 3 + post.view_count, // 新增
                LikedByMe = false // 或自行判断
            }).ToList();

            return Ok(dtoList);
        }


        // 在 PostsController.cs 里添加
        public class DeletePostRequest
        {
            public int id { get; set; }
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> DeletePost([FromBody] DeletePostRequest req)
        {
            if (req == null || req.id <= 0)
                return BadRequest("请求参数缺失或不合法");

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var post = await _registerContext.Posts.FindAsync(req.id);
            if (post == null)
                return NotFound("帖子不存在");

            if (post.author_id != userId)
                return Forbid("只能删除自己发布的帖子");

            if (post.status != 0)
                return BadRequest("该帖子已被删除或处于不可操作状态");

            post.status = 1; // 1表示删除

            await _registerContext.SaveChangesAsync();

            return Ok(new { message = "删除成功，帖子状态已变为删除。" });
        }

        // 后端建议加这个接口（只返回本人帖子）
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyPosts()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var posts = await _db.Posts
                .Where(p => p.status == 0 && p.author_id == userId)
                .Include(p => p.useraccount)
                .Include(p => p.userdata)
                .OrderByDescending(p => p.createTime)
                .ToListAsync();

            // DTO组装同原来的 list 接口
            var dtoList = posts.Select(post => new PostsViewDto
            {
                Id = post.id,
                Title = post.post_title,
                Content = post.content,
                PostType = post.post_type,
                Images = string.IsNullOrEmpty(post.images)
                    ? new List<string>()
                    : post.images.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Videos = string.IsNullOrEmpty(post.videos)
                    ? new List<string>()
                    : post.videos.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Author = post.useraccount?.username ?? "",
                AuthorAvatar = post.userdata?.logo,
                CreateTime = post.createTime.ToString("o"),
                LikeCount = post.like_count,
                CommentCount = post.comment_count
            }).ToList();

            return Ok(dtoList);
        }





    }
}