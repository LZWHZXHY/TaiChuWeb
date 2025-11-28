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

        public FeedBackController(ILogger<FeedBackController> logger, FeedBackDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// 创建反馈
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateFeedback([FromForm] FeedBackDto dto)
        {
            try
            {
                // 基本验证
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = string.Join("; ", errors)
                    });
                }

                // 获取当前用户
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                // 处理图片上传
                string? imageUrl = null;
                if (dto.ErrorImage != null)
                {
                    var uploadResult = await HandleImageUpload(dto.ErrorImage);
                    if (!uploadResult.Success)
                    {
                        return BadRequest(new ApiResponse<object> { Success = false, Message = uploadResult.Message });
                    }
                    imageUrl = uploadResult.ImageUrl;
                }

                // 创建反馈记录
                var feedback = new Feedback
                {
                    title = dto.Title.Trim(),
                    content = dto.Content.Trim(),
                    type = dto.Type,
                    status = 0, // 待处理
                    createTime = DateTime.Now
                };

                _context.FeedBacks.Add(feedback);
                await _context.SaveChangesAsync();

                _logger.LogInformation("用户 {UserId} 创建反馈成功: {FeedbackId}", userId, feedback.id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = GetSuccessMessage(dto.Type),
                    Data = new { FeedbackId = feedback.id, CreateTime = feedback.createTime, ImageUrl = imageUrl }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建反馈失败");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "创建反馈失败" });
            }
        }

        /// <summary>
        /// 获取反馈列表
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<PagedResult<FeedBackItemDto>>>> GetFeedbacks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? type = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query = _context.FeedBacks.AsQueryable();

                // 筛选
                if (type.HasValue) query = query.Where(x => x.type == type);
                if (status.HasValue) query = query.Where(x => x.status == status);

                // 分页
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(x => x.createTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new FeedBackItemDto
                    {
                        Id = x.id,
                        Title = x.title,
                        Content = x.content,
                        Type = x.type,
                        Status = x.status,
                        CreateTime = x.createTime
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<PagedResult<FeedBackItemDto>>
                {
                    Success = true,
                    Data = new PagedResult<FeedBackItemDto>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        Page = page,
                        PageSize = pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取反馈列表失败");
                return StatusCode(500, new ApiResponse<PagedResult<FeedBackItemDto>> { Success = false, Message = "获取列表失败" });
            }
        }

        /// <summary>
        /// 获取反馈详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FeedBackItemDto>>> GetFeedback(int id)
        {
            try
            {
                var feedback = await _context.FeedBacks
                    .Where(x => x.id == id)
                    .Select(x => new FeedBackItemDto
                    {
                        Id = x.id,
                        Title = x.title,
                        Content = x.content,
                        Type = x.type,
                        Status = x.status,
                        CreateTime = x.createTime
                    })
                    .FirstOrDefaultAsync();

                if (feedback == null)
                {
                    return NotFound(new ApiResponse<FeedBackItemDto> { Success = false, Message = "反馈不存在" });
                }

                return Ok(new ApiResponse<FeedBackItemDto> { Success = true, Data = feedback });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取反馈详情失败: {Id}", id);
                return StatusCode(500, new ApiResponse<FeedBackItemDto> { Success = false, Message = "获取详情失败" });
            }
        }

        /// <summary>
        /// 更新反馈状态（管理员）
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            try
            {
                var feedback = await _context.FeedBacks.FindAsync(id);
                if (feedback == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = "反馈不存在" });
                }

                feedback.status = dto.Status;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object> { Success = true, Message = "状态更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新反馈状态失败: {Id}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "更新状态失败" });
            }
        }

        /// <summary>
        /// 获取反馈类型选项
        /// </summary>
        [HttpGet("types")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<object>> GetTypes()
        {
            var types = new[]
            {
                new { Value = 1, Label = "网站BUG反馈" },
                new { Value = 2, Label = "社区意见" },
                new { Value = 3, Label = "内容举报" },
                new { Value = 4, Label = "其他" }
            };

            return Ok(new ApiResponse<object> { Success = true, Data = types });
        }

        // 辅助方法
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : null;
        }

        private async Task<(bool Success, string Message, string? ImageUrl)> HandleImageUpload(IFormFile image)
        {
            try
            {
                // 简单的图片上传逻辑
                if (image.Length > 10 * 1024 * 1024) // 10MB
                    return (false, "图片不能超过10MB", null);

                var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(image.FileName).ToLower();
                if (!allowedTypes.Contains(extension))
                    return (false, "只支持jpg、png、gif格式", null);

                // 这里可以添加实际的文件保存逻辑
                var fileName = $"{Guid.NewGuid()}{extension}";
                var imageUrl = $"/uploads/feedback/{fileName}";

                return (true, "上传成功", imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "图片上传失败");
                return (false, "图片上传失败", null);
            }
        }

        private string GetSuccessMessage(int type) => type switch
        {
            1 => "BUG反馈提交成功，我们会尽快处理",
            2 => "感谢您的宝贵建议",
            3 => "举报已收到，我们会尽快核实",
            4 => "反馈提交成功，感谢您的支持",
            _ => "反馈提交成功"
        };
    }
}