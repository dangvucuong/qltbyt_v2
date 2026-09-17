# -*- coding: utf-8 -*-
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def w(rel: str, content: str) -> None:
    p = ROOT / rel.replace("/", "\\")
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content.strip() + "\n", encoding="utf-8", newline="\n")
    print("written", rel)

w("QLTBYT.API/Helpers/OrganizationScopeHelper.cs", """
namespace QLTBYT.API.Helpers;

public static class OrganizationScopeHelper
{
    public static List<int> ParseOrganizationIds(string? maDv)
    {
        if (string.IsNullOrWhiteSpace(maDv))
        {
            return [];
        }

        return maDv.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    public static bool IsOrganizationAllowed(int? organizationId, IReadOnlyList<int> allowedIds)
    {
        if (organizationId is null)
        {
            return false;
        }

        return allowedIds.Contains(organizationId.Value);
    }
}
""")

w("QLTBYT.API/Services/ICurrentUserService.cs", """
namespace QLTBYT.API.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? AccountType { get; }
    string Role { get; }
    string? OrganizationIdsRaw { get; }
    IReadOnlyList<int> OrganizationIds { get; }
    bool MustFilterByOrganization { get; }
}
""")

w("QLTBYT.API/Services/CurrentUserService.cs", """
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using QLTBYT.API.Helpers;

namespace QLTBYT.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId => int.TryParse(GetClaim(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public int? AccountType => int.TryParse(GetClaim("LoaiTK"), out var t) ? t : null;

    public string Role => GetClaim(ClaimTypes.Role) ?? "User";

    public string? OrganizationIdsRaw => GetClaim("MaDV");

    public IReadOnlyList<int> OrganizationIds =>
        OrganizationScopeHelper.ParseOrganizationIds(OrganizationIdsRaw);

    public bool MustFilterByOrganization => AccountType == 3;

    private string? GetClaim(string type) =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(type);
}
""")

w("QLTBYT.API/Models/DTOs/Auth/ChangePasswordRequest.cs", """
namespace QLTBYT.API.Models.DTOs.Auth;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
""")

w("QLTBYT.API/Models/DTOs/Auth/ForgotPasswordRequest.cs", """
namespace QLTBYT.API.Models.DTOs.Auth;

public class ForgotPasswordRequest
{
    public string? Email { get; set; }
    public string? Username { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Auth/ResetPasswordRequest.cs", """
namespace QLTBYT.API.Models.DTOs.Auth;

public class ResetPasswordRequest
{
    public string Username { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
""")

w("QLTBYT.API/Models/DTOs/Auth/ForgotPasswordResponse.cs", """
namespace QLTBYT.API.Models.DTOs.Auth;

public class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;
    public string? ResetToken { get; set; }
}
""")

w("QLTBYT.API/Validators/ChangePasswordRequestValidator.cs", """
using FluentValidation;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Mat khau cu khong duoc de trống.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mat khau moi khong duoc de trống.")
            .MinimumLength(6).WithMessage("Mat khau moi phai co it nhat 6 ky tu.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Mat khau moi phai khac mat khau cu.");
    }
}
""")

w("QLTBYT.API/Validators/ForgotPasswordRequestValidator.cs", """
using FluentValidation;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Username))
            .WithMessage("Can nhap email hoac ten dang nhap.");
    }
}
""")

w("QLTBYT.API/Validators/ResetPasswordRequestValidator.cs", """
using FluentValidation;
using QLTBYT.API.Models.DTOs.Auth;

namespace QLTBYT.API.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Ten dang nhap khong duoc de trống.");
        RuleFor(x => x.ResetToken).NotEmpty().WithMessage("Ma dat lai khong hop le.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mat khau moi khong duoc de trống.")
            .MinimumLength(6).WithMessage("Mat khau moi phai co it nhat 6 ky tu.");
    }
}
""")

print("done")
