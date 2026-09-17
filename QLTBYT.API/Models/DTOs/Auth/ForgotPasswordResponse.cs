namespace QLTBYT.API.Models.DTOs.Auth;

public class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;
    public string? ResetToken { get; set; }
}
