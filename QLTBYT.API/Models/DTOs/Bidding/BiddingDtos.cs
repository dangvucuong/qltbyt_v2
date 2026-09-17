namespace QLTBYT.API.Models.DTOs.Bidding;

public class BiddingPackageDto
{
    public int Id { get; set; }
    public string? PackageCode { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? WinningVendor { get; set; }
    public string? TaxCode { get; set; }
    public string? ContractNumber { get; set; }
    public double? Value { get; set; }
    public int DeviceCount { get; set; }
}

public class CreateBiddingPackageRequest
{
    public string PackageCode { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? WinningVendor { get; set; }
    public string? TaxCode { get; set; }
    public string? ContractNumber { get; set; }
    public double? Value { get; set; }
}

public class UpdateBiddingPackageRequest
{
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? WinningVendor { get; set; }
    public string? TaxCode { get; set; }
    public string? ContractNumber { get; set; }
    public double? Value { get; set; }
}
