using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Audit;

namespace QLTBYT.API.Services;

public interface ISystemAuditService
{
    Task WriteAsync(int? userId, string content, int? deviceId = null);
    Task<PagedResult<SystemAuditLogDto>> GetLogsAsync(SystemAuditQueryParams query);
}

public class SystemAuditService : ISystemAuditService
{
    private readonly AppDbContext _context;

    public SystemAuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task WriteAsync(int? userId, string content, int? deviceId = null)
    {
        _context.SystemAuditLogs.Add(new SystemAuditLog
        {
            UserId = userId,
            Content = content,
            Timestamp = DateTime.Now,
            DeviceId = deviceId
        });
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<SystemAuditLogDto>> GetLogsAsync(SystemAuditQueryParams query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var q = _context.SystemAuditLogs.AsNoTracking().AsQueryable();

        if (query.UserId.HasValue)
            q = q.Where(l => l.UserId == query.UserId);

        if (query.DeviceId.HasValue)
            q = q.Where(l => l.DeviceId == query.DeviceId);

        if (query.FromDate.HasValue)
            q = q.Where(l => l.Timestamp >= query.FromDate.Value);

        if (query.ToDate.HasValue)
        {
            var to = query.ToDate.Value.Date.AddDays(1);
            q = q.Where(l => l.Timestamp < to);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(l => l.Content != null && l.Content.Contains(term));
        }

        var total = await q.CountAsync();

        // SQL Server cũ (compat < 2012) không hỗ trợ OFFSET/FETCH — phân trang in-memory theo Id.
        var sortRows = await q
            .Select(l => new { l.Id, l.Timestamp })
            .ToListAsync();
        var pageIds = sortRows
            .OrderByDescending(x => x.Timestamp)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToList();

        if (pageIds.Count == 0)
        {
            return new PagedResult<SystemAuditLogDto>
            {
                Items = new List<SystemAuditLogDto>(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
            };
        }

        var fetched = await _context.SystemAuditLogs.AsNoTracking()
            .Where(l => pageIds.Contains(l.Id))
            .ToListAsync();
        var rows = pageIds.Select(id => fetched.First(r => r.Id == id)).ToList();

        var userIds = rows.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).Distinct().ToList();
        var deviceIds = rows.Where(r => r.DeviceId.HasValue).Select(r => r.DeviceId!.Value).Distinct().ToList();

        var users = await _context.UserAccounts.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Username, u.FullName })
            .ToListAsync();
        var userMap = users.ToDictionary(u => u.Id);

        var devices = await _context.Devices.AsNoTracking()
            .Where(d => deviceIds.Contains(d.Id))
            .Select(d => new { d.Id, d.DeviceCode, d.Name })
            .ToListAsync();
        var deviceMap = devices.ToDictionary(d => d.Id);

        var items = rows.Select(r =>
        {
            userMap.TryGetValue(r.UserId ?? 0, out var user);
            deviceMap.TryGetValue(r.DeviceId ?? 0, out var device);
            return new SystemAuditLogDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = user?.Username,
                UserFullName = user?.FullName,
                Content = r.Content,
                Timestamp = r.Timestamp,
                DeviceId = r.DeviceId,
                DeviceCode = r.DeviceId.HasValue
                    ? Helpers.DeviceIdentityHelper.DisplayCode(r.DeviceId.Value, device?.DeviceCode)
                    : device?.DeviceCode,
                DeviceName = device?.Name,
            };
        }).ToList();

        return new PagedResult<SystemAuditLogDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }
}
