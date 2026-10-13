using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Helpers;

/// <summary>
/// Ánh xạ role JWT/UI từ bảng legacy Taikhoan (LoaiTK + Admin).
/// LoaiTK (taikhoan.aspx): 0=Ban lãnh đạo, 1=Tổ kho/thiết bị, 2=Kỹ sư khu vực, 3=Nhân viên mạng lưới.
/// </summary>
public static class RoleMapper
{
    /// <summary>Role dùng cho JWT claim và menu frontend (Admin / Technician / User).</summary>
    public static string ResolveRole(UserAccount account)
    {
        var loai = account.AccountType ?? 3;

        // Ban lãnh đạo (0) hoặc cờ Admin
        if (account.Admin == 1 || loai == 0)
            return "Admin";

        // Tổ kho, thiết bị
        if (loai == 1)
            return "Technician";

        // 2 = Kỹ sư khu vực, 3 = Nhân viên mạng lưới → User (phân quyền menu chi tiết theo LoaiTK)
        return "User";
    }

    /// <summary>Giữ cho code cũ; ưu tiên <see cref="ResolveRole"/> khi có entity.</summary>
    public static string FromAccountType(int? accountType) =>
        ResolveRole(new UserAccount
        {
            AccountType = accountType,
            Admin = accountType == 0 ? 1 : 0
        });

    public static bool IsAdminTier(UserAccount account) => ResolveRole(account) == "Admin";

    public static bool IsTechnicianOrAbove(UserAccount account) =>
        ResolveRole(account) is "Admin" or "Technician";

    /// <summary>Điều kiện SQL/EF: tài khoản nhận cảnh báo kỹ thuật (tương đương Technician+ / LoaiTK 0–2).</summary>
    public static bool MatchesTechnicianOrAboveFilter(UserAccount u) =>
        u.Admin == 1 || (u.AccountType ?? 3) <= 2;

    /// <summary>Legacy: số liệu tổng hợp (baocao) — LoaiTK 0,1,2; chặn 3.</summary>
    public static bool CanAccessReports(int? accountType, bool isAdminFlag = false) =>
        isAdminFlag || (accountType ?? 3) <= 2;

    /// <summary>Legacy: gói thầu — LoaiTK 0,1.</summary>
    public static bool CanAccessBidding(int? accountType, bool isAdminFlag = false) =>
        isAdminFlag || (accountType ?? 3) <= 1;

    /// <summary>Legacy kho TVT (Nhaphanghoa/baocaokho): LoaiTK 0 hoặc 3.</summary>
    public static bool CanAccessWarehouse(int? accountType, bool isAdminFlag = false) =>
        isAdminFlag || accountType is 0 or 3;

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
