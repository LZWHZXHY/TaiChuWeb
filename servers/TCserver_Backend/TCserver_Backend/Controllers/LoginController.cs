using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TCserver_Backend.Data;
using TCserver_Backend.Models;
using TCserver_Backend.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly RegisterContext _context;
        private readonly IPasswordHasher<useraccount> _passwordHasher;
        private readonly ILogger<LoginController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly JwtService _jwtService;
        private readonly RefreshTokenService _refreshTokenService;

        public LoginController(
            JwtService jwtService,
            RefreshTokenService refreshTokenService,
            RegisterContext context,
            IPasswordHasher<useraccount> passwordHasher,
            ILogger<LoginController> logger,
            IWebHostEnvironment environment)
        {
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _environment = environment;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                // 验证输入
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    _logger.LogWarning("用户名或密码为空");
                    return BadRequest(new { success = false, message = "用户名和密码不能为空" });
                }

                // 使用EF Core查询用户
                var user = await _context.UserAccounts
                    .FirstOrDefaultAsync(u => u.username == model.Username || u.email_address == model.Username);

                if (user == null)
                {
                    _logger.LogWarning($"未找到用户: {model.Username}");
                    return Unauthorized(new { success = false, message = "用户名或密码不正确" });
                }

                // 检查账户是否被锁定
                const int maxAttempts = 5;
                const int lockoutMinutes = 15;

                if (user.failed_login_attempts >= maxAttempts &&
                    user.last_failed_login > DateTime.UtcNow.AddMinutes(-lockoutMinutes))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "账户已锁定，请稍后再试"
                    });
                }





                // 将字节数组转换为字符串
                string passwordHashString = Encoding.UTF8.GetString(user.password_hash);



                // 验证密码
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                    user,
                    passwordHashString,
                    model.Password
                );

                if (passwordVerificationResult != PasswordVerificationResult.Success)
                {
                    // 更新失败尝试次数
                    user.failed_login_attempts++;
                    user.last_failed_login = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogWarning($"密码验证失败: {model.Username}");
                    return Unauthorized(new { success = false, message = "用户名或密码不正确" });
                }

                // 登录成功重置尝试次数
                user.failed_login_attempts = 0;
                await _context.SaveChangesAsync();

                // 获取用户数据
                var userData = await _context.UserDatas.FirstOrDefaultAsync(u => u.id == user.Id);

                // 创建声明
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, user.username),
                    new Claim(ClaimTypes.Email, user.email_address),
                    new Claim("rank", user.rank.ToString()),
                    new Claim("is_verified", user.is_verified.ToString())
                };

                // 添加从userdata获取的字段
                if (userData != null)
                {
                    claims.Add(new Claim("level", userData.level.ToString()));
                    claims.Add(new Claim("points", userData.points.ToString()));
                    claims.Add(new Claim("exp", userData.exp.ToString()));
                    claims.Add(new Claim("title", userData.title ?? ""));
                }
                else
                {
                    // 设置默认值
                    claims.Add(new Claim("level", "0"));
                    claims.Add(new Claim("points", "0"));
                    claims.Add(new Claim("exp", "0"));
                    claims.Add(new Claim("title", "初入太初"));
                }

                // 生成访问令牌和刷新令牌
                var accessToken = _jwtService.GenerateAccessToken(claims);
                var refreshToken = _jwtService.GenerateRefreshToken();


                int refreshTokenExpiryDays = model.RememberMe ? 7 : 1;


                // 保存刷新令牌到数据库
                await _refreshTokenService.SaveRefreshToken(user.Id, refreshToken, refreshTokenExpiryDays);
                _logger.LogInformation($"Login success for: {model.Username}, accessToken: {(accessToken?.Length ?? 0)} chars");

                // 返回登录成功信息
                return Ok(new
                {
                    success = true,
                    message = "登录成功",
                    accessToken,
                    token = accessToken, // <--- 新增
                    refreshToken,
                    expiresIn = 30 * 60, // 30分钟有效期
                    rememberMe = model.RememberMe, // 返回记住我状态
                    user = new
                    {
                        id = user.Id,
                        username = user.username,
                        email = user.email_address,
                        state = user.state,
                        rank = user.rank,
                        is_verified = user.is_verified,
                        points = userData?.points ?? 0,
                        level = userData?.level ?? 0,
                        exp = userData?.exp ?? 0,
                        title = userData?.title ?? "",
                        lastlogin = userData?.lastlogin ?? DateTime.UtcNow,
                        logo = userData?.logo ?? "",
                        background = userData?.background ?? "",
                        byd = userData?.byd ?? 0,
                        creater = userData?.creater ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"登录过程中发生异常: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "服务器错误，请稍后再试",
                    details = _environment.IsDevelopment() ? ex.ToString() : null
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.AccessToken) || string.IsNullOrEmpty(model.RefreshToken))
                {
                    return BadRequest(new { success = false, message = "令牌不能为空" });
                }

                // 验证访问令牌
                var principal = _jwtService.GetPrincipalFromToken(model.AccessToken);
                var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));

                // 验证刷新令牌
                var isValid = await _refreshTokenService.ValidateRefreshToken(userId, model.RefreshToken);
                if (!isValid)
                {
                    return Unauthorized(new { success = false, message = "无效的刷新令牌" });
                }

                // 生成新令牌
                var newAccessToken = _jwtService.GenerateAccessToken(principal.Claims);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // 更新刷新令牌
                await _refreshTokenService.UpdateRefreshToken(userId, model.RefreshToken, newRefreshToken);

                return Ok(new
                {
                    success = true,
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken,
                    expiresIn = 30 * 60 // 30分钟
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新令牌失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "刷新令牌失败",
                    details = _environment.IsDevelopment() ? ex.ToString() : null
                });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.RefreshToken))
                {
                    return BadRequest(new { success = false, message = "刷新令牌不能为空" });
                }

                // 验证访问令牌
                var principal = _jwtService.GetPrincipalFromToken(model.AccessToken);
                var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));

                // 撤销刷新令牌
                await _refreshTokenService.RevokeRefreshToken(userId, model.RefreshToken);

                return Ok(new
                {
                    success = true,
                    message = "注销成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注销失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "注销失败",
                    details = _environment.IsDevelopment() ? ex.ToString() : null
                });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}