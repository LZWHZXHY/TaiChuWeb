// Services/VerificationCodeService.cs
using Microsoft.EntityFrameworkCore;
using System;
using THCY_BE.DataBase;
using THCY_BE.Models;
using THCY_BE.Models.LoginRegister;

namespace THCY_BE.Services
{
    public interface IVerificationCodeService
    {
        Task<string> GenerateVerificationCodeAsync(string email);
        Task<bool> VerifyCodeAsync(string email, string code);
        Task CleanExpiredCodesAsync(); // 新增方法
    }

    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly BasicInfoDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<VerificationCodeService> _logger;

        public VerificationCodeService(BasicInfoDbContext context, IEmailService emailService, ILogger<VerificationCodeService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> GenerateVerificationCodeAsync(string email)
        {
            // 检查1分钟内是否已发送过验证码
            var oneMinuteAgo = DateTime.Now.AddMinutes(-1);
            var recentCode = await _context.EmailVerifications
                .Where(v => v.Email == email && v.createTime > oneMinuteAgo)
                .FirstOrDefaultAsync();

            if (recentCode != null)
            {
                throw new Exception("验证码发送过于频繁，请1分钟后再试");
            }

            // 清理过期的验证码
            await CleanExpiredCodesAsync();

            // 生成6位随机数字
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // 保存到数据库
            var verification = new EmailVerification
            {
                Email = email,
                Code = code,
                createTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(3) // 3分钟有效期
            };

            _context.EmailVerifications.Add(verification);
            await _context.SaveChangesAsync();

            // 发送邮件
            var emailSent = await _emailService.SendVerificationCodeAsync(email, code);

            if (!emailSent)
            {
                // 如果发送失败，删除刚保存的验证码记录
                _context.EmailVerifications.Remove(verification);
                await _context.SaveChangesAsync();
                throw new Exception("发送验证码失败");
            }

            _logger.LogInformation($"为邮箱 {email} 生成验证码: {code}");
            return code;
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            // 先清理过期验证码
            await CleanExpiredCodesAsync();

            var verification = await _context.EmailVerifications
                .Where(v => v.Email == email && v.Code == code)
                .OrderByDescending(v => v.ExpireTime)
                .FirstOrDefaultAsync();

            if (verification == null)
            {
                _logger.LogWarning($"验证码验证失败: 邮箱 {email} 验证码 {code} 不存在或已使用");
                return false;
            }

            if (verification.ExpireTime < DateTime.Now)
            {
                _logger.LogWarning($"验证码验证失败: 邮箱 {email} 验证码 {code} 已过期");
                return false;
            }

            // 验证成功后删除验证码，防止重复使用
            _context.EmailVerifications.Remove(verification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"验证码验证成功: 邮箱 {email}");
            return true;
        }

        public async Task CleanExpiredCodesAsync()
        {
            var expiredCodes = _context.EmailVerifications
                .Where(v => v.ExpireTime < DateTime.Now);

            _context.EmailVerifications.RemoveRange(expiredCodes);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"清理了 {expiredCodes.Count()} 个过期验证码");
        }
    }
}