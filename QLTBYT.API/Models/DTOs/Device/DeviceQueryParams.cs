namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public int? OrganizationId { get; set; }
    public int? StatusId { get; set; }
    public int? TypeId { get; set; }
    public string? BiddingPackageCode { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int? ExpiringWithinDays { get; set; }
}
