using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Helpers;

public static class BaocaoHelper
{
    public static bool HasAssetValue(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.TotalAmount)) return false;
        var raw = device.TotalAmount.Trim();
        if (raw is "" or "0") return false;
        return FinancialParseHelper.ParseLegacyAmount(raw) > 0;
    }

    public static double CalculateDepreciation(double amount, DateTime? receivedDate)
    {
        if (amount <= 0 || receivedDate is null) return amount;
        var daysPassed = (DateTime.Today - receivedDate.Value.Date).Days;
        if (daysPassed <= 0) return Math.Floor(amount);
        var years = (int)(daysPassed / 365.25) + 1;
        var value = amount * (1 - years * 0.125);
        if (value < 0) value = 0;
        return Math.Floor(value);
    }

    public static string ResolveProcurementLabel(string? code) => code switch
    {
        "1" => "Ngu\u1ed3n d\u1ef1 \u00e1n",
        "2" => "Qu\u1ef9 ph\u00e1t tri\u1ec3n s\u1ef1 nghi\u1ec7p",
        "3" => "T\u00e0i tr\u1ee3 - vi\u1ec7n tr\u1ee3",
        "4" => "Ngu\u1ed3n ng\u00e2n s\u00e1ch",
        "5" => "Ngu\u1ed3n vay k\u00edch c\u1ea7u",
        _ => code ?? string.Empty
    };

    public static string ResolveMaintenanceLabel(int? typeId) => typeId switch
    {
        1 => "T\u1ef1 b\u1ea3o tr\u00ec",
        2 => "B\u1ea3o tr\u00ec c\u01a1 b\u1ea3n",
        3 => "B\u1ea3o tr\u00ec tr\u1ecdn g\u00f3i",
        4 => "B\u1ea3o tr\u00ec c\u01a1 b\u1ea3n c\u00f3 thay th\u1ebf linh ki\u1ec7n",
        _ => typeId?.ToString() ?? string.Empty
    };
}
