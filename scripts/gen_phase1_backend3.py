# -*- coding: utf-8 -*-
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def w(rel, content):
    p = ROOT / rel.replace("/", "\\")
    p.write_text(content.strip() + "\n", encoding="utf-8", newline="\n")
    print("ok", rel)

device_service = r'''
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public DeviceService(
        AppDbContext context,
        IMapper mapper,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
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
            throw new InvalidOperationException("Ma thiet bi da ton tai.");
        }

        var entity = _mapper.Map<Device>(request);
        _context.Devices.Add(entity);
        await _context.SaveChangesAsync();
        return await GetDeviceByIdAsync(entity.Id)
            ?? throw new InvalidOperationException("Khong the doc thiet bi sau khi tao.");
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
            throw new InvalidOperationException("Khong tim thay thiet bi.");
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
'''

devices_controller = r'''
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize(Policy = "AllUsers")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DeviceListDto>>>> GetDevices(
        [FromQuery] DeviceQueryParams queryParams)
    {
        var result = await _deviceService.GetDevicesAsync(queryParams);
        return Ok(ApiResponse<PagedResult<DeviceListDto>>.CreateSuccess(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> GetDevice(int id)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device is null)
        {
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Khong tim thay thiet bi."));
        }

        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(device));
    }

    [HttpGet("{id:int}/children")]
    public async Task<ActionResult<ApiResponse<List<DeviceListDto>>>> GetChildren(int id)
    {
        var children = await _deviceService.GetChildrenAsync(id);
        return Ok(ApiResponse<List<DeviceListDto>>.CreateSuccess(children));
    }

    [HttpPost]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> CreateDevice([FromBody] CreateDeviceRequest request)
    {
        try
        {
            var device = await _deviceService.CreateDeviceAsync(request);
            return CreatedAtAction(nameof(GetDevice), new { id = device.Id },
                ApiResponse<DeviceDetailDto>.CreateSuccess(device, "Them thiet bi thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DeviceDetailDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AllUsers")]
    public async Task<ActionResult<ApiResponse<DeviceDetailDto>>> UpdateDevice(int id, [FromBody] UpdateDeviceRequest request)
    {
        var device = await _deviceService.UpdateDeviceAsync(id, request);
        if (device is null)
        {
            return NotFound(ApiResponse<DeviceDetailDto>.CreateFail("Khong tim thay thiet bi."));
        }

        return Ok(ApiResponse<DeviceDetailDto>.CreateSuccess(device, "Cap nhat thiet bi thanh cong."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDevice(int id)
    {
        var ok = await _deviceService.DeleteDeviceAsync(id);
        if (!ok)
        {
            return NotFound(ApiResponse<object>.CreateFail("Khong tim thay thiet bi."));
        }

        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xoa thiet bi thanh cong."));
    }

    [HttpPost("{id:int}/files")]
    [Authorize(Policy = "TechnicianAndAbove")]
  [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<DeviceFileDto>>> UploadFile(
        int id,
        IFormFile file,
        [FromForm] string? description)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail("File khong hop le."));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var dto = await _deviceService.AddFileAsync(id, stream, file.FileName, description);
            return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Upload file thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<DeviceFileDto>.CreateFail(ex.Message));
        }
    }
}
'''

w("QLTBYT.API/Services/DeviceService.cs", device_service)
w("QLTBYT.API/Controllers/DevicesController.cs", devices_controller)
print("batch3 done")
