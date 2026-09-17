namespace QLTBYT.API.Models.DTOs.Auth;

public class UserInfoDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = string.Empty;
    public int? AccountType { get; set; }
    public string? OrganizationIds { get; set; }
}
