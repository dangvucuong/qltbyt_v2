using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public partial class DeviceService
{
    private const double Million = 1_000_000d;

    public async Task<DeviceLifecycleReportDto?> GetLifecycleReportAsync(int id)
    {
        var device = await _context.Devices.AsNoTracking()
            .Include(d => d.DeviceStatus)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (device is null || !IsDeviceAllowed(device))
            return null;

        var events = await _context.DeviceEvents.AsNoTracking()
            .Where(e => e.DeviceId == id)
            .OrderByDescending(e => e.EventDateTime)
            .ThenByDescending(e => e.Id)
            .ToListAsync();

        var managerIds = events
            .Where(e => e.ManagerUserId.HasValue)
            .Select(e => e.ManagerUserId!.Value)
            .Distinct()
            .ToList();
        var managers = managerIds.Count == 0
            ? new Dictionary<int, string?>()
            : await _context.UserAccounts.AsNoTracking()
                .Where(u => managerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.Username);

        var capEx = FinancialParseHelper.ParseLegacyAmount(device.TotalAmount);
        var maintenanceRepair = events
            .Where(e => e.TypeId is 1 or 2 or 3)
            .Sum(e => e.Cost ?? 0);
        var transferRelated = events
            .Where(e => e.TypeId == 4)
            .Sum(e => e.Cost ?? 0);
        // Vật tư: Quanlyvattu không có đơn giá — ước lượng 0; giữ category khi có dữ liệu khác.
        var spareParts = 0d;
        var operating = device.ProcessingCost ?? 0d;

        var tco = new List<DeviceLifecycleTcoItemDto>();
        AddTcoIfPositive(tco, "Chi phí mua sắm ban đầu (CapEx)", capEx);
        AddTcoIfPositive(tco, "Chi phí bảo trì & sửa chữa", maintenanceRepair + transferRelated);
        AddTcoIfPositive(tco, "Chi phí vật tư & linh kiện thay thế", spareParts);
        AddTcoIfPositive(tco, "Chi phí vận hành / xử lý", operating);

        var yearly = BuildYearlyHistory(events);
        var logs = events.Select(e => new DeviceLifecycleMaintenanceLogDto
        {
            Date = e.EventDateTime,
            Type = MapLifecycleTypeName(e.TypeId),
            Description = e.Content,
            Cost = (decimal)(e.Cost ?? 0),
            Engineer = ResolveEngineer(e, managers),
            Status = DeviceEventLabels.GetStatusName(e.StatusId)
        }).ToList();

        var health = ComputeHealthScore(device, events);
        var statusName = device.DeviceStatus?.Name ?? "Không rõ";
        var nextInspection = device.InspectionExpiryDate;
        var summary = BuildSummaryLine(device);
        var statusLine =
            $"Trạng thái hiện tại: {statusName} (Health Score: {health}/100)" +
            (nextInspection.HasValue
                ? $" - Lịch kiểm định tiếp theo: {nextInspection.Value:dd/MM/yyyy}"
                : " - Chưa có lịch kiểm định");

        return new DeviceLifecycleReportDto
        {
            Header = new DeviceLifecycleHeaderDto
            {
                DeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                DeviceName = device.Name,
                Manufacturer = device.Manufacturer,
                ModelCode = device.ModelCode,
                YearOfManufacture = device.YearOfManufacture,
                StatusName = statusName,
                HealthScore = health,
                NextInspectionDate = nextInspection,
                SummaryLine = summary,
                StatusLine = statusLine,
                IsHealthy = health >= 60 && device.StatusId is 1 or null
            },
            TcoBreakdown = tco,
            YearlyHistory = yearly,
            MaintenanceLogs = logs
        };
    }

    private static void AddTcoIfPositive(List<DeviceLifecycleTcoItemDto> list, string category, double amountVnd)
    {
        if (amountVnd <= 0) return;
        list.Add(new DeviceLifecycleTcoItemDto
        {
            Category = category,
            AmountMillion = Math.Round((decimal)(amountVnd / Million), 3)
        });
    }

    private static List<DeviceLifecycleYearlyItemDto> BuildYearlyHistory(List<DeviceEvent> events)
    {
        var currentYear = DateTime.Today.Year;
        var startYear = currentYear - 4;
        var dated = events.Where(e => e.EventDateTime.HasValue).ToList();
        if (dated.Count > 0)
        {
            var minYear = dated.Min(e => e.EventDateTime!.Value.Year);
            if (minYear < startYear) startYear = minYear;
        }

        var result = new List<DeviceLifecycleYearlyItemDto>();
        for (var year = startYear; year <= currentYear; year++)
        {
            var yearEvents = dated.Where(e => e.EventDateTime!.Value.Year == year).ToList();
            var breakdowns = yearEvents.Count(e => e.TypeId == 1);
            var repairCost = yearEvents
                .Where(e => e.TypeId is 1 or 2 or 3)
                .Sum(e => e.Cost ?? 0);
            result.Add(new DeviceLifecycleYearlyItemDto
            {
                Year = year.ToString(),
                Breakdowns = breakdowns,
                RepairCostMillion = Math.Round((decimal)(repairCost / Million), 3)
            });
        }

        return result;
    }

    private static int ComputeHealthScore(Device device, List<DeviceEvent> events)
    {
        var score = 100;
        if (device.StatusId is 2 or 8)
            score -= 25;
        else if (device.StatusId is 3)
            score -= 10;

        var recentBreakdowns = events.Count(e =>
            e.TypeId == 1 &&
            e.EventDateTime.HasValue &&
            e.EventDateTime.Value >= DateTime.Today.AddYears(-1));
        score -= Math.Min(40, recentBreakdowns * 8);

        if (device.InspectionExpiryDate.HasValue)
        {
            var days = (device.InspectionExpiryDate.Value.Date - DateTime.Today).TotalDays;
            if (days < 0) score -= 20;
            else if (days <= 30) score -= 10;
            else if (days <= 90) score -= 5;
        }

        var openIncidents = events.Count(e => e.StatusId == 1 && e.TypeId is 1 or 2);
        score -= Math.Min(15, openIncidents * 5);

        return Math.Clamp(score, 0, 100);
    }

    private static string BuildSummaryLine(Device device)
    {
        var parts = new List<string> { $"Thiết bị: {device.Name ?? "—"}" };
        if (!string.IsNullOrWhiteSpace(device.Manufacturer))
            parts.Add($"Hãng: {device.Manufacturer.Trim()}");
        if (!string.IsNullOrWhiteSpace(device.ModelCode))
            parts.Add($"Model: {device.ModelCode.Trim()}");
        if (!string.IsNullOrWhiteSpace(device.YearOfManufacture))
            parts.Add($"Năm SX: {device.YearOfManufacture.Trim()}");
        parts.Add($"ID: {device.Id}");
        if (!string.IsNullOrWhiteSpace(device.DeviceCode))
            parts.Add($"Mã TB: {device.DeviceCode.Trim()}");
        return string.Join(" - ", parts);
    }

    private static string MapLifecycleTypeName(int? typeId) =>
        typeId switch
        {
            1 => "Sửa chữa đột xuất",
            2 => "Bảo trì / kiểm tra",
            3 => "Yêu cầu khác",
            4 => "Chuyển giao",
            _ => DeviceEventLabels.GetTypeName(typeId)
        };

    private static string? ResolveEngineer(DeviceEvent e, Dictionary<int, string?> managers)
    {
        if (e.ManagerUserId.HasValue &&
            managers.TryGetValue(e.ManagerUserId.Value, out var name) &&
            !string.IsNullOrWhiteSpace(name))
            return name;
        return string.IsNullOrWhiteSpace(e.AccountUsername) ? null : e.AccountUsername;
    }
}
