using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QLTBYT.API.Configuration;

namespace QLTBYT.API.Services;

public interface IFirebasePushService
{
    Task SendToDeviceAsync(string registrationId, string title, string body);
}

public class FirebasePushNotificationService : IFirebasePushService
{
    private readonly FirebaseSettings _settings;
    private readonly ILogger<FirebasePushNotificationService> _logger;
    private static readonly object InitLock = new();

    public FirebasePushNotificationService(
        IOptions<FirebaseSettings> settings,
        ILogger<FirebasePushNotificationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendToDeviceAsync(string registrationId, string title, string body)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(registrationId))
        {
            return;
        }

        try
        {
            EnsureFirebaseApp();
            if (FirebaseApp.DefaultInstance is null)
            {
                return;
            }

            var message = new Message
            {
                Token = registrationId.Trim(),
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            var id = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Firebase message sent: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase push failed for token {Token}", registrationId);
        }
    }

    private void EnsureFirebaseApp()
    {
        if (FirebaseApp.DefaultInstance is not null)
        {
            return;
        }

        lock (InitLock)
        {
            if (FirebaseApp.DefaultInstance is not null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_settings.CredentialPath) ||
                !File.Exists(_settings.CredentialPath))
            {
                _logger.LogWarning("Firebase enabled but CredentialPath missing or file not found.");
                return;
            }

            var options = new AppOptions
            {
                Credential = GoogleCredential.FromFile(_settings.CredentialPath),
                ProjectId = _settings.ProjectId
            };
            FirebaseApp.Create(options);
        }
    }
}
