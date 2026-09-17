namespace QLTBYT.API.Models.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfoDto User { get; set; } = new();
}
