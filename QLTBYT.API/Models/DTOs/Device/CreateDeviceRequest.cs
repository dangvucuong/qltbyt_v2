namespace QLTBYT.API.Models.DTOs.Device;

public class CreateDeviceRequest
{
    public string DeviceCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public int OrganizationId { get; set; }
    public int StatusId { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public string? Manufacturer { get; set; }
    public string? CountryOfOrigin { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public string? Note { get; set; }
    public string? ParentDeviceCode { get; set; }
}
