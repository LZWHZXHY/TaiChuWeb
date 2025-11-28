using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using THCY_BE.DataBase;
using THCY_BE.Dto.Friends;
using THCY_BE.Models.Friends;

namespace THCY_BE.Controller.Friends
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需携带 Bearer Token 才能访问
    public class FriendsController : ControllerBase
    {
        private readonly FriendsDbContext _context;
        private readonly ILogger<FriendsController> _logger;
        public FriendsController(FriendsDbContext context, ILogger<FriendsController> logger)
        {
            _context = context;
            _logger = logger;
        }


        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
        /// <summary>
        /// 搜索用户（用于添加好友）
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchUsersResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SearchUsersResponseDto>> SearchUsers(
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // 1. 验证当前用户
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new SearchUsersResponseDto
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 2. 验证搜索关键词
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new SearchUsersResponseDto
                    {
                        Success = false,
                        Message = "搜索关键词不能为空"
                    });
                }

                if (keyword.Length < 2)
                {
                    return BadRequest(new SearchUsersResponseDto
                    {
                        Success = false,
                        Message = "搜索关键词至少需要2个字符"
                    });
                }

                // 3. 分页验证
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 20;

                // 4. 搜索用户
                var query = from ua in _context.UserAccounts
                            join ud in _context.UserDatas on ua.Id equals ud.id
                            where ua.Id != currentUserId.Value && // 排除自己
                                  ua.state != 2 && // 排除封禁用户
                                  (ua.username.Contains(keyword) ||
                                   ua.Id.ToString().Contains(keyword) ||
                                   (ud.title != null && ud.title.Contains(keyword)))
                            select new UserSearchResultDto
                            {
                                Id = ua.Id,
                                Username = ua.username,
                                Email = ua.email_address,
                                Level = ud.level,
                                Points = ud.points,
                                Exp = ud.exp,
                                Title = ud.title,
                                Logo = ud.logo,
                                Rank = ua.rank,
                                State = ua.state,
                                IsVerified = ua.is_verified,
                                LastActiveTime = ud.last_active_time,
                                Likes = ud.likes,
                                Byd = ud.byd,
                                Creater = ud.creater,
                                // 关系状态
                                IsFriend = _context.Friends.Any(f =>
                                    f.UserId == currentUserId.Value &&
                                    f.FriendId == ua.Id &&
                                    f.status == 0),
                                HasPendingRequest = _context.FriendRequests.Any(r =>
                                    r.FromUserId == currentUserId.Value &&
                                    r.ToUserId == ua.Id &&
                                    r.Status == 0),
                                HasReceivedRequest = _context.FriendRequests.Any(r =>
                                    r.FromUserId == ua.Id &&
                                    r.ToUserId == currentUserId.Value &&
                                    r.Status == 0)
                            };

                // 5. 获取总数和分页数据
                var totalCount = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.Level) // 按等级排序
                    .ThenByDescending(u => u.Points) // 然后按积分排序
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 6. 返回结果
                return Ok(new SearchUsersResponseDto
                {
                    Success = true,
                    Message = "搜索成功",
                    Data = new SearchUsersDataDto
                    {
                        TotalCount = totalCount,
                        Page = page,
                        PageSize = pageSize,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                        Users = users
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索用户失败");
                return StatusCode(500, new SearchUsersResponseDto
                {
                    Success = false,
                    Message = "搜索失败，请重试"
                });
            }
        }
        /// <summary>
        /// 发送好友请求
        /// </summary>
        [HttpPost("request")]
        [ProducesResponseType(typeof(SendFriendRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SendFriendRequestResponseDto>> SendFriendRequest([FromBody] SendFriendRequestDto requestDto)
        {
            try
            {
                // 1. 验证当前用户
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new SendFriendRequestResponseDto
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 2. 基本验证
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new SendFriendRequestResponseDto
                    {
                        Success = false,
                        Message = string.Join("; ", errors)
                    });
                }

                // 3. 检查不能添加自己
                if (currentUserId.Value == requestDto.ToUserId)
                {
                    return BadRequest(new SendFriendRequestResponseDto
                    {
                        Success = false,
                        Message = "不能添加自己为好友"
                    });
                }

                // 4. 检查目标用户是否存在且状态正常
                var targetUser = await _context.UserAccounts
                    .Include(u => u.userdata)
                    .FirstOrDefaultAsync(u => u.Id == requestDto.ToUserId);

                if (targetUser == null)
                {
                    return BadRequest(new SendFriendRequestResponseDto
                    {
                        Success = false,
                        Message = "目标用户不存在"
                    });
                }

                if (targetUser.state == 2) // 被封禁
                {
                    return BadRequest(new SendFriendRequestResponseDto
                    {
                        Success = false,
                        Message = "目标用户已被封禁，无法添加"
                    });
                }

                // 5. 检查是否已是好友
                var existingFriendship = await _context.Friends
                    .AnyAsync(f =>
                        f.UserId == currentUserId.Value &&
                        f.FriendId == requestDto.ToUserId &&
                        f.status == 0);

                if (existingFriendship)
                {
                    return BadRequest(new SendFriendRequestResponseDto
                    {
                        Success = false,
                        Message = "该用户已经是您的好友"
                    });
                }

                // 6. 检查是否有待处理的请求
                var existingRequest = await _context.FriendRequests
                    .FirstOrDefaultAsync(r =>
                        r.FromUserId == currentUserId.Value &&
                        r.ToUserId == requestDto.ToUserId &&
                        r.Status == 0); // 0=待处理

                if (existingRequest != null)
                {
                    return BadRequest(new SendFriendRequestResponseDto
                    {
                        Success = false,
                        Message = "已向该用户发送过好友请求，请等待对方处理"
                    });
                }

                // 7. 检查对方是否已向你发送请求（避免互相发送）
                var reverseRequest = await _context.FriendRequests
                    .FirstOrDefaultAsync(r =>
                        r.FromUserId == requestDto.ToUserId &&
                        r.ToUserId == currentUserId.Value &&
                        r.Status == 0);

                if (reverseRequest != null)
                {
                    return BadRequest(new SendFriendRequestResponseDto
                    {
                        Success = false,
                        Message = "对方已向您发送好友请求，请先处理"
                    });
                }

                // 8. 创建好友请求
                var friendRequest = new FriendRequest
                {
                    FromUserId = currentUserId.Value,
                    ToUserId = requestDto.ToUserId,
                    Remark = requestDto.Remark?.Trim() ?? "",
                    Status = 0, // 待处理
                    CreateTime = DateTime.Now
                };

                // 使用事务确保数据一致性
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.FriendRequests.Add(friendRequest);
                    await _context.SaveChangesAsync();

                    // 更新目标用户的最后活跃时间（可选）
                    if (targetUser.userdata != null)
                    {
                        targetUser.userdata.last_active_time = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("用户 {FromUserId} 向用户 {ToUserId} 发送好友请求成功",
                        currentUserId, requestDto.ToUserId);

                    return Ok(new SendFriendRequestResponseDto
                    {
                        Success = true,
                        Message = "好友请求发送成功",
                        RequestId = friendRequest.Id
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw; // 重新抛出异常，由外层catch处理
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "数据库更新失败");
                return StatusCode(500, new SendFriendRequestResponseDto
                {
                    Success = false,
                    Message = "数据库操作失败，请重试"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送好友请求失败");
                return StatusCode(500, new SendFriendRequestResponseDto
                {
                    Success = false,
                    Message = "发送好友请求失败，请重试"
                });
            }
        }


    }
}
