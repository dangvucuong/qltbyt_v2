using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Helpers;

/// <summary>
/// Ánh xạ role JWT/UI từ bảng legacy Taikhoan (LoaiTK + Admin).
/// </summary>
public static class RoleMapper
{
    /// <summary>Role dùng cho JWT claim và menu frontend (Admin / Technician / User).</summary>
    public static string ResolveRole(UserAccount account)
    {
        if (account.Admin == 1)
        {
            return "Admin";
        }

        var loai = account.AccountType ?? 3;

        // Legacy WebForms: LoaiTK < 2 = quyen admin (0, 1)
        if (loai < 2)
        {
            return "Admin";
        }

        if (loai == 2)
        {
            return "Technician";
        }

        return "User";
    }

    /// <summary>Giữ cho code cũ; ưu tiên <see cref="ResolveRole"/> khi có entity.</summary>
    public static string FromAccountType(int? accountType) =>
        ResolveRole(new UserAccount { AccountType = accountType, Admin = accountType == 1 ? 1 : 0 });

    public static bool IsAdminTier(UserAccount account) => ResolveRole(account) == "Admin";

    public static bool IsTechnicianOrAbove(UserAccount account) =>
        ResolveRole(account) is "Admin" or "Technician";

    /// <summary>Điều kiện SQL/EF: tài khoản nhận cảnh báo kỹ thuật (tương đương Technician+).</summary>
    public static bool MatchesTechnicianOrAboveFilter(UserAccount u) =>
        u.Admin == 1 || u.AccountType == 1 || u.AccountType == 2 || (u.AccountType ?? 3) < 2;

    public static UserInfoDto ToUserInfo(UserAccount account)
    {
        return new UserInfoDto
        {
            Id = account.Id,
            Username = account.Username ?? string.Empty,
            FullName = account.FullName,
            AccountType = account.AccountType,
            Role = ResolveRole(account),
            OrganizationIds = account.OrganizationIds
        };
    }
}
