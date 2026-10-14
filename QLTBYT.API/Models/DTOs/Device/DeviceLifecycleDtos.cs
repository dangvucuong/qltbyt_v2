namespace QLTBYT.API.Models.DTOs.Device;

/// <summary>
/// Báo cáo phân tích vòng đời thiết bị (tương đương DeviceLifecycleReport.aspx).
/// Số tiền biểu đồ TCO / lịch sử theo đơn vị Triệu VNĐ; nhật ký giữ nguyên VNĐ.
/// </summary>
public class DeviceLifecycleReportDto
{
    public DeviceLifecycleHeaderDto Header { get; set; } = new();
    public List<DeviceLifecycleTcoItemDto> TcoBreakdown { get; set; } = new();
    public List<DeviceLifecycleYearlyItemDto> YearlyHistory { get; set; } = new();
    public List<DeviceLifecycleMaintenanceLogDto> MaintenanceLogs { get; set; } = new();
}

public class DeviceLifecycleHeaderDto
{
    public int DeviceId { get; set; }
    public string? DeviceCode { get; set; }
    public string? DeviceName { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelCode { get; set; }
    public string? YearOfManufacture { get; set; }
    public string? StatusName { get; set; }
    public int HealthScore { get; set; }
    public DateTime? NextInspectionDate { get; set; }
    public string SummaryLine { get; set; } = string.Empty;
    public string StatusLine { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
}

public class DeviceLifecycleTcoItemDto
{
    public string Category { get; set; } = string.Empty;
    /// <summary>Số tiền (Triệu VNĐ).</summary>
    public decimal AmountMillion { get; set; }
}

public class DeviceLifecycleYearlyItemDto
{
    public string Year { get; set; } = string.Empty;
    public int Breakdowns { get; set; }
    /// <summary>Chi phí sửa chữa (Triệu VNĐ).</summary>
    public decimal RepairCostMillion { get; set; }
}

public class DeviceLifecycleMaintenanceLogDto
{
    public DateTime? Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Chi phí (VNĐ).</summary>
    public decimal Cost { get; set; }
    public string? Engineer { get; set; }
    public string Status { get; set; } = string.Empty;
}
