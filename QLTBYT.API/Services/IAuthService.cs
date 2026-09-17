using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest request, bool includeDevToken);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<UserInfoDto?> GetUserInfoAsync(int userId);
}
