using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using TCserver_Backend.Data;
using TCserver_Backend.Models;

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

        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // 获取所有好友ID（去重）
                var friendIds = await _functionDb.Friends
                    .Where(f => f.status == 0 &&
                               (f.UserId == currentUserId || f.FriendId == currentUserId))
                    .Select(f => f.UserId == currentUserId ? f.FriendId : f.UserId)
                    .Distinct()
                    .ToListAsync();

                // 获取好友详细信息
                var friends = await _functionDb.userdata
                    .Include(u => u.useraccount)
                    .Where(u => friendIds.Contains(u.id))
                    .Select(u => new
                    {
                        id = u.id,
                        username = u.useraccount.username,
                        logo = u.logo,
                        level = u.level,
                        remark = _functionDb.Friends
                            .Where(f => f.UserId == currentUserId && f.FriendId == u.id && f.status == 0)
                            .Select(f => f.Remark)
                            .FirstOrDefault() ?? "",
                        addedTime = _functionDb.Friends
                            .Where(f => (f.UserId == currentUserId && f.FriendId == u.id && f.status == 0) ||
                                       (f.UserId == u.id && f.FriendId == currentUserId && f.status == 0))
                            .Min(f => f.CreateTime)
                    })
                    .OrderByDescending(f => f.addedTime)
                    .ToListAsync();

                return Ok(friends);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取好友列表失败", error = ex.Message });
            }
        }

        // ========== 获取黑名单 ==========
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
                    // 检查是否已存在关系
                    var existingRelation1 = await _functionDb.Friends
                        .AnyAsync(f => f.UserId == currentUserId && f.FriendId == friendRequest.FromUserId && f.status == 0);

                    var existingRelation2 = await _functionDb.Friends
                        .AnyAsync(f => f.UserId == friendRequest.FromUserId && f.FriendId == currentUserId && f.status == 0);

                    if (!existingRelation1)
                    {
                        var friendRelation1 = new Friends
                        {
                            UserId = currentUserId,
                            FriendId = friendRequest.FromUserId,
                            CreateTime = DateTime.Now,
                            status = 0,
                            Remark = friendRequest.Remark
                        };
                        _functionDb.Friends.Add(friendRelation1);
                    }

                    if (!existingRelation2)
                    {
                        var friendRelation2 = new Friends
                        {
                            UserId = friendRequest.FromUserId,
                            FriendId = currentUserId,
                            CreateTime = DateTime.Now,
                            status = 0,
                            Remark = ""
                        };
                        _functionDb.Friends.Add(friendRelation2);
                    }
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

                // 查找所有相关的好友关系
                var friendRelations = await _functionDb.Friends
                    .Where(f =>
                        (f.UserId == currentUserId && f.FriendId == friendId && f.status == 0) ||
                        (f.UserId == friendId && f.FriendId == currentUserId && f.status == 0)
                    )
                    .ToListAsync();

                if (friendRelations.Count == 0)
                {
                    return NotFound(new { message = "好友关系不存在" });
                }

                // 软删除所有关系
                foreach (var relation in friendRelations)
                {
                    relation.status = 1;
                }

                await _functionDb.SaveChangesAsync();

                return Ok(new { message = "好友删除成功", deletedCount = friendRelations.Count });
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

        // ========== 清理重复好友关系 ==========
        [HttpPost("cleanup-duplicates")]
        public async Task<IActionResult> CleanupDuplicateFriends()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // 查找重复的好友关系
                var duplicates = await _functionDb.Friends
                    .Where(f => f.UserId == currentUserId && f.status == 0)
                    .GroupBy(f => f.FriendId)
                    .Where(g => g.Count() > 1)
                    .Select(g => new
                    {
                        FriendId = g.Key,
                        Count = g.Count(),
                        Relations = g.OrderByDescending(f => f.CreateTime).ToList()
                    })
                    .ToListAsync();

                var deletedCount = 0;

                foreach (var duplicate in duplicates)
                {
                    // 保留最新的关系，删除其他重复的
                    var relationsToDelete = duplicate.Relations.Skip(1);

                    foreach (var relation in relationsToDelete)
                    {
                        relation.status = 1; // 软删除
                        deletedCount++;
                    }
                }

                await _functionDb.SaveChangesAsync();

                return Ok(new
                {
                    message = "清理完成",
                    deletedCount,
                    duplicateGroups = duplicates.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "清理重复好友失败", error = ex.Message });
            }
        }

        // ========== 检查好友关系 ==========
        [HttpGet("check/{friendId}")]
        public async Task<IActionResult> CheckFriendship(int friendId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var isMyFriend = await _functionDb.Friends
                    .AnyAsync(f => f.UserId == currentUserId && f.FriendId == friendId && f.status == 0);

                var isTheirFriend = await _functionDb.Friends
                    .AnyAsync(f => f.UserId == friendId && f.FriendId == currentUserId && f.status == 0);

                return Ok(new
                {
                    isMyFriend,
                    isTheirFriend,
                    isMutualFriendship = isMyFriend && isTheirFriend
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "检查好友关系失败", error = ex.Message });
            }
        }

        // ========== 修复单向好友关系 ==========
        [HttpPost("fix-oneway/{friendId}")]
        public async Task<IActionResult> FixOneWayFriendship(int friendId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // 检查是否已经是双向好友
                var isMyFriend = await _functionDb.Friends
                    .AnyAsync(f => f.UserId == currentUserId && f.FriendId == friendId && f.status == 0);

                var isTheirFriend = await _functionDb.Friends
                    .AnyAsync(f => f.UserId == friendId && f.FriendId == currentUserId && f.status == 0);

                if (isMyFriend && isTheirFriend)
                {
                    return Ok(new { message = "已经是双向好友关系" });
                }

                // 补充缺失的关系
                if (!isMyFriend)
                {
                    var relation1 = new Friends
                    {
                        UserId = currentUserId,
                        FriendId = friendId,
                        CreateTime = DateTime.Now,
                        status = 0,
                        Remark = ""
                    };
                    _functionDb.Friends.Add(relation1);
                }

                if (!isTheirFriend)
                {
                    var relation2 = new Friends
                    {
                        UserId = friendId,
                        FriendId = currentUserId,
                        CreateTime = DateTime.Now,
                        status = 0,
                        Remark = ""
                    };
                    _functionDb.Friends.Add(relation2);
                }

                await _functionDb.SaveChangesAsync();

                return Ok(new { message = "好友关系已修复为双向" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "修复好友关系失败", error = ex.Message });
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