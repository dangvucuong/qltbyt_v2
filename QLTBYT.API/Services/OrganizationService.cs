using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.DTOs.Organization;

namespace QLTBYT.API.Services;

public partial class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public OrganizationService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<OrganizationListDto>> GetOrganizationsAsync()
    {
        var query = _context.Organizations.AsNoTracking()
            .Include(o => o.OrganizationType)
            .AsQueryable();

        if (_currentUser.MustFilterByOrganization)
        {
            var allowed = _currentUser.OrganizationIds;
            query = query.Where(o => allowed.Contains(o.Id));
        }

        return await query
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationListDto
            {
                Id = o.Id,
                Name = o.Name,
                TypeId = o.TypeId,
                TypeName = o.OrganizationType != null ? o.OrganizationType.Name : null,
                Phone = o.Phone,
                Email = o.Email
            })
            .ToListAsync();
    }
}
