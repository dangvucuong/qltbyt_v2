namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceListDto
{
    public int Id { get; set; }
    public string? DeviceCode { get; set; }
    public string? Name { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
}
