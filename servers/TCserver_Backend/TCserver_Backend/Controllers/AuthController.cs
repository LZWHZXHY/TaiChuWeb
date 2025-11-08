using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TCserver_Backend.Data;
using TCserver_Backend.Dtos;
using TCserver_Backend.Models;
using TCserver_Backend.Services;



namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterContext _db;
        private readonly IPasswordHasher<useraccount> _passwordHasher;
        private readonly ILogger<AuthController> _logger;
        private readonly EmailSender _emailSender;
        private readonly IHostEnvironment _env;
        private readonly IWebHostEnvironment _environment;
        private readonly JwtService _jwtService;


        public AuthController(
            JwtService jwtService,
            RegisterContext db,
            IPasswordHasher<useraccount> passwordHasher,
            ILogger<AuthController> logger,
            EmailSender emailSender,
            IHostEnvironment env,
            IWebHostEnvironment environment)
        {
            _jwtService = jwtService;
            _db = db;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _emailSender = emailSender;
            _env = env;
            _environment = environment;


        }


        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendCodeDto dto)
        {
            _logger.LogInformation($"收到发送验证码请求: {dto.email_address}, 用户名: {dto.username}");

            try
            {
                // 1. 检查邮箱是否已被注册
                if (await _db.UserAccounts.AnyAsync(u => u.email_address == dto.email_address))
                    return BadRequest(new { message = "邮箱已经被注册过了" });

                // 2. 生成6位随机验证码
                var random = new Random();
                var code = random.Next(100000, 999999).ToString();

                // 3. 保存验证码到数据库
                var verification = new EmailVerification
                {
                    Email = dto.email_address,
                    Code = code,
                    ExpireTime = DateTime.UtcNow.AddMinutes(5) // 5分钟有效
                };

                // 如果已有记录，则更新
                var existing = await _db.EmailVerifications.FirstOrDefaultAsync(e => e.Email == dto.email_address);
                if (existing != null)
                {
                    existing.Code = code;
                    existing.ExpireTime = verification.ExpireTime;
                }
                else
                {
                    _db.EmailVerifications.Add(verification);
                }

                await _db.SaveChangesAsync();


                await _emailSender.SendVerificationCodeAsync(dto.email_address, dto.username, code);
                _logger.LogInformation($"邮件已发送到: {dto.email_address}");

                return Ok(new { message = "验证码已发送，请查收邮件" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送验证码过程中发生错误");
                return StatusCode(500, new
                {
                    message = "发送验证码失败，请稍后重试。",
                    error = ex.Message
                });
            }
        }

        [HttpPost("register-with-code")]
        public async Task<IActionResult> RegisterWithCode([FromBody] RegisterWithCodeDto dto)
        {
            _logger.LogInformation($"收到验证码注册请求: {dto.username}, {dto.email_address}");
            Console.WriteLine("注册验证测试！=======》》》》》》》》》》》》");

            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    // 1. 验证验证码
                    var verification = await _db.EmailVerifications
                        .FirstOrDefaultAsync(e => e.Email == dto.email_address && e.Code == dto.verification_code);

                    if (verification == null)
                        return BadRequest(new { message = "验证码无效" });

                    if (verification.ExpireTime < DateTime.UtcNow)
                        return BadRequest(new { message = "验证码已过期" });

                    // 2. 检查用户名是否可用
                    if (await _db.UserAccounts.AnyAsync(u => u.username == dto.username))
                        return BadRequest(new { message = "用户名已存在" });

                    // 3. 检查邮箱是否可用
                    if (await _db.UserAccounts.AnyAsync(u => u.email_address == dto.email_address))
                        return BadRequest(new { message = "邮箱已注册" });

                    // 4. 创建用户账户
                    var user = new useraccount
                    {
                        username = dto.username,
                        email_address = dto.email_address,
                        is_verified = true,
                        rank = 0,
                        state = 1,
                        date = DateTime.UtcNow,
                    };

                    // 5. 哈希密码
                    // 在注册方法中
                    var hashedPassword = _passwordHasher.HashPassword(user, dto.password_hash);
                    user.password_hash = Encoding.UTF8.GetBytes(hashedPassword);

                    // 6. 保存用户账户
                    _db.UserAccounts.Add(user);
                    await _db.SaveChangesAsync(); // 先保存用户账户以获取Id
                    _logger.LogInformation($"用户已保存，ID: {user.Id}");

                    // 7. 创建用户扩展数据
                    var userData = new userdata
                    {
                        id = user.Id,
                        points = 0,
                        level = 0,
                        exp = 0,
                        title = "初入太初",
                        lastlogin = DateTime.UtcNow,
                        logo = "",
                        background = "",
                        likes = 0
                    };

                    _db.UserDatas.Add(userData);

                    Console.WriteLine("确认输入数据到userData");

                    await _db.SaveChangesAsync(); // 保存用户扩展数据
                    _logger.LogInformation($"用户扩展数据已创建 (ID: {user.Id})");

                    // 8. 删除验证码记录
                    _db.EmailVerifications.Remove(verification);
                    await _db.SaveChangesAsync(); // 删除验证码

                    await transaction.CommitAsync();
                    _logger.LogInformation("事务已提交");
                    _logger.LogInformation($"用户注册成功: {dto.username}");

                    return Ok(new
                    {
                        message = "注册成功！",
                        userId = user.Id,
                        username = user.username
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "注册过程中发生错误");
                    return StatusCode(500, new
                    {
                        message = "注册失败，请稍后重试。",
                        error = ex.Message,
                        innerError = ex.InnerException?.Message
                    });
                }
            });
        }

        // 在 AuthController 中添加以下方法

        /// <summary>
        /// 获取注册用户总数
        /// </summary>
        [HttpGet("total-users")]
        public async Task<ActionResult<int>> GetTotalUsers()
        {
            try
            {
                var totalUsers = await _db.UserAccounts.CountAsync();
                return totalUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户总数失败");
                return StatusCode(500, "获取用户总数失败");
            }
        }

        // 在 UserInfoController.cs 中添加
        [HttpGet("level")]
        [Authorize]
        public async Task<IActionResult> GetUserLevel()
        {
            Console.WriteLine("检查等级");
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _db.UserDatas.FindAsync(userId);

            if (user == null)
            {
                return NotFound("用户不存在");
            }

            return Ok(new { level = user.level });
        }












    }
}