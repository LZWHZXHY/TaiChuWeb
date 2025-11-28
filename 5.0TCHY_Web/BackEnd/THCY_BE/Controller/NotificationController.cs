using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using THCY_BE.DataBase;
using THCY_BE.Dto.Notification;
using THCY_BE.Models.Notification;

namespace THCY_BE.Controller
{
    [Authorize] // 需携带 Bearer Token 才能访问
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(NotificationDbContext context, ILogger<NotificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户列表（原有接口，保留）
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult> GetUserDataList()
        {
            try
            {
                var userList = await _context.UserDatas
                    .Join(_context.UserAccounts,
                          userData => userData.id,
                          userAccount => userAccount.Id,
                          (userData, userAccount) => new { userData, userAccount })
                    .Where(joined => joined.userAccount.state != 2) // 排除封禁用户
                    .Select(joined => new
                    {
                        id = joined.userData.id,
                        points = joined.userData.points,
                        level = joined.userData.level,
                        exp = joined.userData.exp,
                        title = joined.userData.title,
                        lastLogin = joined.userData.lastlogin,
                        logo = joined.userData.logo,
                        background = joined.userData.background,
                        likes = joined.userData.likes,
                        lastActiveTime = joined.userData.last_active_time,
                        byd = joined.userData.byd,
                        creater = joined.userData.creater,

                        username = joined.userAccount.username,
                        email = joined.userAccount.email_address,
                        state = joined.userAccount.state,
                        rank = joined.userAccount.rank,
                        isVerified = joined.userAccount.is_verified,
                        registrationDate = joined.userAccount.date,
                        failedLoginAttempts = joined.userAccount.failed_login_attempts,
                        lastFailedLogin = joined.userAccount.last_failed_login
                    })
                    .OrderBy(u => u.id)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        total = userList.Count,
                        items = userList
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserDataList failed");
                return StatusCode(500, new
                {
                    success = false,
                    message = "获取用户列表失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 批量发送通知：为每个 recipients 创建一条 notifications 表记录
        /// 前端 POST /api/notification/send
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotifications([FromBody] NotificationRequest req)
        {
            if (req == null)
                return BadRequest(new { success = false, message = "请求体不能为空" });

            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest(new { success = false, message = "通知内容不能为空" });

            if (req.Recipients == null || !req.Recipients.Any())
                return BadRequest(new { success = false, message = "请至少指定一个接收者" });

            // 根据你前面的说明，type=1 为系统紧急通知，type=2 为更新通知
            if (req.Type <= 0)
                return BadRequest(new { success = false, message = "非法的通知类型" });

            var now = DateTime.Now;
            var senderId = req.SenderId ?? 1; // 默认系统发送者为 1

            // 去重并转换 recipient id（确保是 int）
            var recipients = req.Recipients
                .Where(x => x != null)
                .Select(x => Convert.ToInt32(x))
                .Distinct()
                .ToList();

            var notifications = new List<Notification>(recipients.Count);
            foreach (var uid in recipients)
            {
                notifications.Add(new Notification
                {
                    UserId = uid,
                    type = req.Type,
                    title = req.Title ?? string.Empty,
                    content = req.Content,
                    senderID = senderId,
                    IsRead = false,      // 使用 bool，默认未读
                    IsDeleted = false,
                    CreateTime = now,
                    ExpireTime = req.ExpireTime
                });
            }

            // 使用事务确保原子性
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Notifications.AddRangeAsync(notifications);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return Ok(new { success = true, inserted = notifications.Count });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "SendNotifications failed");
                return StatusCode(500, new { success = false, message = "写入通知失败" });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotifications()
        {
            try
            {
                var list = await _context.Notifications
                    .AsNoTracking()
                    .Where(n => !n.IsDeleted) // 如需包含已删除项可去掉此条件
                    .OrderByDescending(n => n.CreateTime)
                    .Select(n => new
                    {
                        n.Id,
                        n.UserId,
                        Type = n.type,
                        n.title,
                        n.content,
                        n.senderID,
                        n.IsRead,     // 返回 bool
                        n.IsDeleted,
                        n.ExpireTime,
                        n.CreateTime,
                        n.ReadTime
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        total = list.Count,
                        items = list
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllNotifications failed");
                return StatusCode(500, new { success = false, message = "获取通知失败: " + ex.Message });
            }
        }

        // ----------------- 新增 API: 标记为已读/未读 -----------------
        /// <summary>
        /// 标记指定通知为已读或未读（当前登录用户只能操作自己的通知，管理员例外）
        /// PATCH /api/notification/{id}/read
        /// body: { "isRead": true/false }
        /// </summary>
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(int id, [FromBody] MarkReadRequest dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "请求体不能为空" });

            try
            {
                var notif = await _context.Notifications.FindAsync(id);
                if (notif == null || notif.IsDeleted)
                    return NotFound(new { success = false, message = "通知未找到" });

                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { success = false, message = "无法获取当前用户信息" });

                // 只有通知拥有者或管理员可以标记
                if (notif.UserId != currentUserId && !User.IsInRole("Admin"))
                    return Forbid();

                notif.IsRead = dto.IsRead;
                notif.ReadTime = dto.IsRead ? DateTime.Now : (DateTime?)null;

                _context.Notifications.Update(notif);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, id = id, isRead = notif.IsRead, readTime = notif.ReadTime });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkRead failed for id {Id}", id);
                return StatusCode(500, new { success = false, message = "操作失败" });
            }
        }

        // ----------------- 新增 API: 用户自行删除通知（软删除） -----------------
        /// <summary>
        /// 删除（软删除）指定通知：当前登录用户只能删除自己的通知，管理员例外
        /// DELETE /api/notification/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var notif = await _context.Notifications.FindAsync(id);
                if (notif == null || notif.IsDeleted)
                    return NotFound(new { success = false, message = "通知未找到" });

                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { success = false, message = "无法获取当前用户信息" });

                // 只有通知拥有者或管理员可以删除
                if (notif.UserId != currentUserId && !User.IsInRole("Admin"))
                    return Forbid();

                // 软删除以保留审计/回滚能力
                notif.IsDeleted = true;
                _context.Notifications.Update(notif);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteNotification failed for id {Id}", id);
                return StatusCode(500, new { success = false, message = "删除失败" });
            }
        }

        // ----------------- 辅助方法 -----------------
        /// <summary>
        /// 从 Claims 中尝试获取当前用户 Id（整型），返回 null 表示无法解析
        /// 注意：需保证 JWT 中包含用户 id（sub / nameidentifier / id）
        /// </summary>
        private int? GetCurrentUserId()
        {
            // 常见的 claim 名称：sub, nameidentifier, id
            var idClaim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (idClaim == null) return null;
            if (int.TryParse(idClaim.Value, out var id)) return id;
            return null;
        }

        // 我把 MarkReadRequest 内嵌到 Controller 里，避免额外 DTO 文件。
        // 如果你更希望保留 DTO 文件也可以，但目前按你的要求把它写在这里。
        public class MarkReadRequest
        {
            public bool IsRead { get; set; } = true;
        }

        // 片段：GetMyNotifications 的核心实现（可直接放进你的 Controller）
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int? type, [FromQuery] bool? isRead,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == currentUserId.Value && !n.IsDeleted);

            if (type.HasValue) query = query.Where(n => n.type == type.Value);
            if (isRead.HasValue) query = query.Where(n => n.IsRead == isRead.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(n => n.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new {
                    n.Id,
                    n.UserId,
                    Type = n.type,
                    n.title,
                    n.content,
                    n.senderID,
                    n.IsRead,
                    n.IsDeleted,
                    n.ExpireTime,
                    n.CreateTime,
                    n.ReadTime
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new { total, page, pageSize, items }
            });
        }


        // 将此方法添加到 NotificationController 中（例如在 GetMyNotifications 附近）
        [HttpGet("unread/count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { success = false, message = "无法获取当前用户信息" });

            try
            {
                var cnt = await _context.Notifications
                    .AsNoTracking()
                    .Where(n => n.UserId == currentUserId.Value && !n.IsDeleted && !n.IsRead)
                    .CountAsync();

                return Ok(new { success = true, unread = cnt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUnreadCount failed for user {UserId}", currentUserId);
                return StatusCode(500, new { success = false, message = "获取未读数量失败" });
            }
        }

    }
}