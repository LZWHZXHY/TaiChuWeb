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

        // ========= 私有辅助 =========

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
}