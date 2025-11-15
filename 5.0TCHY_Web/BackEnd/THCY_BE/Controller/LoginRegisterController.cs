using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using THCY_BE.DataBase;
using THCY_BE.Models.LoginRegister;
using THCY_BE.Models.UserDate;
using THCY_BE.Requests;
using THCY_BE.Services;
using THCY_BE.Utils;

namespace THCY_BE.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginRegisterController : ControllerBase
    {
        private readonly BasicInfoDbContext _context;
        private readonly ILogger<LoginRegisterController> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public LoginRegisterController(
            BasicInfoDbContext context,
            ILogger<LoginRegisterController> logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }

        // POST: api/loginregister/send-verification-code
        [HttpPost("send-verification-code")]
        public async Task<ActionResult> SendVerificationCode([FromBody] SendCodeRequest request)
        {
            try
            {
                // 验证邮箱格式
                if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
                {
                    return BadRequest(new { error = "请输入有效的邮箱地址" });
                }

                // 检查邮箱是否已注册
                var existingUser = await _context.UserAccounts
                    .FirstOrDefaultAsync(u => u.email_address == request.Email);

                if (existingUser != null)
                {
                    return BadRequest(new { error = "该邮箱已被注册" });
                }

                // 生成6位随机验证码
                var random = new Random();
                var code = random.Next(100000, 999999).ToString();

                // 保存验证码到数据库
                var emailVerification = new EmailVerification
                {
                    Email = request.Email,
                    Code = code,
                    createTime = DateTime.Now,
                    ExpireTime = DateTime.Now.AddMinutes(3)
                };

                _context.EmailVerifications.Add(emailVerification);
                await _context.SaveChangesAsync();

                // 发送邮件
                var emailSent = await _emailService.SendVerificationCodeAsync(request.Email, code);

                if (!emailSent)
                {
                    _logger.LogWarning($"邮件发送失败: {request.Email}");
                    return StatusCode(500, new { error = "发送验证码失败，请检查邮箱配置" });
                }

                _logger.LogInformation($"验证码生成并发送成功: {request.Email}");

                return Ok(new
                {
                    message = "验证码已发送到您的邮箱，请查收",
                    code = code // 测试用
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送验证码失败");
                return StatusCode(500, new { error = "发送验证码失败，请稍后重试" });
            }
        }

        // POST: api/loginregister/verify-code
        [HttpPost("verify-code")]
        public async Task<ActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
                {
                    return BadRequest(new { error = "请输入有效的邮箱地址" });
                }

                if (string.IsNullOrEmpty(request.Code) || request.Code.Length != 6)
                {
                    return BadRequest(new { error = "验证码必须是6位数字" });
                }

                var verification = await _context.EmailVerifications
                    .Where(v => v.Email == request.Email && v.Code == request.Code)
                    .OrderByDescending(v => v.createTime)
                    .FirstOrDefaultAsync();

                if (verification == null)
                {
                    return BadRequest(new { error = "验证码无效" });
                }

                if (verification.ExpireTime < DateTime.Now)
                {
                    return BadRequest(new { error = "验证码已过期" });
                }

                _logger.LogInformation($"验证码验证成功: {request.Email}");

                return Ok(new
                {
                    success = true,
                    message = "验证码验证成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证码验证失败");
                return StatusCode(500, new { error = "验证码验证失败，请稍后重试" });
            }
        }

        // POST: api/loginregister/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // 验证输入
                if (string.IsNullOrEmpty(request.Username) || request.Username.Length < 3)
                {
                    return BadRequest(new { error = "用户名至少3个字符" });
                }

                if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
                {
                    return BadRequest(new { error = "请输入有效的邮箱地址" });
                }

                if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
                {
                    return BadRequest(new { error = "密码长度至少6位" });
                }

                if (string.IsNullOrEmpty(request.VerificationCode) || request.VerificationCode.Length != 6)
                {
                    return BadRequest(new { error = "验证码必须是6位数字" });
                }

                // 1. 验证验证码
                var verification = await _context.EmailVerifications
                    .Where(v => v.Email == request.Email && v.Code == request.VerificationCode)
                    .OrderByDescending(v => v.createTime)
                    .FirstOrDefaultAsync();

                if (verification == null)
                {
                    return BadRequest(new { error = "验证码无效" });
                }

                if (verification.ExpireTime < DateTime.Now)
                {
                    return BadRequest(new { error = "验证码已过期" });
                }

                // 2. 检查用户名和邮箱是否已存在
                var existingUser = await _context.UserAccounts
                    .FirstOrDefaultAsync(u => u.username == request.Username || u.email_address == request.Email);

                if (existingUser != null)
                {
                    if (existingUser.username == request.Username)
                    {
                        return BadRequest(new { error = "用户名已被注册" });
                    }
                    else
                    {
                        return BadRequest(new { error = "邮箱已被注册" });
                    }
                }

                // 3. 创建用户账户 - 直接使用byte[]存储到VARBINARY
                var newUser = new UserAccount
                {
                    username = request.Username,
                    email_address = request.Email,
                    password_hash = PasswordHasher.HashPassword(request.Password), // 直接使用byte[]
                    date = DateTime.Now,
                    state = 1,
                    rank = 0,
                    is_verified = true,
                    failed_login_attempts = 0
                };

                _context.UserAccounts.Add(newUser);
                await _context.SaveChangesAsync();

                // 4. 创建用户数据
                var userData = new UserData
                {
                    id = newUser.Id,
                    points = 0,
                    level = 1,
                    exp = 0,
                    title = "新晋成员",
                    lastlogin = DateTime.Now,
                    logo = "",
                    background = "",
                    likes = 0,
                    last_active_time = DateTime.Now,
                    byd = 0,
                    creater = 0
                };

                _context.UserDatas.Add(userData);
                await _context.SaveChangesAsync();

                // 5. 验证成功后删除验证码记录
                _context.EmailVerifications.Remove(verification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"用户注册成功: {request.Username} ({request.Email})");

                return Ok(new
                {
                    success = true,
                    message = "注册成功",
                    userId = newUser.Id,
                    username = newUser.username
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户注册失败");
                return StatusCode(500, new { error = "注册失败，请稍后重试" });
            }
        }

        // POST: api/loginregister/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UsernameOrEmail))
                {
                    return BadRequest(new { error = "请输入用户名或邮箱" });
                }

                if (string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "请输入密码" });
                }

                // 查找用户
                var user = await _context.UserAccounts
                    .FirstOrDefaultAsync(u => u.username == request.UsernameOrEmail ||
                                             u.email_address == request.UsernameOrEmail);

                if (user == null)
                {
                    return BadRequest(new { error = "用户名或密码错误" });
                }

                // 检查账户状态
                if (user.state == 0)
                {
                    return BadRequest(new { error = "账户已被禁用" });
                }

                // 验证密码 - 直接使用byte[]验证
                bool isPasswordValid = PasswordHasher.VerifyPassword(request.Password, user.password_hash);

                if (!isPasswordValid)
                {
                    user.failed_login_attempts++;
                    await _context.SaveChangesAsync();
                    return BadRequest(new { error = "用户名或密码错误" });
                }

                // 登录成功
                user.failed_login_attempts = 0;
                await _context.SaveChangesAsync();

                // 更新用户数据
                var userData = await _context.UserDatas.FindAsync(user.Id);
                if (userData != null)
                {
                    userData.lastlogin = DateTime.Now;
                    userData.last_active_time = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"用户登录成功: {user.username}");

                // 生成JWT Token
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    success = true,
                    message = "登录成功",
                    userId = user.Id,
                    username = user.username,
                    token = token // 返回token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录失败");
                return StatusCode(500, new { error = "登录失败，请稍后重试" });
            }
        }

        // 生成JWT Token的方法
        private string GenerateJwtToken(UserAccount user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                // 从配置中获取JWT设置
                var secretKey = _configuration["Jwt:SecretKey"];
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var expireDays = _configuration.GetValue<int>("Jwt:ExpireDays", 7);

                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new Exception("JWT SecretKey 未配置，请在appsettings.json中配置JWT设置");
                }

                var key = Encoding.UTF8.GetBytes(secretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.username),
                        new Claim(ClaimTypes.Email, user.email_address),
                        new Claim("userId", user.Id.ToString()),
                        new Claim("username", user.username)
                    }),
                    Expires = DateTime.UtcNow.AddDays(expireDays),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成JWT Token失败");
                throw new Exception("生成Token失败，请检查JWT配置");
            }
        }

        // 其他方法保持不变...
        [HttpGet("check-email")]
        public async Task<ActionResult> CheckEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
                {
                    return BadRequest(new { error = "请输入有效的邮箱地址" });
                }

                var existingUser = await _context.UserAccounts
                    .FirstOrDefaultAsync(u => u.email_address == email);

                return Ok(new
                {
                    available = existingUser == null,
                    message = existingUser == null ? "邮箱可用" : "邮箱已被注册"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查邮箱失败");
                return StatusCode(500, new { error = "检查邮箱失败" });
            }
        }

        [HttpGet("check-username")]
        public async Task<ActionResult> CheckUsername(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || username.Length < 3)
                {
                    return BadRequest(new { error = "用户名至少3个字符" });
                }

                var existingUser = await _context.UserAccounts
                    .FirstOrDefaultAsync(u => u.username == username);

                return Ok(new
                {
                    available = existingUser == null,
                    message = existingUser == null ? "用户名可用" : "用户名已被使用"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户名失败");
                return StatusCode(500, new { error = "检查用户名失败" });
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    public class LoginRequest
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}