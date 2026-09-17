namespace QLTBYT.API.Models.DTOs.User;

public class UserListDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? AccountType { get; set; }
    public string? Role { get; set; }
    public string? OrganizationIds { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int AccountType { get; set; }
    public string? OrganizationIds { get; set; }
}

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? AccountType { get; set; }
    public string? OrganizationIds { get; set; }
    public string? NewPassword { get; set; }
}

