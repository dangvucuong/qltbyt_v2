namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceSupplyDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Unit { get; set; }
    public int? Quantity { get; set; }
}

public class CreateDeviceSupplyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int? Quantity { get; set; }
}

public class TransferDeviceRequest
{
    public int ToOrganizationId { get; set; }
    public int? HandoverOrganizationId { get; set; }
    public string? Note { get; set; }
}

public class UpdateDeviceQrRequest
{
    public string? QrCode { get; set; }
    public string? Nfc { get; set; }
}

public class DeviceHistoryItemDto
{
    public string Type { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Actor { get; set; }
}

public class DeviceReportSummaryDto
{
    public int TotalDevices { get; set; }
    public int ExpiringInspectionCount { get; set; }
    public int MaintenanceDueCount { get; set; }
    public int ByStatusActive { get; set; }
    public int ByStatusInactive { get; set; }
}
