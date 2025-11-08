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
    public class FriendsController : ControllerBase
    {
        private readonly FunctionDbContext _functionDb;

        public FriendsController(FunctionDbContext functionDb)
        {
            _functionDb = functionDb;
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

        // ========== 新增：获取好友列表 ==========
        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var friends = await _functionDb.Friends
                    .Include(f => f.Friend)
                    .ThenInclude(u => u.useraccount)
                    .Where(f => f.UserId == currentUserId && f.status == 0)
                    .OrderByDescending(f => f.CreateTime)
                    .Select(f => new
                    {
                        id = f.FriendId,
                        username = f.Friend.useraccount.username,
                        logo = f.Friend.logo,
                        level = f.Friend.level,
                        remark = f.Remark,
                        addedTime = f.CreateTime
                    })
                    .ToListAsync();

                return Ok(friends);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取好友列表失败", error = ex.Message });
            }
        }

        // ========== 新增：获取黑名单 ==========
        [HttpGet("blocked")]
        public async Task<IActionResult> GetBlockedUsers()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var blockedUsers = await _functionDb.BlockUsers
                    .Include(b => b.BlockedUser)
                    .ThenInclude(u => u.useraccount)
                    .Where(b => b.UserId == currentUserId)
                    .OrderByDescending(b => b.CreateTime)
                    .Select(b => new
                    {
                        id = b.BlockID,
                        username = b.BlockedUser.useraccount.username,
                        logo = b.BlockedUser.logo,
                        level = b.BlockedUser.level,
                        blockedTime = b.CreateTime
                    })
                    .ToListAsync();

                return Ok(blockedUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取黑名单失败", error = ex.Message });
            }
        }

        // ========== 搜索用户 ==========
        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> SearchUsers(string keyword)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var users = await _functionDb.userdata
                    .Include(u => u.useraccount)
                    .Where(u => u.id.ToString().Contains(keyword) ||
                               u.useraccount.username.Contains(keyword))
                    .Where(u => u.id != currentUserId)
                    .Select(u => new
                    {
                        id = u.id,
                        username = u.useraccount.username,
                        logo = u.logo,
                        level = u.level,
                        lastActive = u.last_active_time,
                        isFriend = _functionDb.Friends.Any(f =>
                            f.UserId == currentUserId && f.FriendId == u.id && f.status == 0),
                        isBlocked = _functionDb.BlockUsers.Any(b =>
                            b.UserId == currentUserId && b.BlockID == u.id)
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "搜索失败", error = ex.Message });
            }
        }

        // ========== 发送好友请求 ==========
        [HttpPost("request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto requestDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (currentUserId == requestDto.ToUserId)
                {
                    return BadRequest(new { message = "不能向自己发送好友请求" });
                }

                // 检查目标用户是否存在
                var targetUser = await _functionDb.userdata.FindAsync(requestDto.ToUserId);
                if (targetUser == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 检查是否已经是好友
                var isAlreadyFriend = await _functionDb.Friends
                    .AnyAsync(f => f.UserId == currentUserId && f.FriendId == requestDto.ToUserId && f.status == 0);

                if (isAlreadyFriend)
                {
                    return BadRequest(new { message = "已经是好友关系" });
                }

                // 检查是否已有待处理的好友请求
                var existingRequest = await _functionDb.FriendRequests
                    .FirstOrDefaultAsync(fr =>
                        fr.FromUserId == currentUserId &&
                        fr.ToUserId == requestDto.ToUserId &&
                        fr.Status == 0);

                if (existingRequest != null)
                {
                    return BadRequest(new { message = "已发送过好友请求，请等待对方处理" });
                }

                // 检查是否被拉黑
                var isBlocked = await _functionDb.BlockUsers
                    .AnyAsync(b => b.UserId == requestDto.ToUserId && b.BlockID == currentUserId);

                if (isBlocked)
                {
                    return BadRequest(new { message = "无法向该用户发送好友请求" });
                }

                // 创建好友请求
                var friendRequest = new FriendRequest
                {
                    FromUserId = currentUserId,
                    ToUserId = requestDto.ToUserId,
                    Remark = requestDto.Remark ?? "",
                    Status = 0,
                    CreateTime = DateTime.Now
                };

                _functionDb.FriendRequests.Add(friendRequest);
                await _functionDb.SaveChangesAsync();

                return Ok(new { message = "好友请求发送成功", requestId = friendRequest.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "发送好友请求失败", error = ex.Message });
            }
        }

        // ========== 处理好友请求 ==========
        [HttpPost("request/{requestId}/process")]
        public async Task<IActionResult> ProcessFriendRequest(int requestId, [FromBody] ProcessFriendRequestDto processDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var friendRequest = await _functionDb.FriendRequests
                    .Include(fr => fr.FromUser)
                    .ThenInclude(u => u.useraccount)
                    .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ToUserId == currentUserId);

                if (friendRequest == null)
                {
                    return NotFound(new { message = "好友请求不存在" });
                }

                if (friendRequest.Status != 0)
                {
                    return BadRequest(new { message = "该好友请求已被处理" });
                }

                friendRequest.Status = processDto.Action;
                friendRequest.ProcessTime = DateTime.Now;

                if (processDto.Action == 1) // 同意
                {
                    // 创建双向好友关系
                    var friendRelation1 = new Friends
                    {
                        UserId = friendRequest.FromUserId,
                        FriendId = friendRequest.ToUserId,
                        CreateTime = DateTime.Now,
                        status = 0,
                        Remark = ""
                    };

                    var friendRelation2 = new Friends
                    {
                        UserId = friendRequest.ToUserId,
                        FriendId = friendRequest.FromUserId,
                        CreateTime = DateTime.Now,
                        status = 0,
                        Remark = friendRequest.Remark
                    };

                    _functionDb.Friends.Add(friendRelation1);
                    _functionDb.Friends.Add(friendRelation2);
                }

                await _functionDb.SaveChangesAsync();

                var actionText = processDto.Action == 1 ? "同意" : "拒绝";
                return Ok(new { message = $"已{actionText}好友请求" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "处理好友请求失败", error = ex.Message });
            }
        }

        // ========== 获取收到的好友请求 ==========
        [HttpGet("requests/received")]
        public async Task<IActionResult> GetReceivedFriendRequests()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var requests = await _functionDb.FriendRequests
                    .Include(fr => fr.FromUser)
                    .ThenInclude(u => u.useraccount)
                    .Where(fr => fr.ToUserId == currentUserId && fr.Status == 0)
                    .OrderByDescending(fr => fr.CreateTime)
                    .Select(fr => new
                    {
                        id = fr.Id,
                        fromUserId = fr.FromUserId,
                        fromUserName = fr.FromUser.useraccount.username,
                        fromUserLogo = fr.FromUser.logo,
                        remark = fr.Remark,
                        createTime = fr.CreateTime
                    })
                    .ToListAsync();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取好友请求失败", error = ex.Message });
            }
        }

        // ========== 获取发送的好友请求 ==========
        [HttpGet("requests/sent")]
        public async Task<IActionResult> GetSentFriendRequests()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var requests = await _functionDb.FriendRequests
                    .Include(fr => fr.ToUser)
                    .ThenInclude(u => u.useraccount)
                    .Where(fr => fr.FromUserId == currentUserId)
                    .OrderByDescending(fr => fr.CreateTime)
                    .Select(fr => new
                    {
                        id = fr.Id,
                        toUserId = fr.ToUserId,
                        toUserName = fr.ToUser.useraccount.username,
                        toUserLogo = fr.ToUser.logo,
                        status = fr.Status,
                        remark = fr.Remark,
                        createTime = fr.CreateTime,
                        processTime = fr.ProcessTime
                    })
                    .ToListAsync();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取发送的好友请求失败", error = ex.Message });
            }
        }

        // ========== 取消好友请求 ==========
        [HttpDelete("request/{requestId}")]
        public async Task<IActionResult> CancelFriendRequest(int requestId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var friendRequest = await _functionDb.FriendRequests
                    .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.FromUserId == currentUserId);

                if (friendRequest == null)
                {
                    return NotFound(new { message = "好友请求不存在" });
                }

                if (friendRequest.Status != 0)
                {
                    return BadRequest(new { message = "无法取消已处理的好友请求" });
                }

                _functionDb.FriendRequests.Remove(friendRequest);
                await _functionDb.SaveChangesAsync();

                return Ok(new { message = "好友请求已取消" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "取消好友请求失败", error = ex.Message });
            }
        }

        // ========== 删除好友 ==========
        [HttpDelete("{friendId}")]
        public async Task<IActionResult> DeleteFriend(int friendId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var friendRelation = await _functionDb.Friends
                    .FirstOrDefaultAsync(f => f.UserId == currentUserId && f.FriendId == friendId && f.status == 0);

                if (friendRelation == null)
                {
                    return NotFound(new { message = "好友关系不存在" });
                }

                friendRelation.status = 1;
                await _functionDb.SaveChangesAsync();

                return Ok(new { message = "好友删除成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "删除好友失败", error = ex.Message });
            }
        }

        // ========== 取消拉黑 ==========
        [HttpDelete("blocked/{blockedUserId}")]
        public async Task<IActionResult> UnblockUser(int blockedUserId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var blockedUser = await _functionDb.BlockUsers
                    .FirstOrDefaultAsync(b => b.UserId == currentUserId && b.BlockID == blockedUserId);

                if (blockedUser == null)
                {
                    return NotFound(new { message = "该用户不在黑名单中" });
                }

                _functionDb.BlockUsers.Remove(blockedUser);
                await _functionDb.SaveChangesAsync();

                return Ok(new { message = "已取消拉黑" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "取消拉黑失败", error = ex.Message });
            }
        }
    }

    // DTO类
    public class SendFriendRequestDto
    {
        public int ToUserId { get; set; }
        public string Remark { get; set; }
    }

    public class ProcessFriendRequestDto
    {
        public int Action { get; set; } // 1-同意，2-拒绝
    }
}