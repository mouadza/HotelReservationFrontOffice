using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    private readonly string _smtpServer = "smtp.gmail.com"; // Replace with your SMTP server
    private readonly int _smtpPort = 587; // Replace with your SMTP port
    private const string _smtpUser = "elidrissiabdallah689@gmail.com"; // Votre email
    private const string _smtpPass = "unqn bqbf zaeh egbz";  // Replace with your app password

    public void SendEmail(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Hotel Reservation", _smtpUser));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            client.Connect(_smtpServer, _smtpPort, false);
            client.Authenticate(_smtpUser, _smtpPass);

            client.Send(message);
            client.Disconnect(true);
        }
    }
}