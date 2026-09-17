using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using QLTBYT.API.Configuration;

namespace QLTBYT.API.Services;

public interface IEmailNotificationService
{
    Task SendAsync(string toEmail, string subject, string body);
}

public class MailKitEmailNotificationService : IEmailNotificationService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<MailKitEmailNotificationService> _logger;

    public MailKitEmailNotificationService(
        IOptions<SmtpSettings> settings,
        ILogger<MailKitEmailNotificationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("SMTP disabled — skip email to {Email}", toEmail);
            return;
        }

        if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(_settings.Host))
        {
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _settings.FromDisplayName ?? "QLTBYT",
            _settings.From ?? _settings.User ?? "noreply@qltbyt.local"));
        message.To.Add(MailboxAddress.Parse(toEmail.Trim()));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            if (!string.IsNullOrWhiteSpace(_settings.User))
            {
                await client.AuthenticateAsync(_settings.User, _settings.Password);
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}
