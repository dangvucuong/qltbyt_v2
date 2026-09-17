using System.Globalization;

namespace QLTBYT.API.Helpers;

public static class FinancialParseHelper
{
    public static double ParseLegacyAmount(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0;
        var cleaned = raw.Trim().Replace(".", "").Replace(",", "");
        if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
            return v;
        cleaned = new string(raw.Where(char.IsDigit).ToArray());
        return double.TryParse(cleaned, out v) ? v : 0;
    }

    public static DateTime? ParseLegacyDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();
        if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out var d))
            return d.Date;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            return d.Date;
        return null;
    }
}