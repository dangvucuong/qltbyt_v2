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

    public bool MustFilterByOrganization =>
        Role != "Admin" && Role != "Technician";

    private string? GetClaim(string type) =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(type);
}
