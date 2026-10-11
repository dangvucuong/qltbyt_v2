using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.DTOs.Master;

namespace QLTBYT.API.Services;

public class MasterDataService : IMasterDataService
{
    private readonly AppDbContext _context;

    public MasterDataService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MasterItemDto>> GetDeviceTypesAsync() =>
        await _context.DeviceTypes.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new MasterItemDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

    public async Task<List<MasterItemDto>> GetDeviceStatusesAsync() =>
        await _context.DeviceStatuses.AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new MasterItemDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

    public async Task<List<MasterItemDto>> GetOrganizationTypesAsync() =>
        await _context.OrganizationTypes.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new MasterItemDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

    public async Task<List<MasterItemDto>> GetCountriesAsync() =>
        await _context.Countries.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new MasterItemDto { Id = c.Id, Name = c.Name })
            .ToListAsync();

    public async Task<List<MasterItemDto>> GetTaskTypesAsync() =>
        await _context.TaskTypes.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new MasterItemDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

    public async Task<List<MasterItemDto>> GetEventKeywordsAsync() =>
        await _context.EventKeywords.AsNoTracking()
            .OrderBy(k => k.Id)
            .Select(k => new MasterItemDto { Id = k.Id, Name = k.Keyword.Trim() })
            .ToListAsync();

    public async Task<List<MasterItemDto>> GetPersonnelTypesAsync() =>
        await _context.PersonnelTypes.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new MasterItemDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

    public async Task<List<MasterItemDto>> GetDocumentTypesAsync() =>
        await _context.DocumentTypes.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new MasterItemDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

    public async Task<List<BiddingPackageLookupDto>> GetBiddingPackagesAsync() =>
        await _context.BiddingPackages.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BiddingPackageLookupDto
            {
                PackageCode = b.PackageCode,
                Name = b.Name,
                WinningVendor = b.WinningVendor,
            })
            .ToListAsync();
}
