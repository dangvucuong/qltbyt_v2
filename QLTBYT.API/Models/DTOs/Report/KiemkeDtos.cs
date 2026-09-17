namespace QLTBYT.API.Models.DTOs.Report;

public class KiemkeAssetQueryParams
{
    public int? OrganizationId { get; set; }
    public string? LocationCode { get; set; }
    public string? ProcurementSource { get; set; }
    public int? ReceivedYear { get; set; }
    public double? MinAmount { get; set; }
    public double? MaxAmount { get; set; }
    /// <summary>Department view excludes zero-value assets (legacy gridThietbi).</summary>
    public bool ExcludeZeroAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class KiemkeAssetRowDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? IdentityCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? OrganizationName { get; set; }
    public string? LocationCode { get; set; }
    public string? ProcurementSourceLabel { get; set; }
    public double? TotalAmount { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public double? DepreciationAmount { get; set; }
    public string? Note { get; set; }
}
