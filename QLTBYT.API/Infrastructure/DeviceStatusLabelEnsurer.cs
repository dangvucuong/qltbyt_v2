using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;

namespace QLTBYT.API.Infrastructure;

public static class DeviceStatusLabelEnsurer
{
    private static readonly Dictionary<int, string> CanonicalById = new()
    {
        { 1, "\u0110ang ho\u1ea1t \u0111\u1ed9ng" },
        { 2, "Kh\u00f4ng ho\u1ea1t \u0111\u1ed9ng / H\u1ecfng" },
        { 3, "Ch\u01b0a s\u1eed d\u1ee5ng" },
    };

    private static readonly Dictionary<string, string> LegacyToCanonical = new()
    {
        { "Dang hoat dong", "\u0110ang ho\u1ea1t \u0111\u1ed9ng" },
        { "Khong hoat dong / Hong", "Kh\u00f4ng ho\u1ea1t \u0111\u1ed9ng / H\u1ecfng" },
        { "Chua su dung", "Ch\u01b0a s\u1eed d\u1ee5ng" },
    };

    public static async Task EnsureAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var statuses = await context.DeviceStatuses.ToListAsync(cancellationToken);
        foreach (var status in statuses)
        {
            if (CanonicalById.TryGetValue(status.Id, out var byId))
            {
                status.Name = byId;
                continue;
            }

            if (status.Name != null && LegacyToCanonical.TryGetValue(status.Name, out var byLegacy))
            {
                status.Name = byLegacy;
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
