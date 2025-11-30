using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using THCY_BE.DataBase;
using THCY_BE.Dto.FeedBack;
using THCY_BE.Models.FeedBack;

namespace THCY_BE.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FeedBackController : ControllerBase
    {
        private readonly ILogger<FeedBackController> _logger;
        private readonly FeedBackDbContext _context;

        private const string BASE_PHYSICAL_PATH = "/www/wwwroot/bianyuzhou.com/uploads";
        private const string BASE_WEB_PATH = "/uploads";
        private const string BASE_URL = "https://bianyuzhou.com"; // 生产环境URL

        public FeedBackController(ILogger<FeedBackController> logger, FeedBackDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 🔥 添加 LocalUploadResult 类的定义
        private class LocalUploadResult
        {
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
            public string? FileName { get; set; }
            public string? FilePath { get; set; }
            public long FileSize { get; set; }
            public string? PhysicalPath { get; set; }
        }

        /// <summary>
        /// 验证图片文件
        /// </summary>
        private bool ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // 检查文件大小（5MB限制）
            if (file.Length > 5 * 1024 * 1024)
                return false;

            // 检查文件类型
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(fileExtension);
        }

        /// <summary>
        /// 🔥 修复：将 BuildImageUrl 改为静态方法
        /// </summary>
        private static string BuildImageUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            return $"{BASE_URL.TrimEnd('/')}/{BASE_WEB_PATH.TrimStart('/')}/{relativePath.TrimStart('/')}"
                .Replace("https:/", "https://")
                .Replace("http:/", "http://")
                .Replace("\\", "/")
                .Replace("//", "/");
        }

        /// <summary>
        /// 上传反馈图片
        /// </summary>
        private async Task<LocalUploadResult> UploadFeedbackImageAsync(IFormFile file, int userId)
        {
            try
            {
                if (!ValidateImageFile(file))
                {
                    return new LocalUploadResult { Success = false, ErrorMessage = "文件格式不支持或文件过大" };
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // 生成文件名：feedback_{用户ID}_{时间戳}_{随机数}
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var random = new Random().Next(1000, 9999);
                var fileName = $"feedback_{userId}_{timestamp}_{random}{fileExtension}";

                var userFolder = userId.ToString();

                // 构建路径（保持原有路径不变）
                var physicalPath = Path.Combine(BASE_PHYSICAL_PATH, "反馈板块", "意见箱", userFolder, fileName);
                var relativePath = Path.Combine("反馈板块", "意见箱", userFolder, fileName).Replace("\\", "/");

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

                _logger.LogInformation("✅ 反馈图片保存成功: {FileName}", fileName);

                return new LocalUploadResult
                {
                    Success = true,
                    FileName = fileName,
                    FilePath = relativePath, // 返回相对路径
                    FileSize = file.Length,
                    PhysicalPath = physicalPath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 反馈图片上传失败");
                return new LocalUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        // 修改创建反馈方法，存储相对路径
        [HttpPost("create")]
        public async Task<ActionResult> CreateFeedback([FromForm] FeedBackDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "数据验证失败" });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                // 处理QQ号码转换
                int? contactQQ = null;
                if (!string.IsNullOrEmpty(dto.ContactQQ))
                {
                    if (int.TryParse(dto.ContactQQ, out int qq) && qq >= 10000 && qq <= 999999999999999)
                        contactQQ = qq;
                    else
                        return BadRequest(new { success = false, message = "QQ号码格式不正确" });
                }

                // 处理图片上传
                string imageRelativePath = null; // 存储相对路径
                string imageFullUrl = null; // 完整URL用于返回给前端
                if (dto.ErrorImage != null && dto.ErrorImage.Length > 0)
                {
                    var uploadResult = await UploadFeedbackImageAsync(dto.ErrorImage, userId.Value);
                    if (!uploadResult.Success)
                        return BadRequest(new { success = false, message = uploadResult.ErrorMessage });

                    imageRelativePath = uploadResult.FilePath; // 存储相对路径到数据库
                    imageFullUrl = BuildImageUrl(uploadResult.FilePath); // 构建完整URL给前端
                }

                // 创建反馈
                var feedback = new Feedback
                {
                    title = dto.Title.Trim(),
                    content = dto.Content.Trim(),
                    type = dto.Type,
                    contactQQ = contactQQ,
                    imagesUrl = imageRelativePath, // 存储相对路径
                    status = 0,
                    createTime = DateTime.Now
                };

                _context.FeedBacks.Add(feedback);
                await _context.SaveChangesAsync();

                _logger.LogInformation("用户 {UserId} 创建反馈成功: {FeedbackId}", userId, feedback.id);

                return Ok(new
                {
                    success = true,
                    message = GetSuccessMessage(dto.Type),
                    data = new
                    {
                        id = feedback.id,
                        title = feedback.title,
                        type = feedback.type,
                        createTime = feedback.createTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        hasImage = !string.IsNullOrEmpty(imageRelativePath),
                        imageUrl = imageFullUrl // 返回完整URL给前端
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建反馈失败");
                return StatusCode(500, new { success = false, message = "创建反馈失败" });
            }
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult> GetFeedbacks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? type = null)
        {
            try
            {
                var query = _context.FeedBacks.AsQueryable();

                if (type.HasValue)
                    query = query.Where(x => x.type == type);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(x => x.createTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.id,
                        x.title,
                        x.content,
                        x.type,
                        x.status,
                        x.createTime,
                        x.contactQQ,
                        imagesUrl = x.imagesUrl, // 数据库中的相对路径
                        // 🔥 修复：使用静态方法调用
                        imageFullUrl = !string.IsNullOrEmpty(x.imagesUrl) ? BuildImageUrl(x.imagesUrl) : null
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items,
                        totalCount,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取反馈列表失败");
                return StatusCode(500, new { success = false, message = "获取列表失败" });
            }
        }

        /// <summary>
        /// 获取反馈详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetFeedback(int id)
        {
            try
            {
                var feedback = await _context.FeedBacks
                    .Where(x => x.id == id)
                    .Select(x => new
                    {
                        x.id,
                        x.title,
                        x.content,
                        x.type,
                        x.status,
                        x.createTime,
                        x.contactQQ,
                        imagesUrl = x.imagesUrl, // 相对路径
                        // 🔥 修复：使用静态方法调用
                        imageFullUrl = !string.IsNullOrEmpty(x.imagesUrl) ? BuildImageUrl(x.imagesUrl) : null
                    })
                    .FirstOrDefaultAsync();

                if (feedback == null)
                {
                    return NotFound(new { success = false, message = "反馈不存在" });
                }

                return Ok(new
                {
                    success = true,
                    data = feedback
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取反馈详情失败: {Id}", id);
                return StatusCode(500, new { success = false, message = "获取详情失败" });
            }
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : null;
        }

        /// <summary>
        /// 根据反馈类型返回成功消息
        /// </summary>
        private string GetSuccessMessage(int type) => type switch
        {
            1 => "BUG反馈提交成功，我们会尽快处理",
            2 => "感谢您的宝贵建议",
            3 => "举报已收到，我们会尽快核实",
            4 => "反馈提交成功，感谢您对太初寰宇社区做出的贡献",
            _ => "反馈提交成功"
        };
    }
}