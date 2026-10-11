using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public partial class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly IDeviceEventService _deviceEventService;
    private readonly ISystemAuditService _audit;

    public DeviceService(
        AppDbContext context,
        IMapper mapper,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage,
        IDeviceEventService deviceEventService,
        ISystemAuditService audit)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _deviceEventService = deviceEventService;
        _audit = audit;
    }

    public async Task<PagedResult<DeviceListDto>> GetDevicesAsync(DeviceQueryParams query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var queryable = _context.Devices
            .AsNoTracking()
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceStatus)
            .AsQueryable();

        queryable = ApplyOrganizationScope(queryable);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            queryable = queryable.Where(d =>
                (d.Name != null && d.Name.Contains(term)) ||
                (d.DeviceCode != null && d.DeviceCode.Contains(term)));
        }

        if (query.OrganizationId.HasValue)
        {
            queryable = queryable.Where(d => d.OrganizationId == query.OrganizationId);
        }

        if (query.StatusId.HasValue)
        {
            queryable = queryable.Where(d => d.StatusId == query.StatusId);
        }

        if (query.TypeId.HasValue)
        {
            queryable = queryable.Where(d => d.TypeId == query.TypeId);
        }

        if (!string.IsNullOrWhiteSpace(query.BiddingPackageCode))
        {
            var packageCode = query.BiddingPackageCode.Trim();
            queryable = queryable.Where(d => d.BiddingPackageCode == packageCode);
        }

        if (query.ExpiringWithinDays.HasValue && query.ExpiringWithinDays > 0)
        {
            var threshold = DateTime.Today.AddDays(query.ExpiringWithinDays.Value);
            queryable = queryable.Where(d =>
                d.InspectionExpiryDate != null && d.InspectionExpiryDate <= threshold);
        }

        var total = await queryable.CountAsync();

        var sortRows = await queryable
            .Select(d => new { d.Id, d.Name, d.InspectionExpiryDate })
            .ToListAsync();

        var desc = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        IEnumerable<int> orderedIds;
        switch (query.SortBy?.ToLowerInvariant())
        {
            case "name":
                orderedIds = desc
                    ? sortRows.OrderByDescending(x => x.Name).Select(x => x.Id)
                    : sortRows.OrderBy(x => x.Name).Select(x => x.Id);
                break;
            case "inspection":
                orderedIds = desc
                    ? sortRows.OrderByDescending(x => x.InspectionExpiryDate).Select(x => x.Id)
                    : sortRows.OrderBy(x => x.InspectionExpiryDate).Select(x => x.Id);
                break;
            default:
                orderedIds = desc
                    ? sortRows.OrderByDescending(x => x.Id).Select(x => x.Id)
                    : sortRows.OrderBy(x => x.Id).Select(x => x.Id);
                break;
        }

        var pageIds = orderedIds.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        if (pageIds.Count == 0)
        {
            return new PagedResult<DeviceListDto>
            {
                Items = new List<DeviceListDto>(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        var entities = await queryable.Where(d => pageIds.Contains(d.Id)).ToListAsync();
        entities = pageIds.Select(id => entities.First(e => e.Id == id)).ToList();
        var items = _mapper.Map<List<DeviceListDto>>(entities);

        return new PagedResult<DeviceListDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DeviceDetailDto?> GetDeviceByIdAsync(int id)
    {
        var queryable = _context.Devices
            .AsNoTracking()
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceStatus)
            .Include(d => d.Files)
            .Include(d => d.Codes)
            .Where(d => d.Id == id);

        queryable = ApplyOrganizationScope(queryable);
        var device = await queryable.FirstOrDefaultAsync();
        return device is null ? null : _mapper.Map<DeviceDetailDto>(device);
    }

    public async Task<DeviceDetailDto> CreateDeviceAsync(CreateDeviceRequest request)
    {
        var exists = await _context.Devices.AnyAsync(d => d.DeviceCode == request.DeviceCode);
        if (exists)
        {
            throw new InvalidOperationException("Mã thiết bị đã tồn tại.");
        }

        var entity = _mapper.Map<Device>(request);
        _context.Devices.Add(entity);
        await _context.SaveChangesAsync();
        return await GetDeviceByIdAsync(entity.Id)
            ?? throw new InvalidOperationException("Không đọc được thiết bị sau khi tạo.");
    }

    public async Task<DeviceDetailDto?> UpdateDeviceAsync(int id, UpdateDeviceRequest request)
    {
        var queryable = _context.Devices.Where(d => d.Id == id);
        queryable = ApplyOrganizationScope(queryable);
        var entity = await queryable.FirstOrDefaultAsync();
        if (entity is null)
        {
            return null;
        }

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetDeviceByIdAsync(id);
    }

    public async Task<bool> DeleteDeviceAsync(int id)
    {
        var queryable = _context.Devices.Where(d => d.Id == id);
        queryable = ApplyOrganizationScope(queryable);
        var entity = await queryable.FirstOrDefaultAsync();
        if (entity is null)
        {
            return false;
        }

        var files = await _context.DeviceFiles.Where(f => f.DeviceId == id).ToListAsync();
        _context.DeviceFiles.RemoveRange(files);
        var codes = await _context.DeviceCodes.Where(c => c.DeviceId == id).ToListAsync();
        _context.DeviceCodes.RemoveRange(codes);
        _context.Devices.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DeviceListDto>> GetChildrenAsync(int parentId)
    {
        var parent = await GetDeviceByIdAsync(parentId);
        if (parent is null || string.IsNullOrWhiteSpace(parent.DeviceCode))
        {
            return new List<DeviceListDto>();
        }

        var queryable = _context.Devices
            .AsNoTracking()
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceStatus)
            .Where(d => d.ParentDeviceCode == parent.DeviceCode);

        queryable = ApplyOrganizationScope(queryable);
        var list = await queryable.OrderBy(d => d.Id).ToListAsync();
        return _mapper.Map<List<DeviceListDto>>(list);
    }

    public async Task<DeviceFileDto> AddFileAsync(int deviceId, Stream content, string fileName, string? description)
    {
        if (await GetDeviceByIdAsync(deviceId) is null)
        {
            throw new InvalidOperationException("Không tìm thấy thiết bị.");
        }

        var relativePath = await _fileStorage.SaveDeviceFileAsync(deviceId, content, fileName);
        var file = new DeviceFile
        {
            DeviceId = deviceId,
            Path = relativePath,
            Description = description,
            FileDate = DateTime.Today
        };
        _context.DeviceFiles.Add(file);
        await _context.SaveChangesAsync();
        return _mapper.Map<DeviceFileDto>(file);
    }
    public async Task<DeviceFileDto> AddFileWithPathAsync(int deviceId, string relativePath, string? description)
    {
        if (await GetDeviceByIdAsync(deviceId) is null)
            throw new InvalidOperationException("Không tìm thấy thiết bị.");

        var file = new DeviceFile
        {
            DeviceId = deviceId,
            Path = relativePath.Replace('\\', '/'),
            Description = description,
            FileDate = DateTime.Today
        };
        _context.DeviceFiles.Add(file);
        await _context.SaveChangesAsync();
        return _mapper.Map<DeviceFileDto>(file);
    }


    public async Task<string> UploadMaintenanceContractAsync(int deviceId, Stream content, string fileName)
    {
        var queryable = _context.Devices.Where(d => d.Id == deviceId);
        queryable = ApplyOrganizationScope(queryable);
        var entity = await queryable.FirstOrDefaultAsync();
        if (entity is null)
            throw new InvalidOperationException("Không tìm thấy thiết bị.");

        var relativePath = await _fileStorage.SaveMaintenanceContractAsync(deviceId, content, fileName);
        var storedName = Path.GetFileName(relativePath);
        entity.LegalDocumentRef = storedName;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return relativePath;
    }

    private IQueryable<Device> ApplyOrganizationScope(IQueryable<Device> queryable)
    {
        if (!_currentUser.MustFilterByOrganization)
        {
            return queryable;
        }

        var allowed = _currentUser.OrganizationIds;
        if (allowed.Count == 0)
        {
            return queryable.Where(d => false);
        }

        return queryable.Where(d =>
            d.OrganizationId != null && allowed.Contains(d.OrganizationId.Value));
    }
}
