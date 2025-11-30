using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using THCY_BE.DataBase;
using THCY_BE.Models.Friends;
using THCY_BE.Models.UserDate;

namespace THCY_BE.Controller.Friends
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly FriendsDbContext _context;
        private readonly ILogger<FriendsController> _logger;

        public FriendsController(FriendsDbContext context, ILogger<FriendsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : null;
        }

        /// <summary>
        /// 搜索用户
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult> SearchUsers(
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null) return Unauthorized();

                if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                    return BadRequest(new { success = false, message = "关键词至少需要2个字符" });

                // 获取当前用户的好友列表和待处理请求
                var friendIds = await _context.Friends
                    .Where(f => f.UserId == currentUserId.Value && f.status == 0)
                    .Select(f => f.FriendId)
                    .ToListAsync();

                var pendingRequestFromIds = await _context.FriendRequests
                    .Where(r => r.ToUserId == currentUserId.Value && r.Status == 0)
                    .Select(r => r.FromUserId)
                    .ToListAsync();

                var pendingRequestToIds = await _context.FriendRequests
                    .Where(r => r.FromUserId == currentUserId.Value && r.Status == 0)
                    .Select(r => r.ToUserId)
                    .ToListAsync();

                var query = _context.UserDatas
                    .Include(u => u.useraccount)
                    .Where(u => u.useraccount.username.Contains(keyword) ||
                               u.id.ToString().Contains(keyword) ||
                               (u.title != null && u.title.Contains(keyword)))
                    .Where(u => u.id != currentUserId.Value) // 排除自己
                    .OrderByDescending(u => u.level);

                var totalCount = await query.CountAsync();
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        id = u.id,
                        username = u.useraccount.username,
                        level = u.level,
                        points = u.points,
                        exp = u.exp,
                        likes = u.likes,
                        title = u.title,
                        logo = u.logo,
                        isOnline = u.useraccount.state == 1,
                        isFriend = friendIds.Contains(u.id),
                        hasPendingRequestFromMe = pendingRequestToIds.Contains(u.id),
                        hasPendingRequestToMe = pendingRequestFromIds.Contains(u.id)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalCount,
                        page,
                        pageSize,
                        users
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索用户失败");
                return StatusCode(500, new { success = false, message = "搜索失败" });
            }
        }

        /// <summary>
        /// 获取用户收到的未处理好友申请
        /// </summary>
        [HttpGet("pending-requests")]
        public async Task<ActionResult> GetPendingRequests()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null) return Unauthorized();

                var requests = await _context.FriendRequests
                    .Include(r => r.FromUser)
                    .ThenInclude(u => u.useraccount)
                    .Where(r => r.ToUserId == currentUserId.Value && r.Status == 0)
                    .OrderByDescending(r => r.CreateTime)
                    .Select(r => new
                    {
                        RequestId = r.Id,
                        FromUserId = r.FromUserId,
                        FromUserName = r.FromUser.useraccount.username,
                        FromUserLevel = r.FromUser.level,
                        FromUserAvatar = r.FromUser.logo,
                        FromUserTitle = r.FromUser.title,
                        Remark = r.Remark,
                        CreateTime = r.CreateTime
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取好友申请失败");
                return StatusCode(500, new { success = false, message = "获取失败" });
            }
        }

        /// <summary>
        /// 发送好友请求
        /// </summary>
        [HttpPost("send-request")]
        public async Task<ActionResult> SendFriendRequest([FromBody] SendRequestDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null) return Unauthorized();

                if (request.ToUserId <= 0)
                    return BadRequest(new { success = false, message = "参数错误" });

                if (currentUserId.Value == request.ToUserId)
                    return BadRequest(new { success = false, message = "不能添加自己为好友" });

                // 检查目标用户是否存在
                var targetUser = await _context.UserDatas
                    .FirstOrDefaultAsync(u => u.id == request.ToUserId);
                if (targetUser == null)
                    return BadRequest(new { success = false, message = "用户不存在" });

                // 检查是否已是好友
                var isFriend = await _context.Friends
                    .AnyAsync(f => f.UserId == currentUserId.Value && f.FriendId == request.ToUserId && f.status == 0);
                if (isFriend)
                    return BadRequest(new { success = false, message = "已是好友" });

                // 检查是否有待处理的申请
                var existingRequest = await _context.FriendRequests
                    .FirstOrDefaultAsync(r => r.FromUserId == currentUserId.Value &&
                                            r.ToUserId == request.ToUserId &&
                                            r.Status == 0);
                if (existingRequest != null)
                    return BadRequest(new { success = false, message = "已发送过申请" });

                // 检查对方是否已向你发送请求
                var reverseRequest = await _context.FriendRequests
                    .FirstOrDefaultAsync(r => r.FromUserId == request.ToUserId &&
                                            r.ToUserId == currentUserId.Value &&
                                            r.Status == 0);
                if (reverseRequest != null)
                    return BadRequest(new { success = false, message = "对方已向您发送好友请求，请先处理" });

                var friendRequest = new FriendRequest
                {
                    FromUserId = currentUserId.Value,
                    ToUserId = request.ToUserId,
                    Remark = request.Remark?.Trim() ?? "",
                    Status = 0,
                    CreateTime = DateTime.Now
                };

                _context.FriendRequests.Add(friendRequest);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "好友申请发送成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送好友申请失败");
                return StatusCode(500, new { success = false, message = "发送失败" });
            }
        }

        /// <summary>
        /// 处理好友申请
        /// </summary>
        [HttpPost("handle-request")]
        public async Task<ActionResult> HandleFriendRequest([FromBody] HandleRequestDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null) return Unauthorized();

                if (request.RequestId <= 0 || string.IsNullOrEmpty(request.Action))
                    return BadRequest(new { success = false, message = "参数错误" });

                var friendRequest = await _context.FriendRequests
                    .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.ToUserId == currentUserId.Value);

                if (friendRequest == null)
                    return NotFound(new { success = false, message = "申请不存在" });

                if (friendRequest.Status != 0)
                    return BadRequest(new { success = false, message = "申请已处理" });

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    if (request.Action.ToLower() == "accept")
                    {
                        // 同意申请
                        friendRequest.Status = 1;
                        friendRequest.ProcessTime = DateTime.Now;

                        // 创建双向好友关系
                        _context.Friends.Add(new Friend
                        {
                            UserId = friendRequest.FromUserId,
                            FriendId = friendRequest.ToUserId,
                            status = 0,
                            CreateTime = DateTime.Now
                        });

                        _context.Friends.Add(new Friend
                        {
                            UserId = friendRequest.ToUserId,
                            FriendId = friendRequest.FromUserId,
                            status = 0,
                            CreateTime = DateTime.Now
                        });

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Ok(new { success = true, message = "已同意好友申请" });
                    }
                    else if (request.Action.ToLower() == "reject")
                    {
                        // 拒绝申请
                        friendRequest.Status = 2;
                        friendRequest.ProcessTime = DateTime.Now;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Ok(new { success = true, message = "已拒绝好友申请" });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "无效操作" });
                    }
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理好友申请失败");
                return StatusCode(500, new { success = false, message = "处理失败" });
            }
        }

        /// <summary>
        /// 获取好友列表
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult> GetFriendList()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null) return Unauthorized();

                var friends = await _context.Friends
                    .Include(f => f.friend)
                    .ThenInclude(u => u.useraccount)
                    .Where(f => f.UserId == currentUserId.Value && f.status == 0)
                    .OrderByDescending(f => f.CreateTime)
                    .Select(f => new
                    {
                        FriendId = f.FriendId,
                        FriendName = f.friend.useraccount.username,
                        FriendLevel = f.friend.level,
                        IsOnline = f.friend.useraccount.state == 1,
                        CreateTime = f.CreateTime,
                        FriendAvatar = f.friend.logo
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = friends });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取好友列表失败");
                return StatusCode(500, new { success = false, message = "获取失败" });
            }
        }

        /// <summary>
        /// 删除好友
        /// </summary>
        [HttpDelete("{friendId}")]
        public async Task<ActionResult> DeleteFriend(int friendId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null) return Unauthorized();

                // 查找双向好友关系
                var friendship1 = await _context.Friends
                    .FirstOrDefaultAsync(f => f.UserId == currentUserId.Value && f.FriendId == friendId && f.status == 0);

                var friendship2 = await _context.Friends
                    .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == currentUserId.Value && f.status == 0);

                if (friendship1 == null)
                    return NotFound(new { success = false, message = "好友关系不存在" });

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 软删除双向关系
                    friendship1.status = 1;
                    if (friendship2 != null)
                        friendship2.status = 1;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "好友删除成功" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除好友失败");
                return StatusCode(500, new { success = false, message = "删除失败" });
            }
        }
    }

    public class SendRequestDto
    {
        public int ToUserId { get; set; }
        public string Remark { get; set; } = string.Empty;
    }

    public class HandleRequestDto
    {
        public int RequestId { get; set; }
        public string Action { get; set; } = string.Empty;
    }
}