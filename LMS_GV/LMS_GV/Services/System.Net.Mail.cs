using MailKit.Net.Smtp;
using MimeKit;


public class EmailService
{
  
public void SendOtp(string toEmail, string otp)
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress("LMS System", "your-email@gmail.com"));
    message.To.Add(MailboxAddress.Parse(toEmail));
    message.Subject = "Mã OTP đăng nhập LMS";
    message.Body = new TextPart("plain") { Text = $"Mã OTP: {otp}" };

    using var client = new SmtpClient();
    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
    client.Authenticate("your-email@gmail.com", "your-app-password");
    client.Send(message);
    client.Disconnect(true);
}

}
