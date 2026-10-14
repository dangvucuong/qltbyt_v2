namespace QLTBYT.API.Helpers;

/// <summary>
/// Chuẩn hóa khóa thiết bị: legacy MaTB thường NULL — dùng Quanlythietbi.ID thay cho MaTB
/// ở InputByDate.MaTB, Thietbichinh, hiển thị và tìm kiếm.
/// </summary>
public static class DeviceIdentityHelper
{
    /// <summary>Khóa lưu trong DB (InputByDate.MaTB, Thietbichinh, …) = ID thiết bị.</summary>
    public static string StorageKey(int deviceId) => deviceId.ToString();

    /// <summary>Nhãn hiển thị: MaTB nếu có, không thì ID.</summary>
    public static string DisplayCode(int deviceId, string? deviceCode) =>
        !string.IsNullOrWhiteSpace(deviceCode) ? deviceCode.Trim() : deviceId.ToString();

    /// <summary>
    /// Khóa tra cứu nhật ký sử dụng: ID (chuẩn) + MaTB cũ nếu còn (đọc dữ liệu legacy).
    /// </summary>
    public static HashSet<string> LookupKeys(int deviceId, string? deviceCode)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            deviceId.ToString()
        };
        if (!string.IsNullOrWhiteSpace(deviceCode))
            keys.Add(deviceCode.Trim());
        return keys;
    }

    public static bool IsParentOf(string? childParentRef, int parentId, string? parentMaTb)
    {
        if (string.IsNullOrWhiteSpace(childParentRef)) return false;
        var p = childParentRef.Trim();
        if (p == parentId.ToString()) return true;
        return !string.IsNullOrWhiteSpace(parentMaTb) &&
               string.Equals(p, parentMaTb.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
