using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string otp);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Email nhận không được để trống", nameof(toEmail));

        var fromEmail = _config["EmailSettings:FromEmail"];
        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("Cấu hình FromEmail chưa thiết lập trong appsettings.json");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Mã OTP đăng nhập";
        message.Body = new TextPart("html")
        {
            Text = $"<p>Mã OTP của bạn là: <b>{otp}</b></p><p>Hết hạn sau 5 phút.</p>"
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]), SecureSocketOptions.StartTls);
        // Thử thay bằng:
        // SecureSocketOptions.Auto
        await smtp.AuthenticateAsync(fromEmail, _config["EmailSettings:Password"]);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

}
