using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Data;
using TCserver_Backend.Models;
using System.Security.Claims;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly FunctionDbContext _functionDb;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(FunctionDbContext functionDb, ILogger<NotificationController> logger)
        {
            _functionDb = functionDb;
            _logger = logger;
        }

        // 获取当前用户ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("无法获取用户ID");
            }
            return userId;
        }

        // 获取通知列表 - 修正版本
        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int? type = null,
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var query = _functionDb.Notifications
                    .Include(n => n.Sender)
                    .ThenInclude(s => s.useraccount)
                    .Where(n => n.UserId == currentUserId && !n.IsDeleted);

                // 按类型筛选
                if (type.HasValue)
                {
                    query = query.Where(n => n.type == type.Value);
                }

                // 只显示未读
                if (unreadOnly)
                {
                    query = query.Where(n => !n.IsRead);
                }

                // 排除过期通知
                query = query.Where(n => n.ExpireTime == null || n.ExpireTime > DateTime.Now);

                // 获取总数
                var totalCount = await query.CountAsync();

                // 先获取数据，然后在内存中处理
                var notificationsData = await query
                    .OrderByDescending(n => n.CreateTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new
                    {
                        id = n.Id,
                        type = n.type,
                        title = n.title,
                        content = n.content,
                        senderId = n.senderID,
                        senderName = n.Sender.useraccount.username,
                        isRead = n.IsRead,
                        isExpired = n.ExpireTime.HasValue && n.ExpireTime < DateTime.Now,
                        createTime = n.CreateTime,
                        readTime = n.ReadTime,
                        expireTime = n.ExpireTime
                    })
                    .ToListAsync();

                // 在内存中添加类型名称和图标
                var notifications = notificationsData.Select(n => new
                {
                    n.id,
                    n.type,
                    typeName = GetTypeNameStatic(n.type), // 使用静态方法
                    typeIcon = GetTypeIconStatic(n.type), // 使用静态方法
                    n.title,
                    n.content,
                    n.senderId,
                    n.senderName,
                    n.isRead,
                    n.isExpired,
                    n.createTime,
                    n.readTime,
                    n.expireTime
                }).ToList();

                return Ok(new
                {
                    notifications,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取通知失败");
                return BadRequest(new { message = "获取通知失败", error = ex.Message });
            }
        }

        // 获取未读通知数量
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var count = await _functionDb.Notifications
                    .CountAsync(n =>
                        n.UserId == currentUserId &&
                        !n.IsRead &&
                        !n.IsDeleted &&
                        (n.ExpireTime == null || n.ExpireTime > DateTime.Now));

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取未读数量失败");
                return BadRequest(new { message = "获取未读数量失败", error = ex.Message });
            }
        }

        // 获取通知详情
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var notificationData = await _functionDb.Notifications
                    .Include(n => n.Sender)
                    .ThenInclude(s => s.useraccount)
                    .Where(n => n.Id == id && n.UserId == currentUserId && !n.IsDeleted)
                    .Select(n => new
                    {
                        id = n.Id,
                        type = n.type,
                        title = n.title,
                        content = n.content,
                        senderId = n.senderID,
                        senderName = n.Sender.useraccount.username,
                        isRead = n.IsRead,
                        createTime = n.CreateTime,
                        readTime = n.ReadTime,
                        expireTime = n.ExpireTime
                    })
                    .FirstOrDefaultAsync();

                if (notificationData == null)
                {
                    return NotFound(new { message = "通知不存在" });
                }

                // 在内存中添加类型信息
                var notification = new
                {
                    notificationData.id,
                    notificationData.type,
                    typeName = GetTypeNameStatic(notificationData.type),
                    typeIcon = GetTypeIconStatic(notificationData.type),
                    notificationData.title,
                    notificationData.content,
                    notificationData.senderId,
                    notificationData.senderName,
                    notificationData.isRead,
                    notificationData.createTime,
                    notificationData.readTime,
                    notificationData.expireTime
                };

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取通知失败");
                return BadRequest(new { message = "获取通知失败", error = ex.Message });
            }
        }

        // 标记通知为已读
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var notification = await _functionDb.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUserId && !n.IsDeleted);

                if (notification == null)
                {
                    return NotFound(new { message = "通知不存在" });
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadTime = DateTime.Now;
                    await _functionDb.SaveChangesAsync();
                }

                return Ok(new { message = "标记为已读成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "标记为已读失败");
                return BadRequest(new { message = "标记为已读失败", error = ex.Message });
            }
        }

        // 标记所有通知为已读
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var unreadNotifications = await _functionDb.Notifications
                    .Where(n => n.UserId == currentUserId && !n.IsRead && !n.IsDeleted)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadTime = DateTime.Now;
                }

                await _functionDb.SaveChangesAsync();

                return Ok(new
                {
                    message = "全部标记为已读成功",
                    markedCount = unreadNotifications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "标记为已读失败");
                return BadRequest(new { message = "标记为已读失败", error = ex.Message });
            }
        }

        // 删除通知
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var notification = await _functionDb.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUserId && !n.IsDeleted);

                if (notification == null)
                {
                    return NotFound(new { message = "通知不存在" });
                }

                notification.IsDeleted = true;
                await _functionDb.SaveChangesAsync();

                return Ok(new { message = "删除通知成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除通知失败");
                return BadRequest(new { message = "删除通知失败", error = ex.Message });
            }
        }

        // 清空所有通知
        [HttpDelete]
        public async Task<IActionResult> ClearAllNotifications()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var notifications = await _functionDb.Notifications
                    .Where(n => n.UserId == currentUserId && !n.IsDeleted)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsDeleted = true;
                }

                await _functionDb.SaveChangesAsync();

                return Ok(new
                {
                    message = "清空通知成功",
                    clearedCount = notifications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空通知失败");
                return BadRequest(new { message = "清空通知失败", error = ex.Message });
            }
        }

        // 创建通知
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = dto.UserId,
                    type = dto.Type,
                    title = dto.Title,
                    content = dto.Content,
                    senderID = dto.SenderId,
                    IsRead = false,
                    IsDeleted = false,
                    ExpireTime = dto.ExpireTime,
                    CreateTime = DateTime.Now
                };

                _functionDb.Notifications.Add(notification);
                await _functionDb.SaveChangesAsync();

                return Ok(new
                {
                    message = "通知创建成功",
                    notificationId = notification.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建通知失败");
                return BadRequest(new { message = "创建通知失败", error = ex.Message });
            }
        }

        // 获取通知类型统计
        [HttpGet("type-stats")]
        public async Task<IActionResult> GetTypeStatistics()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var statsData = await _functionDb.Notifications
                    .Where(n => n.UserId == currentUserId && !n.IsDeleted)
                    .GroupBy(n => n.type)
                    .Select(g => new
                    {
                        type = g.Key,
                        totalCount = g.Count(),
                        unreadCount = g.Count(n => !n.IsRead)
                    })
                    .ToListAsync();

                // 在内存中添加类型名称
                var stats = statsData.Select(s => new
                {
                    s.type,
                    typeName = GetTypeNameStatic(s.type),
                    s.totalCount,
                    s.unreadCount
                }).ToList();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取统计失败");
                return BadRequest(new { message = "获取统计失败", error = ex.Message });
            }
        }

        // 发送系统通知给所有用户
        [HttpPost("send-to-all")]
        public async Task<IActionResult> SendNotificationToAll([FromBody] SendNotificationToAllDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // 获取所有活跃用户ID
                var activeUserIds = await _functionDb.userdata
                    .Where(u => u.last_active_time >= DateTime.Now.AddDays(-30)) // 最近30天活跃的用户
                    .Select(u => u.id)
                    .ToListAsync();

                var sentCount = 0;
                var errors = new List<string>();

                // 批量发送通知（分批处理避免内存溢出）
                var batchSize = 100;
                for (int i = 0; i < activeUserIds.Count; i += batchSize)
                {
                    var batchUserIds = activeUserIds.Skip(i).Take(batchSize).ToList();

                    foreach (var userId in batchUserIds)
                    {
                        try
                        {
                            var notification = new Notification
                            {
                                UserId = userId,
                                type = 1, // 系统通知
                                title = dto.Title,
                                content = dto.Content,
                                senderID = currentUserId, // 发送者为当前管理员
                                IsRead = false,
                                IsDeleted = false,
                                ExpireTime = dto.ExpireTime,
                                CreateTime = DateTime.Now
                            };

                            _functionDb.Notifications.Add(notification);
                            sentCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"用户 {userId}: {ex.Message}");
                            _logger.LogError(ex, $"为用户 {userId} 创建通知失败");
                        }
                    }

                    // 每批保存一次
                    await _functionDb.SaveChangesAsync();
                }

                _logger.LogInformation($"系统通知发送完成: 成功 {sentCount} 条，失败 {errors.Count} 条");

                var result = new
                {
                    message = "系统通知发送完成",
                    sentCount,
                    errorCount = errors.Count,
                    errors = errors.Take(10).ToList() // 只返回前10个错误
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送系统通知失败");
                return BadRequest(new { message = "发送系统通知失败", error = ex.Message });
            }
        }

        // 发送通知给选定用户
        [HttpPost("send-to-users")]
        public async Task<IActionResult> SendNotificationToUsers([FromBody] SendNotificationToUsersDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var sentCount = 0;
                var errors = new List<string>();

                foreach (var userId in dto.UserIds)
                {
                    try
                    {
                        // 检查用户是否存在
                        var userExists = await _functionDb.userdata.AnyAsync(u => u.id == userId);
                        if (!userExists)
                        {
                            errors.Add($"用户 {userId} 不存在");
                            continue;
                        }

                        var notification = new Notification
                        {
                            UserId = userId,
                            type = dto.Type,
                            title = dto.Title,
                            content = dto.Content,
                            senderID = currentUserId,
                            IsRead = false,
                            IsDeleted = false,
                            ExpireTime = dto.ExpireTime,
                            CreateTime = DateTime.Now
                        };

                        _functionDb.Notifications.Add(notification);
                        sentCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"用户 {userId}: {ex.Message}");
                        _logger.LogError(ex, $"为用户 {userId} 创建通知失败");
                    }
                }

                await _functionDb.SaveChangesAsync();

                var result = new
                {
                    message = "通知发送完成",
                    sentCount,
                    errorCount = errors.Count,
                    errors = errors.Take(10).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送用户通知失败");
                return BadRequest(new { message = "发送用户通知失败", error = ex.Message });
            }
        }

        // 发送通知给用户组
        [HttpPost("send-to-group")]
        public async Task<IActionResult> SendNotificationToGroup([FromBody] SendNotificationToGroupDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // 根据用户组类型获取用户ID
                IQueryable<int> userQuery = dto.GroupType switch
                {
                    "vip" => _functionDb.userdata.Where(u => u.level >= 10).Select(u => u.id), // VIP用户（等级≥10）
                    "new" => _functionDb.userdata.Where(u => u.lastlogin >= DateTime.Now.AddDays(-7)).Select(u => u.id), // 新用户（7天内注册）
                    "active" => _functionDb.userdata.Where(u => u.last_active_time >= DateTime.Now.AddDays(-7)).Select(u => u.id), // 活跃用户（7天内有活动）
                    "inactive" => _functionDb.userdata.Where(u => u.last_active_time < DateTime.Now.AddDays(-30)).Select(u => u.id), // 非活跃用户
                    _ => _functionDb.userdata.Select(u => u.id) // 所有用户
                };

                var userIds = await userQuery.ToListAsync();
                var sentCount = 0;
                var errors = new List<string>();

                // 批量处理
                var batchSize = 100;
                for (int i = 0; i < userIds.Count; i += batchSize)
                {
                    var batchUserIds = userIds.Skip(i).Take(batchSize).ToList();

                    foreach (var userId in batchUserIds)
                    {
                        try
                        {
                            var notification = new Notification
                            {
                                UserId = userId,
                                type = dto.Type,
                                title = dto.Title,
                                content = dto.Content,
                                senderID = currentUserId,
                                IsRead = false,
                                IsDeleted = false,
                                ExpireTime = dto.ExpireTime,
                                CreateTime = DateTime.Now
                            };

                            _functionDb.Notifications.Add(notification);
                            sentCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"用户 {userId}: {ex.Message}");
                        }
                    }

                    await _functionDb.SaveChangesAsync();
                }

                var result = new
                {
                    message = $"用户组通知发送完成 ({dto.GroupType})",
                    sentCount,
                    errorCount = errors.Count,
                    groupType = dto.GroupType,
                    totalUsers = userIds.Count
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送用户组通知失败");
                return BadRequest(new { message = "发送用户组通知失败", error = ex.Message });
            }
        }

        // 获取发送统计信息
        [HttpGet("send-stats")]
        public async Task<IActionResult> GetSendStats()
        {
            try
            {
                var totalUsers = await _functionDb.userdata.CountAsync();
                var activeUsers = await _functionDb.userdata
                    .CountAsync(u => u.last_active_time >= DateTime.Now.AddDays(-30));
                var newUsers = await _functionDb.userdata
                    .CountAsync(u => u.lastlogin >= DateTime.Now.AddDays(-7));
                var vipUsers = await _functionDb.userdata
                    .CountAsync(u => u.level >= 10);

                return Ok(new
                {
                    totalUsers,
                    activeUsers,
                    newUsers,
                    vipUsers,
                    inactiveUsers = totalUsers - activeUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取发送统计失败");
                return BadRequest(new { message = "获取发送统计失败", error = ex.Message });
            }
        }

        // 静态方法：获取类型名称
        private static string GetTypeNameStatic(int type)
        {
            return type switch
            {
                1 => "系统通知",
                2 => "活动通知",
                3 => "好友请求",
                4 => "私信通知",
                5 => "帖子回复",
                6 => "评论回复",
                7 => "帖子点赞",
                8 => "评论点赞",
                9 => "关注通知",
                10 => "积分变动",
                11 => "等级提升",
                12 => "奖励发放",
                13 => "举报结果",
                _ => "未知类型"
            };
        }

        // 静态方法：获取类型图标
        private static string GetTypeIconStatic(int type)
        {
            return type switch
            {
                1 => "🔔",
                2 => "🎯",
                3 => "👥",
                4 => "💬",
                5 => "📝",
                6 => "↩️",
                7 => "👍",
                8 => "❤️",
                9 => "➕",
                10 => "💰",
                11 => "⭐",
                12 => "🎁",
                13 => "📋",
                _ => "📄"
            };
        }

        // 保留实例方法供其他用途使用
        private string GetTypeName(int type) => GetTypeNameStatic(type);
        private string GetTypeIcon(int type) => GetTypeIconStatic(type);
    }

    // DTO类
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public int Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int SenderId { get; set; } = 1;
        public DateTime? ExpireTime { get; set; }
    }

    public class SendNotificationToAllDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? ExpireTime { get; set; }
        public bool Important { get; set; }
        public bool RequiresAck { get; set; }
    }

    public class SendNotificationToUsersDto
    {
        public List<int> UserIds { get; set; } = new List<int>();
        public int Type { get; set; } = 1;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? ExpireTime { get; set; }
    }

    public class SendNotificationToGroupDto
    {
        public string GroupType { get; set; } = "all"; // all, vip, new, active, inactive
        public int Type { get; set; } = 1;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? ExpireTime { get; set; }
    }
}