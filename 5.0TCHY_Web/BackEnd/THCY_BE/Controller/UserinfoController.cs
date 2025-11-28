using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using THCY_BE.DataBase;
using THCY_BE.Models.UserDate;

namespace THCY_BE.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需携带 Bearer Token 才能访问
    [Produces("application/json")]
    public class UserinfoController : ControllerBase
    {
        private readonly UserDataDbContext _db;

        public UserinfoController(UserDataDbContext db)
        {
            _db = db;
        }

        
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<UserFullInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<UserFullInfoDto>>> GetAllUsers()
        {
            // 检查当前用户是否有管理员权限
            var currentUser = await FindCurrentUserAsync();
            if (currentUser is null) return Unauthorized();

            if (currentUser.rank < 1) // 需要管理员权限 (rank >= 2)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "需要管理员权限" });
            }

            try
            {
                // 获取所有用户，包含关联数据
                var allUsers = await _db.Set<UserAccount>()
                    .Include(u => u.userdata)
                    .Include(u => u.Posts)
                    .Include(u => u.Comments)
                    .AsNoTracking()
                    .OrderByDescending(u => u.date) // 按注册时间倒序
                    .ToListAsync();

                var result = allUsers.Select(ua => new UserFullInfoDto
                {
                    // 账户信息
                    id = ua.Id,
                    username = ua.username,
                    email_address = ua.email_address,
                    date = ua.date,
                    state = ua.state,
                    rank = ua.rank,
                    is_verified = ua.is_verified,
                    failed_login_attempts = ua.failed_login_attempts,
                    last_failed_login = ua.last_failed_login,

                    // 用户数据
                    points = ua.userdata?.points ?? 0,
                    level = ua.userdata?.level ?? 0,
                    exp = ua.userdata?.exp ?? 0,
                    title = ua.userdata?.title ?? string.Empty,
                    lastlogin = ua.userdata?.lastlogin ?? DateTime.MinValue,
                    logo = ua.userdata?.logo ?? string.Empty,
                    background = ua.userdata?.background ?? string.Empty,
                    likes = ua.userdata?.likes ?? 0,
                    last_active_time = ua.userdata?.last_active_time,
                    byd = ua.userdata?.byd ?? 0,
                    creater = ua.userdata?.creater ?? 0,

                    // 统计信息
                    post_count = ua.Posts?.Count ?? 0,
                    comment_count = ua.Comments?.Count ?? 0
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // 记录异常
               
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "获取用户数据失败" });
            }
        }

        /// <summary>
        /// 获取当前登录用户的基础信息（含 rank）
        /// </summary>
        /// <returns>{ id, username, rank }</returns>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserInfoDto>> Me()
        {
            var ua = await FindCurrentUserAsync();
            if (ua is null) return Unauthorized();

            return Ok(new UserInfoDto
            {
                id = ua.Id,
                username = ua.username,
                rank = ua.rank
            });
        }

        /// <summary>
        /// 基于 rank 的访问校验。前端每次进入敏感页面前调用此接口，后端实时从数据库判定是否允许进入。
        /// </summary>
        /// <param name="minRank">所需最小 rank（默认 1）</param>
        /// <returns>200: { allowed: true, rank }；403: { allowed: false, rank, required }</returns>
        [HttpGet("authorize")]
        [ProducesResponseType(typeof(AuthorizeResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthorizeResultDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AuthorizeByRank([FromQuery] int minRank = 1)
        {
            if (minRank < 1) minRank = 1;

            var ua = await FindCurrentUserAsync();
            if (ua is null) return Unauthorized();

            if (ua.rank >= minRank)
            {
                return Ok(new AuthorizeResultDto
                {
                    allowed = true,
                    rank = ua.rank,
                    required = minRank
                });
            }

            return StatusCode(StatusCodes.Status403Forbidden, new AuthorizeResultDto
            {
                allowed = false,
                rank = ua.rank,
                required = minRank
            });
        }

        

        private async Task<UserAccount?> FindCurrentUserAsync()
        {
            // 1) 优先从 token 中的用户 Id（sub/NameIdentifier）获取
            var id = GetUserIdFromClaims();
            if (id.HasValue)
            {
                return await _db.Set<UserAccount>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Id == id.Value);
            }

            // 2) 退化到用户名（Name/username）
            var uname = GetUsernameFromClaims();
            if (!string.IsNullOrWhiteSpace(uname))
            {
                return await _db.Set<UserAccount>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.username == uname);
            }

            return null;
        }

        private int? GetUserIdFromClaims()
        {
            var c = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (c != null && int.TryParse(c.Value, out var id)) return id;
            return null;
        }

        private string? GetUsernameFromClaims()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("username")?.Value
                ?? User.Identity?.Name;
        }




        /// <summary>
        /// 更新用户信息（管理员权限）
        /// </summary>
        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserDto updateDto)
        {
            // 检查当前用户权限
            var currentUser = await FindCurrentUserAsync();
            if (currentUser is null) return Unauthorized();

            if (currentUser.rank < 1) // 需要管理员权限
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "需要管理员权限" });
            }

            try
            {
                // 获取要更新的用户
                var user = await _db.Set<UserAccount>()
                    .Include(u => u.userdata)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user is null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 检查权限：不能修改比自己rank高的用户
                if (user.rank >= currentUser.rank)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "不能修改权限等级高于或等于您的用户" });
                }

                // 更新用户账户信息
                if (updateDto.Rank.HasValue) user.rank = updateDto.Rank.Value;
                if (updateDto.State.HasValue) user.state = updateDto.State.Value;

                // 更新用户数据
                if (user.userdata != null)
                {
                    if (updateDto.Level.HasValue) user.userdata.level = updateDto.Level.Value;
                    if (updateDto.Points.HasValue) user.userdata.points = updateDto.Points.Value;
                    if (updateDto.Exp.HasValue) user.userdata.exp = updateDto.Exp.Value;
                    if (updateDto.Title != null) user.userdata.title = updateDto.Title;
                    if (updateDto.Creater.HasValue) user.userdata.creater = updateDto.Creater.Value;
                    if (updateDto.Likes.HasValue) user.userdata.likes = updateDto.Likes.Value;
                }

              
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "用户信息更新成功" });
            }
            catch (Exception ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "更新失败" });
            }
        }

        // 更新用户DTO
        public class UpdateUserDto
        {
            public int? Rank { get; set; }
            public int? State { get; set; }
            public int? Level { get; set; }
            public int? Points { get; set; }
            public int? Exp { get; set; }
            public string? Title { get; set; }
            public int? Creater { get; set; }
            public int? Likes { get; set; }
        }
    }

    // DTOs
    public class UserInfoDto
    {
        public int id { get; set; }
        public string username { get; set; } = string.Empty;
        public int rank { get; set; }
    }

    public class AuthorizeResultDto
    {
        public bool allowed { get; set; }
        public int rank { get; set; }
        public int required { get; set; }
    }

    public class UserFullInfoDto
    {
        // 账户信息
        public int id { get; set; }
        public string username { get; set; } = string.Empty;
        public string email_address { get; set; } = string.Empty;
        public DateTime date { get; set; }
        public int state { get; set; }
        public int rank { get; set; }
        public bool is_verified { get; set; }
        public int failed_login_attempts { get; set; }
        public DateTime? last_failed_login { get; set; }

        // 用户数据
        public int points { get; set; }
        public int level { get; set; }
        public int exp { get; set; }
        public string title { get; set; } = string.Empty;
        public DateTime lastlogin { get; set; }
        public string logo { get; set; } = string.Empty;
        public string background { get; set; } = string.Empty;
        public int likes { get; set; }
        public DateTime? last_active_time { get; set; }
        public int byd { get; set; }
        public int creater { get; set; }

        // 统计信息
        public int post_count { get; set; }
        public int comment_count { get; set; }
    }
}