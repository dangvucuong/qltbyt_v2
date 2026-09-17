# -*- coding: utf-8 -*-
"""Generate remaining Phase 1 backend files (UTF-8)."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
API = ROOT / "QLTBYT.API"

def w(rel: str, content: str) -> None:
    p = ROOT / rel.replace("/", "\\")
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content.strip() + "\n", encoding="utf-8", newline="\n")
    print("ok", rel)

w("QLTBYT.API/Models/DTOs/Master/MasterItemDto.cs", """
namespace QLTBYT.API.Models.DTOs.Master;

public class MasterItemDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Organization/OrganizationListDto.cs", """
namespace QLTBYT.API.Models.DTOs.Organization;

public class OrganizationListDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Dashboard/DashboardSummaryDto.cs", """
namespace QLTBYT.API.Models.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalDevices { get; set; }
    public int ExpiringInspectionCount { get; set; }
    public List<ChartItemDto> ByStatus { get; set; } = new List<ChartItemDto>();
    public List<ChartItemDto> ByType { get; set; } = new List<ChartItemDto>();
}

public class ChartItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}
""")

w("QLTBYT.API/Models/DTOs/Device/DeviceQueryParams.cs", """
namespace QLTBYT.API.Models.DTOs.Device;

public class DeviceQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public int? OrganizationId { get; set; }
    public int? StatusId { get; set; }
    public int? TypeId { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int? ExpiringWithinDays { get; set; }
}
""")

w("QLTBYT.API/Services/IMasterDataService.cs", """
using QLTBYT.API.Models.DTOs.Master;

namespace QLTBYT.API.Services;

public interface IMasterDataService
{
    Task<List<MasterItemDto>> GetDeviceTypesAsync();
    Task<List<MasterItemDto>> GetDeviceStatusesAsync();
    Task<List<MasterItemDto>> GetOrganizationTypesAsync();
    Task<List<MasterItemDto>> GetCountriesAsync();
    Task<List<MasterItemDto>> GetTaskTypesAsync();
    Task<List<MasterItemDto>> GetEventKeywordsAsync();
}
""")

w("QLTBYT.API/Services/MasterDataService.cs", """
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
}
""")

w("QLTBYT.API/Services/IOrganizationService.cs", """
using QLTBYT.API.Models.DTOs.Organization;

namespace QLTBYT.API.Services;

public interface IOrganizationService
{
    Task<List<OrganizationListDto>> GetOrganizationsAsync();
}
""")

w("QLTBYT.API/Services/OrganizationService.cs", """
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.DTOs.Organization;

namespace QLTBYT.API.Services;

public class OrganizationService : IOrganizationService
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
""")

w("QLTBYT.API/Services/IDashboardService.cs", """
using QLTBYT.API.Models.DTOs.Dashboard;

namespace QLTBYT.API.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int expiringWithinDays = 30);
}
""")

w("QLTBYT.API/Services/DashboardService.cs", """
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.DTOs.Dashboard;

namespace QLTBYT.API.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(int expiringWithinDays = 30)
    {
        var devices = ApplyScope(_context.Devices.AsNoTracking());
        var total = await devices.CountAsync();
        var threshold = DateTime.Today.AddDays(expiringWithinDays);
        var expiring = await devices.CountAsync(d =>
            d.InspectionExpiryDate != null && d.InspectionExpiryDate <= threshold);

        var byStatus = await devices
            .GroupBy(d => d.StatusId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToListAsync();

        var statusNames = await _context.DeviceStatuses.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Name ?? "");

        var byType = await devices
            .GroupBy(d => d.TypeId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToListAsync();

        var typeNames = await _context.DeviceTypes.AsNoTracking().ToDictionaryAsync(t => t.Id, t => t.Name ?? "");

        return new DashboardSummaryDto
        {
            TotalDevices = total,
            ExpiringInspectionCount = expiring,
            ByStatus = byStatus.Select(x => new ChartItemDto
            {
                Label = x.Id.HasValue && statusNames.ContainsKey(x.Id.Value) ? statusNames[x.Id.Value]! : "Khong ro",
                Value = x.Count
            }).ToList(),
            ByType = byType.Select(x => new ChartItemDto
            {
                Label = x.Id.HasValue && typeNames.ContainsKey(x.Id.Value) ? typeNames[x.Id.Value]! : "Khong ro",
                Value = x.Count
            }).ToList()
        };
    }

    private IQueryable<QLTBYT.API.Models.Domain.Device> ApplyScope(IQueryable<QLTBYT.API.Models.Domain.Device> q)
    {
        if (!_currentUser.MustFilterByOrganization)
        {
            return q;
        }

        var allowed = _currentUser.OrganizationIds;
        if (allowed.Count == 0)
        {
            return q.Where(d => false);
        }

        return q.Where(d => d.OrganizationId != null && allowed.Contains(d.OrganizationId.Value));
    }
}
""")

w("QLTBYT.API/Services/IFileStorageService.cs", """
namespace QLTBYT.API.Services;

public interface IFileStorageService
{
    Task<string> SaveDeviceFileAsync(int deviceId, Stream content, string fileName);
    string GetRootPath();
}
""")

w("QLTBYT.API/Services/LocalFileStorageService.cs", """
using Microsoft.AspNetCore.Hosting;

namespace QLTBYT.API.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _root = Path.Combine(env.ContentRootPath, "FILELUU");
        Directory.CreateDirectory(_root);
    }

    public string GetRootPath() => _root;

    public async Task<string> SaveDeviceFileAsync(int deviceId, Stream content, string fileName)
    {
        var safeName = Path.GetFileName(fileName);
        var folder = Path.Combine(_root, deviceId.ToString());
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, safeName);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);
        return Path.Combine("FILELUU", deviceId.ToString(), safeName).Replace('\\', '/');
    }
}
""")

print("batch1 done")
