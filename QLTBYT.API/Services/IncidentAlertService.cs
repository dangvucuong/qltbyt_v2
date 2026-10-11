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
        var subject = $"[QLTBYT] Sự cố thiết bị {deviceCode}";
        var body = $"Thiết bị: {deviceCode} (ID={deviceId})\nNội dung: {content}\nThời gian: {DateTime.Now:dd/MM/yyyy HH:mm}";
        await NotifyStaffAsync(subject, body, "Yêu cầu thiết bị mới", content);
    }

    public async Task SendIncidentResolvedAsync(int eventId, string deviceCode)
    {
        var subject = $"[QLTBYT] Đã xử lý sự cố {deviceCode}";
        var body = $"Sự cố ID={eventId}, thiết bị {deviceCode} đã được xử lý.\nThời gian: {DateTime.Now:dd/MM/yyyy HH:mm}";
        await NotifyStaffAsync(subject, body, "Sự cố đã xử lý", $"Thiết bị {deviceCode}");
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
