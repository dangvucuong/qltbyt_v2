from pathlib import Path
Path(__file__).resolve().parent  # anchor
content = open(__file__, "r", encoding="utf-8").read().split("'''SERVICE'''")[1].split("'''")[0]
Path(r"d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API/Services/DeviceEventService.cs").write_text(
    content, encoding="utf-8", newline="\n")
print("ok")

'''SERVICE'''
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Event;

namespace QLTBYT.API.Services;

public class DeviceEventService : IDeviceEventService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISystemAuditService _audit;
    private readonly INotificationService _notifications;

    public DeviceEventService(
        AppDbContext context,
        ICurrentUserService currentUser,
        ISystemAuditService audit,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<PagedResult<EventListDto>> GetEventsAsync(EventQueryParams query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var queryable = _context.DeviceEvents
            .AsNoTracking()
            .Include(e => e.Device)
            .AsQueryable();

        queryable = ApplyDeviceScope(queryable);

        if (query.StatusId.HasValue)
            queryable = queryable.Where(e => e.StatusId == query.StatusId.Value);

        if (query.DeviceId.HasValue)
            queryable = queryable.Where(e => e.DeviceId == query.DeviceId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            queryable = queryable.Where(e =>
                (e.Content != null && e.Content.Contains(term)) ||
                (e.Device != null && e.Device.DeviceCode != null && e.Device.DeviceCode.Contains(term)) ||
                (e.Device != null && e.Device.Name != null && e.Device.Name.Contains(term)));
        }

        var list = await queryable
            .OrderByDescending(e => e.EventDateTime)
            .ThenByDescending(e => e.Id)
            .ToListAsync();

        var total = list.Count;
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).Select(MapList).ToList();

        return new PagedResult<EventListDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EventDetailDto?> GetEventByIdAsync(int id)
    {
        var entity = await _context.DeviceEvents.AsNoTracking().Include(e => e.Device)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null || !IsDeviceAllowed(entity.Device))
            return null;
        return await MapDetailAsync(entity);
    }

    public async Task<EventDetailDto> CreateForDeviceAsync(int deviceId, CreateDeviceEventRequest request)
    {
        var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceId);
        if (device is null || !IsDeviceAllowed(device))
            throw new InvalidOperationException("Kh\u00f4ng t\u00ecm th\u1ea5y thi\u1ebft b\u1ecb ho\u1eb7c kh\u00f4ng c\u00f3 quy\u1ec1n.");

        var entity = new DeviceEvent
        {
            DeviceId = deviceId,
            TypeId = request.EventTypeId,
            Content = request.Content.Trim(),
            Keyword = request.Keyword?.Trim(),
            EventDateTime = DateTime.Now,
            StatusId = 1,
            AccountUsername = _currentUser.UserId.HasValue
                ? await _context.UserAccounts.Where(u => u.Id == _currentUser.UserId).Select(u => u.Username).FirstOrDefaultAsync()
                : null
        };

        _context.DeviceEvents.Add(entity);
        await _context.SaveChangesAsync();

        await _audit.WriteAsync(_currentUser.UserId,
            $"T\u1ea1o y\u00eau c\u1ea7u/s\u1ef1 c\u1ed1 thi\u1ebft b\u1ecb ID={deviceId}", deviceId);

        if (_currentUser.UserId.HasValue)
        {
            var notifyUserIds = await _context.UserAccounts
                .Where(u => u.AccountType == 1 || u.AccountType == 2)
                .Select(u => u.Id).ToListAsync();
            await _notifications.NotifyUsersAsync(
                "Y\u00eau c\u1ea7u thi\u1ebft b\u1ecb m\u1edbi",
                request.Content.Trim(),
                _currentUser.UserId.Value,
                notifyUserIds);
        }

        entity.Device = device;
        return await MapDetailAsync(entity);
    }

    public async Task<EventDetailDto?> ResolveAsync(int id, ResolveEventRequest request)
    {
        var entity = await _context.DeviceEvents.Include(e => e.Device).FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null || !IsDeviceAllowed(entity.Device))
            return null;

        entity.StatusId = 2;
        entity.Content = request.ResolutionContent.Trim();
        entity.Cost = request.Cost;
        entity.ManagerUserId = _currentUser.UserId;
        entity.EventDateTime = DateTime.Now;
        await _context.SaveChangesAsync();

        await _audit.WriteAsync(_currentUser.UserId, $"X\u1eed l\u00fd s\u1ef1 c\u1ed1 ID={id}", entity.DeviceId);
        return await MapDetailAsync(entity);
    }

    private IQueryable<DeviceEvent> ApplyDeviceScope(IQueryable<DeviceEvent> queryable)
    {
        if (!_currentUser.MustFilterByOrganization)
            return queryable;
        var allowed = _currentUser.OrganizationIds;
        if (allowed.Count == 0)
            return queryable.Where(e => false);
        return queryable.Where(e =>
            e.Device != null && e.Device.OrganizationId != null && allowed.Contains(e.Device.OrganizationId.Value));
    }

    private bool IsDeviceAllowed(Device? device)
    {
        if (device is null) return false;
        if (!_currentUser.MustFilterByOrganization) return true;
        var allowed = _currentUser.OrganizationIds;
        return device.OrganizationId != null && allowed.Contains(device.OrganizationId.Value);
    }

    private static EventListDto MapList(DeviceEvent e) => new()
    {
        Id = e.Id,
        DeviceId = e.DeviceId,
        DeviceCode = e.Device?.DeviceCode,
        DeviceName = e.Device?.Name,
        TypeId = e.TypeId,
        TypeName = DeviceEventLabels.GetTypeName(e.TypeId),
        Content = e.Content,
        EventDateTime = e.EventDateTime,
        StatusId = e.StatusId,
        StatusName = DeviceEventLabels.GetStatusName(e.StatusId),
        Keyword = e.Keyword,
        Cost = e.Cost,
        AccountUsername = e.AccountUsername
    };

    private async Task<EventDetailDto> MapDetailAsync(DeviceEvent e)
    {
        var dto = new EventDetailDto
        {
            Id = e.Id,
            DeviceId = e.DeviceId,
            DeviceCode = e.Device?.DeviceCode,
            DeviceName = e.Device?.Name,
            TypeId = e.TypeId,
            TypeName = DeviceEventLabels.GetTypeName(e.TypeId),
            Content = e.Content,
            EventDateTime = e.EventDateTime,
            StatusId = e.StatusId,
            StatusName = DeviceEventLabels.GetStatusName(e.StatusId),
            Keyword = e.Keyword,
            Cost = e.Cost,
            AccountUsername = e.AccountUsername,
            ManagerUserId = e.ManagerUserId
        };
        if (e.ManagerUserId.HasValue)
            dto.ManagerName = await _context.UserAccounts.Where(u => u.Id == e.ManagerUserId.Value).Select(u => u.FullName).FirstOrDefaultAsync();
        return dto;
    }
}
'''
