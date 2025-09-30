using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

public class EmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;
    private readonly IHostEnvironment _env;

    public EmailSender(
        IConfiguration configuration,
        ILogger<EmailSender> logger,
        IHostEnvironment env)
    {
        _configuration = configuration;
        _logger = logger;
        _env = env;
    }

    public async Task SendAsync(string to, string subject, string body, bool isHtml = false)
    {
        // 无论环境如何，都记录日志
        _logger.LogInformation($"准备发送邮件到: {to}");
        _logger.LogInformation($"主题: {subject}");

        // 修改：移除开发环境检查，始终尝试发送邮件
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("太初寰宇社区", _configuration["EmailSettings:SmtpUsername"]));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;

            // 支持HTML或纯文本
            email.Body = isHtml
                ? new TextPart("html") { Text = body }
                : new TextPart("plain") { Text = body };

            using var smtp = new SmtpClient();

            // 添加详细日志
            _logger.LogInformation($"连接到SMTP服务器: {_configuration["EmailSettings:SmtpHost"]}:{_configuration["EmailSettings:SmtpPort"]}");
            _logger.LogInformation($"使用账户: {_configuration["EmailSettings:SmtpUsername"]}");

            await smtp.ConnectAsync(
                _configuration["EmailSettings:SmtpHost"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]),
                SecureSocketOptions.SslOnConnect);

            _logger.LogInformation("SMTP服务器连接成功，尝试认证...");

            await smtp.AuthenticateAsync(
                _configuration["EmailSettings:SmtpUsername"],
                _configuration["EmailSettings:SmtpPassword"]);

            _logger.LogInformation("SMTP认证成功，开始发送邮件...");

            await smtp.SendAsync(email);
            _logger.LogInformation("邮件已发送");

            await smtp.DisconnectAsync(true);
            _logger.LogInformation("SMTP连接已关闭");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"发送邮件到 {to} 失败: {ex.Message}");

            // 详细记录错误信息
            if (ex.InnerException != null)
            {
                _logger.LogError($"内部错误: {ex.InnerException.Message}");
            }

            throw;
        }
    }

    // 添加专门用于发送验证码的方法
    public async Task<bool> SendVerificationCodeAsync(string email, string username, string verificationCode)
    {
        try
        {
            string subject = "太初寰宇社区 - 注册验证码";
            string body = GenerateVerificationEmailBody(username, verificationCode);

            // 保持日志记录，但不改变发送行为
            _logger.LogInformation($"准备发送验证码到邮箱: {email}, 验证码: {verificationCode}");

            await SendAsync(email, subject, body, true);
            _logger.LogInformation($"验证码已成功发送到: {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"发送验证码邮件到 {email} 时失败: {ex.Message}");
            return false;
        }
    }

    // 生成验证码邮件HTML内容
    private string GenerateVerificationEmailBody(string username, string verificationCode)
    {
        return $@"
        <html>
        <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eaeaea; border-radius: 5px;'>
                <h2 style='color: #333;'>太初寰宇社区 - 注册验证</h2>
                <p>您好，<b>{username}</b>！</p>
                <p>感谢您注册太初寰宇社区。请使用以下验证码完成注册：</p>
                <div style='background-color: #f9f9f9; padding: 10px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                    {verificationCode}
                </div>
                <p>当你注册完毕后即可在官网畅游！欢迎前往意见箱或者八卦乾听司给我们提供反馈！</p>
                <p>验证码有效期为5分钟，请尽快完成注册。</p>
                <p>如果您没有请求此验证码，请忽略此邮件。</p>
                <p>此邮件由系统自动发送，请勿回复。感谢配合！</p>
                <hr style='border: none; border-top: 1px solid #eaeaea; margin: 20px 0;'>
                <p style='font-size: 12px; color: #999;'>© 太初寰宇社区</p>
            </div>
        </body>
        </html>
    ";
    }
}