namespace QLTBYT.API.Models.DTOs.Auth;

public class ResetPasswordRequest
{
    public string Username { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
