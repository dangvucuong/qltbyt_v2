using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Event;

namespace QLTBYT.API.Services;

public partial class DeviceService
{
    public async Task<DeviceDetailDto?> TransferDeviceAsync(int id, TransferDeviceRequest request)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id);
        if (device is null || !IsDeviceAllowed(device))
            return null;

        var fromOrg = device.OrganizationId;
        device.OrganizationId = request.ToOrganizationId;
        if (request.HandoverOrganizationId.HasValue)
            device.HandoverOrgId = request.HandoverOrganizationId;

        device.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var note = request.Note?.Trim() ?? "Chuyển giao thiết bị";
        await _deviceEventService.CreateForDeviceAsync(id, new CreateDeviceEventRequest
        {
            EventTypeId = 4,
            Content = $"{note} (tu DV {fromOrg} -> {request.ToOrganizationId})",
            Keyword = "Chuyển giao"
        });

        await _audit.WriteAsync(_currentUser.UserId,
            $"Chuyển giao TB {DeviceIdentityHelper.DisplayCode(id, device.DeviceCode)}", id);
        return await GetDeviceByIdAsync(id);
    }

    public async Task<List<DeviceHistoryItemDto>> GetDeviceHistoryAsync(int id)
    {
        var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (device is null || !IsDeviceAllowed(device))
            return new List<DeviceHistoryItemDto>();

        var items = new List<DeviceHistoryItemDto>();
        var usageKeys = DeviceIdentityHelper.LookupKeys(id, device.DeviceCode);

        var events = await _context.DeviceEvents.AsNoTracking()
            .Where(e => e.DeviceId == id)
            .OrderByDescending(e => e.EventDateTime)
            .Take(50)
            .ToListAsync();
        items.AddRange(events.Select(e => new DeviceHistoryItemDto
        {
            Type = "Sự cố / yêu cầu",
            Content = e.Content,
            Timestamp = e.EventDateTime,
            Actor = e.AccountUsername
        }));

        var usageRows = await _context.UsageLogs.AsNoTracking()
            .OrderByDescending(u => u.InputDate)
            .Take(2000)
            .ToListAsync();
        items.AddRange(usageRows
            .Where(u => u.DeviceCode != null && usageKeys.Contains(u.DeviceCode.Trim()))
            .Take(30)
            .Select(u => new DeviceHistoryItemDto
            {
                Type = "Nhật ký sử dụng",
                Content = $"Gio: {u.Hours}, Ca: {u.Sessions}",
                Timestamp = u.InputDate
            }));

        var audits = await _context.SystemAuditLogs.AsNoTracking()
            .Where(a => a.DeviceId == id)
            .OrderByDescending(a => a.Timestamp)
            .Take(30)
            .ToListAsync();
        items.AddRange(audits.Select(a => new DeviceHistoryItemDto
        {
            Type = "Audit",
            Content = a.Content,
            Timestamp = a.Timestamp
        }));

        return items.OrderByDescending(i => i.Timestamp).Take(80).ToList();
    }

    public async Task<DeviceDetailDto?> UpdateQrAsync(int id, UpdateDeviceQrRequest request)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id);
        if (device is null || !IsDeviceAllowed(device))
            return null;

        var code = await _context.DeviceCodes.FirstOrDefaultAsync(c => c.DeviceId == id);
        if (code is null)
        {
            code = new DeviceCode { DeviceId = id };
            _context.DeviceCodes.Add(code);
        }
        if (request.QrCode is not null) code.QrCode = request.QrCode.Trim();
        if (request.Nfc is not null) code.Nfc = request.Nfc.Trim();
        await _context.SaveChangesAsync();
        return await GetDeviceByIdAsync(id);
    }

    public async Task<List<DeviceSupplyDto>> GetSuppliesAsync(int deviceId)
    {
        if (!await _context.Devices.AnyAsync(d => d.Id == deviceId))
            return new List<DeviceSupplyDto>();
        return await _context.DeviceSupplies.AsNoTracking()
            .Where(s => s.DeviceId == deviceId)
            .Select(s => new DeviceSupplyDto
            {
                Id = s.Id,
                Name = s.Name,
                Unit = s.Unit,
                Quantity = s.Quantity
            })
            .ToListAsync();
    }

    public async Task<DeviceSupplyDto> AddSupplyAsync(int deviceId, CreateDeviceSupplyRequest request)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
        if (device is null || !IsDeviceAllowed(device))
            throw new InvalidOperationException("Không tìm thấy thiết bị.");

        var entity = new DeviceSupply
        {
            DeviceId = deviceId,
            Name = request.Name.Trim(),
            Unit = request.Unit?.Trim(),
            Quantity = request.Quantity
        };
        _context.DeviceSupplies.Add(entity);
        await _context.SaveChangesAsync();
        return new DeviceSupplyDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Unit = entity.Unit,
            Quantity = entity.Quantity
        };
    }

    public async Task<bool> DeleteSupplyAsync(int deviceId, int supplyId)
    {
        var entity = await _context.DeviceSupplies.FirstOrDefaultAsync(s => s.Id == supplyId && s.DeviceId == deviceId);
        if (entity is null) return false;
        _context.DeviceSupplies.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> ImportDevicesFromExcelAsync(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();
        var rows = sheet.RowsUsed().Skip(1);
        int count = 0;

        var defaultType = await _context.DeviceTypes.OrderBy(t => t.Id).Select(t => t.Id).FirstOrDefaultAsync();
        var defaultStatus = await _context.DeviceStatuses.OrderBy(s => s.Id).Select(s => s.Id).FirstOrDefaultAsync();
        var defaultOrg = await _context.Organizations.OrderBy(o => o.Id).Select(o => o.Id).FirstOrDefaultAsync();

        foreach (var row in rows)
        {
            var codeRaw = row.Cell(1).GetString().Trim();
            var name = row.Cell(2).GetString().Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            string? code = string.IsNullOrEmpty(codeRaw) ? null : codeRaw;
            if (code != null && await _context.Devices.AnyAsync(d => d.DeviceCode == code))
                continue;

            _context.Devices.Add(new Device
            {
                DeviceCode = code,
                Name = name,
                TypeId = defaultType == 0 ? null : defaultType,
                StatusId = defaultStatus == 0 ? null : defaultStatus,
                OrganizationId = defaultOrg == 0 ? null : defaultOrg,
                UpdatedAt = DateTime.UtcNow
            });
            count++;
        }

        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Import Excel {count} thiết bị");
        return count;
    }

    private bool IsDeviceAllowed(Device device)
    {
        if (!_currentUser.MustFilterByOrganization) return true;
        return device.OrganizationId != null &&
               _currentUser.OrganizationIds.Contains(device.OrganizationId.Value);
    }
}
