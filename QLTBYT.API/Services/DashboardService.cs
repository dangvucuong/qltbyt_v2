using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.DTOs.Dashboard;

namespace QLTBYT.API.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMemoryCache _cache;

    public DashboardService(AppDbContext context, ICurrentUserService currentUser, IMemoryCache cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(int expiringWithinDays = 30)
    {
        var key = $"dashboard:{_currentUser.UserId}:{expiringWithinDays}";
        if (_cache.TryGetValue(key, out DashboardSummaryDto? cached) && cached is not null)
            return cached;

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

        var orgRows = await devices
            .Include(d => d.Organization)
            .Select(d => new { d.OrganizationId, OrgName = d.Organization != null ? d.Organization.Name : null, d.TotalAmount })
            .ToListAsync();

        var byOrganization = orgRows
            .GroupBy(x => x.OrganizationId)
            .Select(g => new OrganizationStatsDto
            {
                OrganizationId = g.Key,
                Label = g.First().OrgName ?? "Không rõ",
                DeviceCount = g.Count(),
                TotalAssetValue = g.Sum(x => FinancialParseHelper.ParseLegacyAmount(x.TotalAmount))
            })
            .OrderByDescending(x => x.DeviceCount)
            .ToList();

        var result = new DashboardSummaryDto
        {
            TotalDevices = total,
            ExpiringInspectionCount = expiring,
            ByStatus = byStatus.Select(x => new ChartItemDto
            {
                Label = x.Id.HasValue && statusNames.ContainsKey(x.Id.Value) ? statusNames[x.Id.Value]! : "Không rõ",
                Value = x.Count
            }).ToList(),
            ByType = byType.Select(x => new ChartItemDto
            {
                Label = x.Id.HasValue && typeNames.ContainsKey(x.Id.Value) ? typeNames[x.Id.Value]! : "Không rõ",
                Value = x.Count
            }).ToList(),
            ByOrganization = byOrganization
        };

        _cache.Set(key, result, TimeSpan.FromSeconds(60));
        return result;
    }

    private IQueryable<Models.Domain.Device> ApplyScope(IQueryable<Models.Domain.Device> q)
    {
        if (!_currentUser.MustFilterByOrganization) return q;
        var allowed = _currentUser.OrganizationIds;
        if (allowed.Count == 0) return q.Where(d => false);
        return q.Where(d => d.OrganizationId != null && allowed.Contains(d.OrganizationId.Value));
    }
}