using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Event;

namespace QLTBYT.API.Services;

public class DeviceEventService : IDeviceEventService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISystemAuditService _audit;
    private readonly INotificationService _notifications;
    private readonly IIncidentAlertService _incidentAlerts;
    private readonly IFileStorageService _fileStorage;

    public DeviceEventService(
        AppDbContext context,
        ICurrentUserService currentUser,
        ISystemAuditService audit,
        INotificationService notifications,
        IIncidentAlertService incidentAlerts,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _audit = audit;
        _notifications = notifications;
        _incidentAlerts = incidentAlerts;
        _fileStorage = fileStorage;
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
            var idTerm = int.TryParse(term, out var parsedId) ? parsedId : (int?)null;
            queryable = queryable.Where(e =>
                (idTerm.HasValue && e.DeviceId == idTerm.Value) ||
                (e.Content != null && e.Content.Contains(term)) ||
                (e.Device != null && e.Device.DeviceCode != null && e.Device.DeviceCode.Contains(term)) ||
                (e.Device != null && e.Device.IdentityCode != null && e.Device.IdentityCode.Contains(term)) ||
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
                .Where(u => u.Admin == 1 || u.AccountType == 0 || u.AccountType == 1 || u.AccountType == 2)
                .Select(u => u.Id).ToListAsync();
            await _notifications.NotifyUsersAsync(
                "Y\u00eau c\u1ea7u thi\u1ebft b\u1ecb m\u1edfi",
                request.Content.Trim(),
                _currentUser.UserId.Value,
                notifyUserIds);
        }

        await _incidentAlerts.SendIncidentCreatedAsync(
            deviceId,
            device.DeviceCode ?? deviceId.ToString(),
            request.Content.Trim());

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

        var code = entity.Device?.DeviceCode ?? entity.DeviceId?.ToString() ?? "";
        await _incidentAlerts.SendIncidentResolvedAsync(id, code);

        return await MapDetailAsync(entity);
    }

    public async Task<List<EventKeywordDto>> GetKeywordsAsync()
    {
        return await _context.EventKeywords.AsNoTracking()
            .OrderBy(k => k.Keyword)
            .Select(k => new EventKeywordDto { Id = k.Id, Keyword = k.Keyword.Trim() })
            .ToListAsync();
    }

    public async Task<EventKeywordDto> AddKeywordAsync(CreateEventKeywordRequest request)
    {
        var kw = request.Keyword?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(kw))
            throw new InvalidOperationException("Từ khóa là bắt buộc.");
        var entity = new EventKeyword { Keyword = kw };
        _context.EventKeywords.Add(entity);
        await _context.SaveChangesAsync();
        return new EventKeywordDto { Id = entity.Id, Keyword = kw };
    }

    public async Task<DeviceFileDto> AddIncidentFileForEventAsync(int eventId, Stream content, string fileName, string kind)
    {
        var entity = await _context.DeviceEvents.Include(e => e.Device).FirstOrDefaultAsync(e => e.Id == eventId);
        if (entity is null || entity.DeviceId is null || !IsDeviceAllowed(entity.Device))
            throw new InvalidOperationException("Không tìm thấy sự cố hoặc không có quyền.");

        return await SaveIncidentFileAsync(entity.DeviceId.Value, content, fileName, kind);
    }

    public async Task<List<DeviceFileDto>> GetIncidentFilesForEventAsync(int eventId)
    {
        var entity = await _context.DeviceEvents.AsNoTracking().Include(e => e.Device)
            .FirstOrDefaultAsync(e => e.Id == eventId);
        if (entity is null || entity.DeviceId is null || !IsDeviceAllowed(entity.Device))
            return new List<DeviceFileDto>();

        return await GetIncidentFilesForDeviceAsync(entity.DeviceId.Value, null);
    }

    public async Task<DeviceFileDto> AddIncidentFileForDeviceAsync(int deviceId, Stream content, string fileName, string kind)
    {
        var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceId);
        if (device is null || !IsDeviceAllowed(device))
            throw new InvalidOperationException("Không tìm thấy thiết bị hoặc không có quyền.");

        return await SaveIncidentFileAsync(deviceId, content, fileName, kind);
    }

    public async Task<List<DeviceFileDto>> GetIncidentFilesForDeviceAsync(int deviceId, string? kind)
    {
        var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceId);
        if (device is null || !IsDeviceAllowed(device))
            return new List<DeviceFileDto>();

        var folder = ResolveIncidentFolder(kind);
        var query = _context.DeviceFiles.AsNoTracking().Where(f => f.DeviceId == deviceId);
        if (folder is not null)
            query = query.Where(f => f.Path != null && f.Path.Contains(folder));
        else
            query = query.Where(f =>
                f.Path != null &&
                (f.Path.Contains("Hinh_anh_su_co") || f.Path.Contains("BAO_CAO_SU_CO")));

        return await query
            .OrderByDescending(f => f.FileDate)
            .ThenByDescending(f => f.Id)
            .Select(f => new DeviceFileDto
            {
                Id = f.Id,
                Path = f.Path,
                Description = f.Description,
                FileDate = f.FileDate
            })
            .ToListAsync();
    }

    private async Task<DeviceFileDto> SaveIncidentFileAsync(int deviceId, Stream content, string fileName, string kind)
    {
        var folder = ResolveIncidentFolder(kind)
            ?? throw new InvalidOperationException("Loại file phải là image hoặc report.");
        var description = folder == "Hinh_anh_su_co" ? "Hình ảnh sự cố" : "Báo cáo sự cố";

        var relativePath = await _fileStorage.SaveIncidentFileAsync(deviceId, folder, content, fileName);
        var file = new DeviceFile
        {
            DeviceId = deviceId,
            Path = relativePath,
            Description = description,
            FileDate = DateTime.Now
        };
        _context.DeviceFiles.Add(file);
        await _context.SaveChangesAsync();

        await _audit.WriteAsync(_currentUser.UserId,
            $"Upload file sự cố ({description}) thiết bị ID={deviceId}", deviceId);

        return new DeviceFileDto
        {
            Id = file.Id,
            Path = file.Path,
            Description = file.Description,
            FileDate = file.FileDate
        };
    }

    private static string? ResolveIncidentFolder(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind)) return null;
        return kind.Trim().ToLowerInvariant() switch
        {
            "image" or "hinh_anh_su_co" or "incident-image" => "Hinh_anh_su_co",
            "report" or "bao_cao_su_co" or "incident-report" => "BAO_CAO_SU_CO",
            _ => null
        };
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
        DeviceCode = e.DeviceId.HasValue
            ? DeviceIdentityHelper.DisplayCode(e.DeviceId.Value, e.Device?.DeviceCode)
            : e.Device?.DeviceCode,
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
            DeviceCode = e.DeviceId.HasValue
                ? DeviceIdentityHelper.DisplayCode(e.DeviceId.Value, e.Device?.DeviceCode)
                : e.Device?.DeviceCode,
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
