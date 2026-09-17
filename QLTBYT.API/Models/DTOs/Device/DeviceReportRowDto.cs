namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceReportRowDto
{
    public int Id { get; set; }
    public string? DeviceCode { get; set; }
    public string? IdentityCode { get; set; }
    public string? Name { get; set; }
    public string? TypeName { get; set; }
    public string? OrganizationName { get; set; }
    public string? StatusName { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public DateTime? MaintenanceDueDate { get; set; }
    public string? Classification { get; set; }
    public double? TotalAmount { get; set; }
}