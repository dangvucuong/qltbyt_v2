namespace QLTBYT.API.Helpers;

public static class OrganizationScopeHelper
{
    public static List<int> ParseOrganizationIds(string? maDv)
    {
        if (string.IsNullOrWhiteSpace(maDv))
        {
            return new List<int>();
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
