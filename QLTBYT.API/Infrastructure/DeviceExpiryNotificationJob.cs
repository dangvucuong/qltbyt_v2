using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QLTBYT.API.Data;
using QLTBYT.API.Services;

namespace QLTBYT.API.Infrastructure;

public class DeviceExpiryNotificationJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DeviceExpiryNotificationJob> _logger;

    public DeviceExpiryNotificationJob(IServiceProvider services, ILogger<DeviceExpiryNotificationJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(6);
            if (now > nextRun)
                nextRun = nextRun.AddDays(1);
            var delay = nextRun - now;
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                await RunCheckAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Device expiry notification job failed");
            }
        }
    }

    private async Task RunCheckAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var threshold = DateTime.Now.AddDays(30);
        var expiring = await context.Devices.AsNoTracking()
            .Where(d => d.InspectionExpiryDate != null && d.InspectionExpiryDate <= threshold)
            .Select(d => new { d.Id, d.DeviceCode, d.InspectionExpiryDate })
            .Take(50)
            .ToListAsync(cancellationToken);

        if (expiring.Count == 0)
            return;

        var adminIds = await context.UserAccounts.AsNoTracking()
            .Where(u => u.Admin == 1 || u.AccountType == 0 || u.AccountType == 1 || u.AccountType == 2)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (adminIds.Count == 0)
            return;

        var summary = string.Join(", ", expiring.Select(e =>
            $"#{e.Id}{(string.IsNullOrWhiteSpace(e.DeviceCode) ? "" : $"/{e.DeviceCode.Trim()}")} ({e.InspectionExpiryDate:dd/MM/yyyy})"));
        await notifications.NotifyUsersAsync(
            "Thiết bị sắp hết hạn kiểm định",
            summary,
            adminIds.First(),
            adminIds);
        _logger.LogInformation("Expiry notifications sent for {Count} devices", expiring.Count);
    }
}

