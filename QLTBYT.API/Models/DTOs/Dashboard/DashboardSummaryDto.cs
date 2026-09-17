namespace QLTBYT.API.Models.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalDevices { get; set; }
    public int ExpiringInspectionCount { get; set; }
    public List<ChartItemDto> ByStatus { get; set; } = new();
    public List<ChartItemDto> ByType { get; set; } = new();
    public List<OrganizationStatsDto> ByOrganization { get; set; } = new();
}

public class ChartItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class OrganizationStatsDto
{
    public int? OrganizationId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int DeviceCount { get; set; }
    public double TotalAssetValue { get; set; }
}