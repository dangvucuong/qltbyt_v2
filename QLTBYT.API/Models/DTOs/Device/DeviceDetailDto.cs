namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceDetailDto
{
    public int Id { get; set; }
    public string? DeviceCode { get; set; }
    public string? TechnicalCode { get; set; }
    public string? IdentityCode { get; set; }
    public string? Name { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public string? Manufacturer { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? ParentDeviceCode { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InspectionExpiryDate { get; set; }
    public string? Note { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? TechnicalSpecs { get; set; }
    public string? YearOfManufacture { get; set; }
    public string? ConditionNote { get; set; }
    public string? DeviceGroup { get; set; }
    public string? Classification { get; set; }
    public int? HandoverOrgId { get; set; }
    public int? MaintenanceOrgId { get; set; }
    public string? BiddingPackageCode { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? UnitPrice { get; set; }
    public string? TotalAmount { get; set; }
    public string? LocationCode { get; set; }
    public string? ProcurementSource { get; set; }
    public string? WarrantyDueMarker { get; set; }
    public string? MaintenanceDueMarker { get; set; }
    public string? CalibrationDueMarker { get; set; }
    public string? InspectionDueMarker { get; set; }
    public string? PutIntoUseMarker { get; set; }
    public string? SpecializedName { get; set; }
    public string? LegalDocumentRef { get; set; }

    public List<DeviceFileDto> Files { get; set; } = new List<DeviceFileDto>();
    public List<DeviceCodeDto> Codes { get; set; } = new List<DeviceCodeDto>();
}
