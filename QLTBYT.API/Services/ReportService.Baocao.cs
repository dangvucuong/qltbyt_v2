using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Report;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Services;

public partial class ReportService
{
    private const string DefaultDeviceGroup = "\u0110i\u1ec1u tr\u1ecb";
    private const string Cs1Cs2LocationToken = "CS1+CS2";

    public async Task<PagedResult<BaocaoAssetRowDto>> GetBaocaoAssetStatisticsAsync(BaocaoAssetQueryParams query)
    {
        var rows = await BuildBaocaoAssetRowsAsync(query);
        return Paginate(rows, query.Page, query.PageSize);
    }

    public async Task<PagedResult<BaocaoOperationalRowDto>> GetBaocaoOperationalStatisticsAsync(BaocaoOperationalQueryParams query)
    {
        var rows = await BuildBaocaoOperationalRowsAsync(query);
        return Paginate(rows, query.Page, query.PageSize);
    }

    public async Task<List<BaocaoAmountOptionDto>> GetBaocaoUsageAmountOptionsAsync(DateTime from, DateTime to)
    {
        var start = from.Date;
        var end = to.Date;
        var deviceCodes = await _context.UsageLogs.AsNoTracking()
            .Where(u => u.InputDate.Date >= start && u.InputDate.Date <= end)
            .Select(u => u.DeviceCode)
            .Distinct()
            .ToListAsync();

        var devices = await ScopedDevices()
            .Where(d => deviceCodes.Contains(d.Id.ToString()))
            .Select(d => d.TotalAmount)
            .ToListAsync();

        return devices
            .Select(a => FinancialParseHelper.ParseLegacyAmount(a))
            .Where(v => v > 0)
            .Distinct()
            .OrderBy(v => v)
            .Select(v => new BaocaoAmountOptionDto
            {
                Amount = v,
                Display = Math.Floor(v).ToString("N0")
            })
            .ToList();
    }

