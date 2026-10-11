using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthService(AppDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || string.IsNullOrEmpty(user.Password))
        {
            return null;
        }

        if (!PasswordHasher.Verify(request.Password, user.Password))
        {
            return null;
        }

        if (!PasswordHasher.IsBcryptHash(user.Password))
        {
            user.Password = PasswordHasher.Hash(request.Password);
            await _context.SaveChangesAsync();
        }

        return new LoginResponse
        {
            Token = _jwtHelper.GenerateToken(user),
            User = RoleMapper.ToUserInfo(user)
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null || string.IsNullOrEmpty(user.Password))
        {
            return false;
        }

        if (!PasswordHasher.Verify(request.CurrentPassword, user.Password))
        {
            return false;
        }

        user.Password = PasswordHasher.Hash(request.NewPassword);
        user.RegId = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest request, bool includeDevToken)
    {
        var user = await FindUserForForgotAsync(request);
        if (user is null)
        {
            return null;
        }

        var token = Guid.NewGuid().ToString("N")[..12];
        var expiry = DateTime.UtcNow.AddHours(1);
        user.RegId = $"RESET:{token}:{expiry:O}";
        await _context.SaveChangesAsync();

        return new ForgotPasswordResponse
        {
            Message = "Nếu tài khoản tồn tại, mã đặt lại mật khẩu đã được tạo (Development: xem ResetToken).",
            ResetToken = includeDevToken ? token : null
        };
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null || string.IsNullOrWhiteSpace(user.RegId))
        {
            return false;
        }

        if (!TryParseResetToken(user.RegId, out var expectedToken, out var expiry))
        {
            return false;
        }

        if (!string.Equals(expectedToken, request.ResetToken.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (expiry < DateTime.UtcNow)
        {
            return false;
        }

        user.Password = PasswordHasher.Hash(request.NewPassword);
        user.RegId = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(int userId)
    {
        var user = await _context.UserAccounts.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        return user is null ? null : RoleMapper.ToUserInfo(user);
    }

    private async Task<QLTBYT.API.Models.Domain.UserAccount?> FindUserForForgotAsync(ForgotPasswordRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            return await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == request.Username.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim();
            return await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == email);
        }

        return null;
    }

    private static bool TryParseResetToken(string regId, out string token, out DateTime expiry)
    {
        token = string.Empty;
        expiry = DateTime.MinValue;
        if (!regId.StartsWith("RESET:", StringComparison.Ordinal))
        {
            return false;
        }

        var parts = regId.Split(':', 3);
        if (parts.Length != 3)
        {
            return false;
        }

        token = parts[1];
        return DateTime.TryParse(parts[2], null, System.Globalization.DateTimeStyles.RoundtripKind, out expiry);
    }
}
