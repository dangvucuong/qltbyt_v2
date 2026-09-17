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
