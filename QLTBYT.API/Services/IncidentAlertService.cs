using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLTBYT.API.Data;

namespace QLTBYT.API.Services;

public interface IIncidentAlertService
{
    Task SendIncidentCreatedAsync(int deviceId, string deviceCode, string content);
    Task SendIncidentResolvedAsync(int eventId, string deviceCode);
}

public class IncidentAlertService : IIncidentAlertService
{
    private readonly AppDbContext _context;
    private readonly IEmailNotificationService _email;
    private readonly IFirebasePushService _firebase;
    private readonly ILogger<IncidentAlertService> _logger;

    public IncidentAlertService(
        AppDbContext context,
        IEmailNotificationService email,
        IFirebasePushService firebase,
        ILogger<IncidentAlertService> logger)
    {
        _context = context;
        _email = email;
        _firebase = firebase;
        _logger = logger;
    }

    public async Task SendIncidentCreatedAsync(int deviceId, string deviceCode, string content)
    {
        var subject = $"[QLTBYT] Su co thiet bi {deviceCode}";
        var body = $"Thiet bi: {deviceCode} (ID={deviceId})\nNoi dung: {content}\nThoi gian: {DateTime.Now:dd/MM/yyyy HH:mm}";
        await NotifyStaffAsync(subject, body, "Yeu cau thiet bi moi", content);
    }

    public async Task SendIncidentResolvedAsync(int eventId, string deviceCode)
    {
        var subject = $"[QLTBYT] Da xu ly su co {deviceCode}";
        var body = $"Su co ID={eventId}, thiet bi {deviceCode} da duoc xu ly.\nThoi gian: {DateTime.Now:dd/MM/yyyy HH:mm}";
        await NotifyStaffAsync(subject, body, "Su co da xu ly", $"Thiet bi {deviceCode}");
    }

    private async Task NotifyStaffAsync(string emailSubject, string emailBody, string pushTitle, string pushBody)
    {
        var staff = await _context.UserAccounts.AsNoTracking()
            .Where(u => u.Admin == 1 || u.AccountType == 0 || u.AccountType == 1 || u.AccountType == 2)
            .ToListAsync();

        foreach (var user in staff)
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _email.SendAsync(user.Email!, emailSubject, emailBody);
            }

            if (!string.IsNullOrWhiteSpace(user.RegId))
            {
                await _firebase.SendToDeviceAsync(user.RegId!, pushTitle, pushBody);
            }
        }

        _logger.LogDebug("Incident alerts dispatched to {Count} staff accounts", staff.Count);
    }
}
