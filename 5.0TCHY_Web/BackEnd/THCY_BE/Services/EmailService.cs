// Services/EmailService.cs
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;

namespace THCY_BE.Services
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "smtp.qq.com";
        public int SmtpPort { get; set; } = 465; // 保持465端口
        public string SmtpUsername { get; set; } = "2740727506@qq.com";
        public string SmtpPassword { get; set; } = "axqmgepywqnjdhbb";
        public string SiteName { get; set; } = "太初寰宇";
    }

    public interface IEmailService
    {
        Task<bool> SendVerificationCodeAsync(string email, string code);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendVerificationCodeAsync(string email, string code)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SiteName, _emailSettings.SmtpUsername));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = $"{_emailSettings.SiteName} - 邮箱验证码";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>{_emailSettings.SiteName}</h2>
                    <p>您好！</p>
                    <p>您的邮箱验证码为：</p>
                    <div style='background: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>
                        <span style='font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 8px;'>{code}</span>
                    </div>
                    <p>验证码有效期3分钟，请尽快使用。</p>
                    <p>如果这不是您请求的，请忽略此邮件。</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='color: #666; font-size: 12px;'>此邮件由系统自动发送，请勿回复。</p>
                </div>";

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();

                // 连接QQ邮箱SMTP服务器，465端口需要使用SSL
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, true);

                // 认证
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                // 发送邮件
                await client.SendAsync(message);

                // 断开连接
                await client.DisconnectAsync(true);

                _logger.LogInformation($"验证码已发送到邮箱: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送邮件到 {email} 失败");
                return false;
            }
        }
    }
}