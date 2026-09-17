namespace QLTBYT.API.Models.DTOs.Organization;

public class OrganizationListDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
}