    public async Task<byte[]> ExportBaocaoAssetStatisticsExcelAsync(BaocaoAssetQueryParams query)
    {
        var rows = await BuildBaocaoAssetRowsAsync(query);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Bao cao tai san");
        sheet.Cell(1, 1).Value = "Ten TB";
        sheet.Cell(1, 2).Value = "Hang SX";
        sheet.Cell(1, 3).Value = "Ky hieu";
        sheet.Cell(1, 4).Value = "Serial";
        sheet.Cell(1, 5).Value = "Don vi";
        sheet.Cell(1, 6).Value = "Nhom loc";
        sheet.Cell(1, 7).Value = "Thanh tien";
        sheet.Cell(1, 8).Value = "Ngay tiep nhan";
        sheet.Cell(1, 9).Value = "Khau hao";
        sheet.Cell(1, 10).Value = "So luong";
        sheet.Cell(1, 11).Value = "Don vi tinh";
        sheet.Cell(1, 12).Value = "Ma dinh danh";
        var r = 2;
        foreach (var row in rows)
        {
            sheet.Cell(r, 1).Value = row.Name;
            sheet.Cell(r, 2).Value = row.Manufacturer;
            sheet.Cell(r, 3).Value = row.ModelCode;
            sheet.Cell(r, 4).Value = row.Serial;
            sheet.Cell(r, 5).Value = row.OrganizationName;
            sheet.Cell(r, 6).Value = row.FilterLabel;
            sheet.Cell(r, 7).Value = row.TotalAmount;
            sheet.Cell(r, 8).Value = row.ReceivedDate?.ToString("dd/MM/yyyy");
            sheet.Cell(r, 9).Value = row.DepreciationAmount;
            sheet.Cell(r, 10).Value = row.Quantity;
            sheet.Cell(r, 11).Value = row.Unit;
            sheet.Cell(r, 12).Value = row.IdentityCode;
            r++;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportBaocaoOperationalStatisticsExcelAsync(BaocaoOperationalQueryParams query)
    {
        var rows = await BuildBaocaoOperationalRowsAsync(query);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Bao cao chuyen mon");
        sheet.Cell(1, 1).Value = "Ten TB";
        sheet.Cell(1, 2).Value = "Hang SX";
        sheet.Cell(1, 3).Value = "Ky hieu";
        sheet.Cell(1, 4).Value = "Serial";
        sheet.Cell(1, 5).Value = "Don vi";
        sheet.Cell(1, 6).Value = "Nhom loc";
        sheet.Cell(1, 7).Value = "Han kiem dinh";
        sheet.Cell(1, 8).Value = "Bat dau hong";
        sheet.Cell(1, 9).Value = "Sua xong";
        sheet.Cell(1, 10).Value = "Ghi chu";
        sheet.Cell(1, 11).Value = "Thanh tien";
        sheet.Cell(1, 12).Value = "So gio";
        sheet.Cell(1, 13).Value = "So ca";
        sheet.Cell(1, 14).Value = "Ma dinh danh";
        var r = 2;
        foreach (var row in rows)
        {
            sheet.Cell(r, 1).Value = row.Name;
            sheet.Cell(r, 2).Value = row.Manufacturer;
            sheet.Cell(r, 3).Value = row.ModelCode;
            sheet.Cell(r, 4).Value = row.Serial;
            sheet.Cell(r, 5).Value = row.OrganizationName;
            sheet.Cell(r, 6).Value = row.FilterLabel;
            sheet.Cell(r, 7).Value = row.InspectionExpiryDate?.ToString("dd/MM/yyyy");
            sheet.Cell(r, 8).Value = row.BreakdownStartedAt?.ToString("dd/MM/yyyy HH:mm");
            sheet.Cell(r, 9).Value = row.RepairedAt;
            sheet.Cell(r, 10).Value = row.DowntimeNote;
            sheet.Cell(r, 11).Value = row.TotalAmount;
            sheet.Cell(r, 12).Value = row.UsageHours;
            sheet.Cell(r, 13).Value = row.UsageSessions;
            sheet.Cell(r, 14).Value = row.IdentityCode;
            r++;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private async Task<List<BaocaoAssetRowDto>> BuildBaocaoAssetRowsAsync(BaocaoAssetQueryParams query)
    {
        var section = NormalizeSection(query.Section, "procurement");
        var value = query.Value?.Trim();
        IQueryable<Device> queryable = ScopedDevices()
            .Include(d => d.Organization)
            .Where(d => d.TotalAmount != null && d.TotalAmount.Trim() != string.Empty && d.TotalAmount.Trim() != "0");

        queryable = section switch
        {
            "procurement" when !string.IsNullOrEmpty(value) => queryable.Where(d => d.ProcurementSource == value),
            "classification" when !string.IsNullOrEmpty(value) => queryable.Where(d => d.Classification == value),
            "location" => ApplyLocationFilter(queryable, value),
            _ => queryable
        };

        var devices = await queryable.OrderBy(d => d.Name).ToListAsync();
        var label = ResolveAssetFilterLabel(section, value);
        return devices
            .Where(BaocaoHelper.HasAssetValue)
            .Select(d => MapAssetRow(d, label))
            .ToList();
    }

    private async Task<List<BaocaoOperationalRowDto>> BuildBaocaoOperationalRowsAsync(BaocaoOperationalQueryParams query)
    {
        var section = NormalizeSection(query.Section, "status");
        return section switch
        {
            "status" => await LoadOperationalStatusRowsAsync(query.Value),
            "devicegroup" => await LoadOperationalDeviceGroupRowsAsync(query.Value),
            "maintenance" => await LoadOperationalMaintenanceRowsAsync(query.Value),
            "inspection" => await LoadOperationalInspectionRowsAsync(query.DaysUntilInspection),
            "downtime" => await LoadOperationalDowntimeRowsAsync(),
            "usage" => await LoadOperationalUsageRowsAsync(query),
            _ => new List<BaocaoOperationalRowDto>()
        };
    }

    private async Task<List<BaocaoOperationalRowDto>> LoadOperationalStatusRowsAsync(string? value)
    {
        if (!int.TryParse(value, out var statusId))
            return new List<BaocaoOperationalRowDto>();

        var devices = await ScopedDevices()
            .Include(d => d.Organization)
            .Include(d => d.DeviceStatus)
            .Where(d => d.StatusId == statusId)
            .OrderBy(d => d.Name)
            .ToListAsync();

        var label = devices.FirstOrDefault()?.DeviceStatus?.Name ?? statusId.ToString();
        return devices.Select(d => MapOperationalDeviceRow(d, label)).ToList();
    }

    private async Task<List<BaocaoOperationalRowDto>> LoadOperationalDeviceGroupRowsAsync(string? value)
    {
        var group = string.IsNullOrWhiteSpace(value) ? DefaultDeviceGroup : value.Trim();
        var devices = await ScopedDevices()
            .Include(d => d.Organization)
            .Where(d => d.DeviceGroup == group)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return devices.Select(d => MapOperationalDeviceRow(d, group)).ToList();
    }

    private async Task<List<BaocaoOperationalRowDto>> LoadOperationalMaintenanceRowsAsync(string? value)
    {
        if (!int.TryParse(value, out var maintenanceOrgId))
            return new List<BaocaoOperationalRowDto>();

        var devices = await ScopedDevices()
            .Include(d => d.Organization)
            .Where(d => d.MaintenanceOrgId == maintenanceOrgId)
            .OrderBy(d => d.Name)
            .ToListAsync();

        var label = BaocaoHelper.ResolveMaintenanceLabel(maintenanceOrgId);
        return devices.Select(d => MapOperationalDeviceRow(d, label)).ToList();
    }

    private async Task<List<BaocaoOperationalRowDto>> LoadOperationalInspectionRowsAsync(int daysUntilInspection)
    {
        var days = daysUntilInspection <= 0 ? 30 : daysUntilInspection;
        var today = DateTime.Today;
        var notifyBefore = today.AddDays(days);

        var devices = await ScopedDevices()
            .Include(d => d.Organization)
            .Where(d => d.InspectionExpiryDate != null
                        && d.InspectionExpiryDate > today
                        && d.InspectionExpiryDate < notifyBefore)
            .OrderBy(d => d.InspectionExpiryDate)
            .ToListAsync();

        var rows = devices
            .Select(d => MapOperationalDeviceRow(d, d.InspectionExpiryDate?.ToString("dd/MM/yyyy")))
            .ToList();

        var scopedIds = await ScopedDevices().Select(d => d.Id).ToListAsync();
        var files = await _context.DeviceFiles.AsNoTracking()
            .Where(f => f.DeviceId != null
                        && scopedIds.Contains(f.DeviceId.Value)
                        && f.FileDate != null
                        && f.FileDate > today
                        && f.FileDate < notifyBefore)
            .OrderBy(f => f.FileDate)
            .ToListAsync();

        foreach (var file in files)
        {
            rows.Add(new BaocaoOperationalRowDto
            {
                Id = file.DeviceId,
                Name = file.Description,
                ModelCode = "...",
                Serial = "...",
                OrganizationName = "...",
                FilterLabel = file.FileDate?.ToString("dd/MM/yyyy"),
                InspectionExpiryDate = file.FileDate
            });
        }

        return rows;
    }

    private async Task<List<BaocaoOperationalRowDto>> LoadOperationalDowntimeRowsAsync()
    {
        var deviceIds = await ScopedDevices().Select(d => d.Id).ToListAsync();
        if (deviceIds.Count == 0)
            return new List<BaocaoOperationalRowDto>();

        var events = await _context.DeviceEvents.AsNoTracking()
            .Where(e => e.DeviceId != null && deviceIds.Contains(e.DeviceId.Value))
            .OrderBy(e => e.Id)
            .ToListAsync();

        var devices = await ScopedDevices()
            .Include(d => d.Organization)
            .Where(d => deviceIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id);

        var rows = new List<BaocaoOperationalRowDto>();
        for (var i = 0; i < events.Count; i++)
        {
            var breakdown = events[i];
            if (breakdown.TypeId != 2 || breakdown.DeviceId is null || breakdown.EventDateTime is null)
                continue;

            var deviceId = breakdown.DeviceId.Value;
            devices.TryGetValue(deviceId, out var device);
            var repair = events.FirstOrDefault(e =>
                e.Id > breakdown.Id && e.DeviceId == deviceId && e.TypeId == 1);

            string repairedAt;
            string note;
            if (repair?.EventDateTime is DateTime fixedAt)
            {
                repairedAt = fixedAt.ToString("dd/MM/yyyy HH:mm");
                var span = fixedAt - breakdown.EventDateTime.Value;
                note = $"{Math.Round(span.TotalDays, 2)} ng\u00e0y";
            }
            else
            {
                repairedAt = "...";
                note = "V\u1eabn ch\u01b0a ho\u1ea1t \u0111\u1ed9ng";
            }

            rows.Add(new BaocaoOperationalRowDto
            {
                Id = deviceId,
                Name = device?.Name ?? deviceId.ToString(),
                IdentityCode = device?.IdentityCode,
                Manufacturer = device?.Manufacturer,
                ModelCode = device?.ModelCode,
                Serial = device?.Serial,
                Quantity = device?.Quantity,
                Unit = device?.Unit,
                OrganizationName = device?.Organization?.Name,
                FilterLabel = breakdown.AccountUsername,
                BreakdownStartedAt = breakdown.EventDateTime,
                RepairedAt = repairedAt,
                DowntimeNote = note
            });
        }

        return rows;
    }

    private async Task<List<BaocaoOperationalRowDto>> LoadOperationalUsageRowsAsync(BaocaoOperationalQueryParams query)
    {
        var start = query.From?.Date ?? DateTime.Today.AddDays(-30);
        var end = query.To?.Date ?? DateTime.Today;
        var minAmount = query.MinAmount ?? 0;
        var maxAmount = query.MaxAmount ?? double.MaxValue;

        var logs = await _context.UsageLogs.AsNoTracking()
            .Where(u => u.InputDate.Date >= start && u.InputDate.Date <= end)
            .ToListAsync();

        if (logs.Count == 0)
            return new List<BaocaoOperationalRowDto>();

        var deviceCodes = logs.Select(l => l.DeviceCode).Distinct().ToList();
        var devices = await ScopedDevices()
            .Include(d => d.Organization)
            .Where(d => deviceCodes.Contains(d.Id.ToString()))
            .ToListAsync();

        var label = $"{start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
        var rows = new List<BaocaoOperationalRowDto>();

        foreach (var device in devices)
        {
            var amount = FinancialParseHelper.ParseLegacyAmount(device.TotalAmount);
            if (amount < minAmount || amount > maxAmount)
                continue;

            var deviceLogs = logs.Where(l => l.DeviceCode == device.Id.ToString()).ToList();
            if (deviceLogs.Count == 0)
                continue;

            var hours = deviceLogs.Sum(l => l.Hours ?? 0);
            var sessions = deviceLogs.Sum(l => l.Sessions ?? 0);
            rows.Add(new BaocaoOperationalRowDto
            {
                Id = device.Id,
                Name = device.Name,
                IdentityCode = device.IdentityCode,
                Manufacturer = device.Manufacturer,
                ModelCode = device.ModelCode,
                Serial = device.Serial,
                Quantity = device.Quantity,
                Unit = device.Unit,
                OrganizationName = device.Organization?.Name,
                FilterLabel = label,
                TotalAmount = amount,
                UsageHours = hours,
                UsageSessions = sessions
            });
        }

        return rows.OrderBy(r => r.Name).ToList();
    }

    private static IQueryable<Device> ApplyLocationFilter(IQueryable<Device> queryable, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return queryable;

        if (IsCs1Cs2Location(value))
        {
            return queryable.Where(d => d.LocationCode == "CS1" || d.LocationCode == "CS2");
        }

        return queryable.Where(d => d.LocationCode == value);
    }

    private static bool IsCs1Cs2Location(string value)
    {
        var trimmed = value.Trim();
        if (string.Equals(trimmed, Cs1Cs2LocationToken, StringComparison.OrdinalIgnoreCase))
            return true;
        return trimmed.Contains("CS1", StringComparison.Ordinal) && trimmed.Contains("CS2", StringComparison.Ordinal);
    }

    private static string ResolveAssetFilterLabel(string section, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return section switch
        {
            "procurement" => BaocaoHelper.ResolveProcurementLabel(value),
            "classification" => value,
            "location" => IsCs1Cs2Location(value) ? Cs1Cs2LocationToken : value,
            _ => value
        };
    }

    private static BaocaoAssetRowDto MapAssetRow(Device device, string filterLabel)
    {
        var amount = FinancialParseHelper.ParseLegacyAmount(device.TotalAmount);
        return new BaocaoAssetRowDto
        {
            Id = device.Id,
            Name = device.Name,
            IdentityCode = device.IdentityCode,
            Manufacturer = device.Manufacturer,
            ModelCode = device.ModelCode,
            Serial = device.Serial,
            Quantity = device.Quantity,
            Unit = device.Unit,
            OrganizationName = device.Organization?.Name,
            FilterLabel = filterLabel,
            TotalAmount = amount,
            ReceivedDate = device.ReceivedDate,
            DepreciationAmount = BaocaoHelper.CalculateDepreciation(amount, device.ReceivedDate)
        };
    }

    private static BaocaoOperationalRowDto MapOperationalDeviceRow(Device device, string filterLabel)
    {
        return new BaocaoOperationalRowDto
        {
            Id = device.Id,
            Name = device.Name,
            IdentityCode = device.IdentityCode,
            Manufacturer = device.Manufacturer,
            ModelCode = device.ModelCode,
            Serial = device.Serial,
            Quantity = device.Quantity,
            Unit = device.Unit,
            OrganizationName = device.Organization?.Name,
            FilterLabel = filterLabel,
            InspectionExpiryDate = device.InspectionExpiryDate,
            TotalAmount = FinancialParseHelper.ParseLegacyAmount(device.TotalAmount)
        };
    }

    private static string NormalizeSection(string? section, string fallback)
    {
        if (string.IsNullOrWhiteSpace(section))
            return fallback;
        return section.Trim().ToLowerInvariant();
    }

    private static PagedResult<T> Paginate<T>(List<T> items, int page, int pageSize)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize is < 1 or > 200 ? 10 : pageSize;
        return new PagedResult<T>
        {
            Items = items.Skip((safePage - 1) * safeSize).Take(safeSize).ToList(),
            TotalCount = items.Count,
            Page = safePage,
            PageSize = safeSize
        };
    }
}