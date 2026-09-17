namespace QLTBYT.API.Models.DTOs.Organization;

public class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public int? TypeId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
}

public class UpdateOrganizationRequest
{
    public string? Name { get; set; }
    public int? TypeId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
}
