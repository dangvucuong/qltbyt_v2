namespace QLTBYT.API.Models.DTOs.Personnel;

public class PersonnelDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? PersonnelTypeId { get; set; }
    public int? UserAccountId { get; set; }
}

public class CreatePersonnelRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? IdentityNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? OrganizationId { get; set; }
    public int? PersonnelTypeId { get; set; }
    public int? UserAccountId { get; set; }
}

public class UpdatePersonnelRequest
{
    public string? FullName { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? OrganizationId { get; set; }
    public int? PersonnelTypeId { get; set; }
    public int? UserAccountId { get; set; }
}

