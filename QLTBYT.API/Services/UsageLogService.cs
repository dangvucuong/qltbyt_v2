using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.UsageLog;

namespace QLTBYT.API.Services;

public interface IUsageLogService
{
    Task<List<UsageLogDto>> GetLogsAsync(UsageLogQueryParams query);
    /// <summary>Ghi mới hoặc cập nhật nếu đã có bản ghi cùng thiết bị + ngày (legacy UpdateEveryday).</summary>
    Task<UsageLogDto> UpsertAsync(CreateUsageLogRequest request);
}

public class UsageLogService : IUsageLogService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISystemAuditService _audit;

    public UsageLogService(AppDbContext context, ICurrentUserService currentUser, ISystemAuditService audit)
    {
        _context = context;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<List<UsageLogDto>> GetLogsAsync(UsageLogQueryParams query)
    {
        var q = _context.UsageLogs.AsNoTracking().AsQueryable();
        HashSet<string>? deviceFilterKeys = null;

        if (!string.IsNullOrWhiteSpace(query.DeviceCode))
        {
            var device = await FindDeviceAsync(query.DeviceCode);
            if (device is null)
                return new List<UsageLogDto>();

            await EnsureDeviceScopeAsync(device);
            // Chuẩn: InputByDate.MaTB = ID thiết bị; vẫn đọc khóa MaTB cũ nếu có.
            deviceFilterKeys = DeviceIdentityHelper.LookupKeys(device.Id, device.DeviceCode);
        }

        if (query.FromDate.HasValue)
            q = q.Where(l => l.InputDate >= query.FromDate.Value);
        if (query.ToDate.HasValue)
            q = q.Where(l => l.InputDate <= query.ToDate.Value);

        // Không dùng Trim() trong LINQ-to-SQL (legacy MaTB có padding) — lọc trong memory.
        var rows = await q.OrderByDescending(l => l.InputDate).Take(2000).ToListAsync();

        if (deviceFilterKeys != null)
        {
            rows = rows
                .Where(l => l.DeviceCode != null && deviceFilterKeys.Contains(l.DeviceCode.Trim()))
                .Take(200)
                .ToList();
        }
        else if (_currentUser.MustFilterByOrganization)
        {
            var keys = await ScopedDeviceKeysAsync();
            rows = rows
                .Where(l => l.DeviceCode != null && keys.Contains(l.DeviceCode.Trim()))
                .Take(200)
                .ToList();
        }
        else
        {
            rows = rows.Take(200).ToList();
        }

        var lookup = await DeviceLookup.BuildAsync(_context);
        return rows.Select(l => Map(l, lookup)).ToList();
    }

    public async Task<UsageLogDto> UpsertAsync(CreateUsageLogRequest request)
    {
        var device = await FindDeviceAsync(request.DeviceCode);
        if (device is null)
            throw new InvalidOperationException("Không tìm thấy thiết bị.");

        await EnsureDeviceScopeAsync(device);

        // Luôn lưu ID thiết bị vào InputByDate.MaTB (thay MaTB string).
        var storageKey = DeviceIdentityHelper.StorageKey(device.Id);
        var lookupKeys = DeviceIdentityHelper.LookupKeys(device.Id, device.DeviceCode);
        var day = request.InputDate.Date;

        var candidates = await _context.UsageLogs
            .Where(l => l.InputDate >= day && l.InputDate < day.AddDays(1))
            .ToListAsync();
        var entity = candidates.FirstOrDefault(l =>
            l.DeviceCode != null && lookupKeys.Contains(l.DeviceCode.Trim()));

        if (entity is null)
        {
            entity = new UsageLog
            {
                DeviceCode = storageKey,
                Hours = request.Hours,
                Sessions = request.Sessions,
                InputDate = day,
                UserId = _currentUser.UserId
            };
            _context.UsageLogs.Add(entity);
            await _context.SaveChangesAsync();
            await _audit.WriteAsync(_currentUser.UserId, $"Nhật ký sử dụng TB#{storageKey}", device.Id);
        }
        else
        {
            entity.Hours = request.Hours;
            entity.Sessions = request.Sessions;
            entity.UserId = _currentUser.UserId;
            entity.InputDate = day;
            // Chuẩn hóa khóa cũ → ID nếu trước đó còn lưu MaTB.
            entity.DeviceCode = storageKey;
            await _context.SaveChangesAsync();
            await _audit.WriteAsync(_currentUser.UserId, $"Cập nhật nhật ký sử dụng TB#{storageKey}", device.Id);
        }

        var lookup = await DeviceLookup.BuildAsync(_context);
        return Map(entity, lookup);
    }

    private async Task<Device?> FindDeviceAsync(string? codeOrId)
    {
        if (string.IsNullOrWhiteSpace(codeOrId))
            return null;

        var key = codeOrId.Trim();
        if (int.TryParse(key, out var id))
        {
            var byId = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (byId != null)
                return byId;
        }

        // Fallback hiếm: vẫn còn MaTB string trong DB.
        return await _context.Devices.AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeviceCode != null && d.DeviceCode.Trim() == key);
    }

    private async Task EnsureDeviceScopeAsync(Device device)
    {
        if (!_currentUser.MustFilterByOrganization)
        {
            return;
        }

        var allowed = _currentUser.OrganizationIds;
        if (device.OrganizationId is null || !allowed.Contains(device.OrganizationId.Value))
            throw new InvalidOperationException("Không có quyền với thiết bị này.");
    }

    private async Task<List<string>> ScopedDeviceKeysAsync()
    {
        var allowed = _currentUser.OrganizationIds;
        if (allowed.Count == 0)
            return new List<string>();

        var devices = await _context.Devices.AsNoTracking()
            .Where(d => d.OrganizationId != null && allowed.Contains(d.OrganizationId.Value))
            .Select(d => new { d.Id, d.DeviceCode })
            .ToListAsync();

        // Khóa theo ID (+ MaTB cũ nếu có) để lọc nhật ký trong phạm vi đơn vị.
        var keys = new List<string>();
        foreach (var d in devices)
        {
            keys.Add(d.Id.ToString());
            if (!string.IsNullOrWhiteSpace(d.DeviceCode))
                keys.Add(d.DeviceCode.Trim());
        }
        return keys;
    }

    private static UsageLogDto Map(UsageLog l, DeviceLookup lookup)
    {
        var storageKey = (l.DeviceCode ?? string.Empty).Trim();
        lookup.TryResolve(storageKey, out var deviceId, out var code, out var name);

        return new UsageLogDto
        {
            Id = l.Id,
            DeviceId = deviceId,
            DeviceCode = !string.IsNullOrWhiteSpace(code) ? code! : (deviceId?.ToString() ?? storageKey),
            DeviceName = name,
            Hours = l.Hours,
            Sessions = l.Sessions,
            InputDate = l.InputDate,
            UserId = l.UserId
        };
    }

    private sealed class DeviceLookup
    {
        private readonly Dictionary<string, (int Id, string? Code, string? Name)> _byKey = new(StringComparer.Ordinal);

        public static async Task<DeviceLookup> BuildAsync(AppDbContext context)
        {
            var lookup = new DeviceLookup();
            var devices = await context.Devices.AsNoTracking()
                .Select(d => new { d.Id, d.DeviceCode, d.Name })
                .ToListAsync();

            foreach (var d in devices)
            {
                var entry = (d.Id, Code: string.IsNullOrWhiteSpace(d.DeviceCode) ? null : d.DeviceCode.Trim(), d.Name);
                lookup._byKey[d.Id.ToString()] = entry;
                if (!string.IsNullOrWhiteSpace(entry.Code))
                {
                    lookup._byKey[entry.Code!] = entry;
                }
            }

            return lookup;
        }

        public bool TryResolve(string storageKey, out int? deviceId, out string? deviceCode, out string? deviceName)
        {
            deviceId = null;
            deviceCode = null;
            deviceName = null;
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                return false;
            }

            if (_byKey.TryGetValue(storageKey, out var hit))
            {
                deviceId = hit.Id;
                deviceCode = hit.Code;
                deviceName = hit.Name;
                return true;
            }

            if (int.TryParse(storageKey, out var parsedId) && _byKey.TryGetValue(parsedId.ToString(), out hit))
            {
                deviceId = hit.Id;
                deviceCode = hit.Code;
                deviceName = hit.Name;
                return true;
            }

            return false;
        }
    }
}
