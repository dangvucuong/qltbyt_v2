# -*- coding: utf-8 -*-
"""Generate Phase 2 backend files (UTF-8). Run: python scripts/gen_phase2_rest.py"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
API = ROOT / "QLTBYT.API"


def write(rel: str, content: str) -> None:
    p = API / rel.replace("/", "\\").replace("\\", "/")
    if "\\" in rel:
        p = API / rel
    p = ROOT / "QLTBYT.API" / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content.replace("\r\n", "\n") + "\n", encoding="utf-8", newline="\n")
    print("write", rel)


NOTIFICATIONS_CONTROLLER = r'''using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Notification;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Policy = "AllUsers")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>> GetNotifications()
    {
        var list = await _notificationService.GetForCurrentUserAsync();
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.CreateSuccess(list));
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead(int id)
    {
        await _notificationService.MarkReadAsync(id);
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Da danh dau da doc."));
    }
}
'''

USER_DTOS = r'''namespace QLTBYT.API.Models.DTOs.User;

public class UserListDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? AccountType { get; set; }
    public string? Role { get; set; }
    public string? OrganizationIds { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int AccountType { get; set; }
    public string? OrganizationIds { get; set; }
}

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? AccountType { get; set; }
    public string? OrganizationIds { get; set; }
    public string? NewPassword { get; set; }
}
'''

USER_SERVICE = r'''using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.User;

namespace QLTBYT.API.Services;

public interface IUserAccountService
{
    Task<List<UserListDto>> GetUsersAsync();
    Task<UserListDto?> GetUserAsync(int id);
    Task<UserListDto> CreateUserAsync(CreateUserRequest request);
    Task<UserListDto?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
}

public class UserAccountService : IUserAccountService
{
    private readonly AppDbContext _context;
    private readonly ISystemAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public UserAccountService(AppDbContext context, ISystemAuditService audit, ICurrentUserService currentUser)
    {
        _context = context;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<List<UserListDto>> GetUsersAsync()
    {
        return await _context.UserAccounts
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => Map(u))
            .ToListAsync();
    }

    public async Task<UserListDto?> GetUserAsync(int id)
    {
        var u = await _context.UserAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return u is null ? null : Map(u);
    }

    public async Task<UserListDto> CreateUserAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Ten dang nhap va mat khau la bat buoc.");

        var exists = await _context.UserAccounts.AnyAsync(u => u.Username == request.Username.Trim());
        if (exists)
            throw new InvalidOperationException("Ten dang nhap da ton tai.");

        var entity = new UserAccount
        {
            Username = request.Username.Trim(),
            Password = PasswordHasher.Hash(request.Password),
            FullName = request.FullName?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            AccountType = request.AccountType,
            OrganizationIds = request.OrganizationIds?.Trim(),
            Admin = request.AccountType == 1 ? 1 : 0
        };
        _context.UserAccounts.Add(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Tao tai khoan {entity.Username}");
        return Map(entity);
    }

    public async Task<UserListDto?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var entity = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == id);
        if (entity is null)
            return null;

        if (request.FullName is not null)
            entity.FullName = request.FullName.Trim();
        if (request.Phone is not null)
            entity.Phone = request.Phone.Trim();
        if (request.Email is not null)
            entity.Email = request.Email.Trim();
        if (request.AccountType.HasValue)
        {
            entity.AccountType = request.AccountType.Value;
            entity.Admin = request.AccountType.Value == 1 ? 1 : 0;
        }
        if (request.OrganizationIds is not null)
            entity.OrganizationIds = request.OrganizationIds.Trim();
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
            entity.Password = PasswordHasher.Hash(request.NewPassword);

        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Cap nhat tai khoan ID={id}");
        return Map(entity);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        if (_currentUser.UserId == id)
            throw new InvalidOperationException("Khong the xoa tai khoan dang dang nhap.");

        var entity = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == id);
        if (entity is null)
            return false;

        _context.UserAccounts.Remove(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Xoa tai khoan ID={id}");
        return true;
    }

    private static UserListDto Map(UserAccount u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        Phone = u.Phone,
        Email = u.Email,
        AccountType = u.AccountType,
        Role = RoleMapper.FromAccountType(u.AccountType),
        OrganizationIds = u.OrganizationIds
    };
}
'''

USERS_CONTROLLER = r'''using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.User;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserAccountService _userService;

    public UsersController(IUserAccountService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserListDto>>> GetUsers()
    {
        var list = await _userService.GetUsersAsync();
        return Ok(ApiResponse<IReadOnlyList<UserListDto>>.CreateSuccess(list));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserListDto>>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        if (user is null)
            return NotFound(ApiResponse<UserListDto>.CreateFail("Khong tim thay tai khoan."));
        return Ok(ApiResponse<UserListDto>.CreateSuccess(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserListDto>>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            return Ok(ApiResponse<UserListDto>.CreateSuccess(user, "Tao tai khoan thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserListDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserListDto>>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            if (user is null)
                return NotFound(ApiResponse<UserListDto>.CreateFail("Khong tim thay tai khoan."));
            return Ok(ApiResponse<UserListDto>.CreateSuccess(user, "Cap nhat tai khoan thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserListDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
    {
        try
        {
            var ok = await _userService.DeleteUserAsync(id);
            if (!ok)
                return NotFound(ApiResponse<object>.CreateFail("Khong tim thay tai khoan."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xoa tai khoan thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }
}
'''

PERSONNEL_DTOS = r'''namespace QLTBYT.API.Models.DTOs.Personnel;

public class PersonnelDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? PersonnelTypeId { get; set; }
    public int? UserAccountId { get; set; }
}

public class CreatePersonnelRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? IdentityNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? OrganizationId { get; set; }
    public int? PersonnelTypeId { get; set; }
    public int? UserAccountId { get; set; }
}

public class UpdatePersonnelRequest
{
    public string? FullName { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? OrganizationId { get; set; }
    public int? PersonnelTypeId { get; set; }
    public int? UserAccountId { get; set; }
}
'''

PERSONNEL_SERVICE = r'''using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Personnel;

namespace QLTBYT.API.Services;

public interface IPersonnelService
{
    Task<List<PersonnelDto>> GetAllAsync();
    Task<PersonnelDto?> GetByIdAsync(int id);
    Task<PersonnelDto> CreateAsync(CreatePersonnelRequest request);
    Task<PersonnelDto?> UpdateAsync(int id, UpdatePersonnelRequest request);
    Task<bool> DeleteAsync(int id);
}

public class PersonnelService : IPersonnelService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISystemAuditService _audit;

    public PersonnelService(AppDbContext context, ICurrentUserService currentUser, ISystemAuditService audit)
    {
        _context = context;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<List<PersonnelDto>> GetAllAsync()
    {
        var query = _context.PersonnelRecords.AsNoTracking().AsQueryable();
        if (_currentUser.MustFilterByOrganization)
        {
            var allowed = _currentUser.OrganizationIds;
            query = allowed.Count == 0
                ? query.Where(p => false)
                : query.Where(p => p.OrganizationId != null && allowed.Contains(p.OrganizationId.Value));
        }

        var rows = await query.OrderBy(p => p.FullName).ToListAsync();
        var orgNames = await _context.Organizations.AsNoTracking()
            .ToDictionaryAsync(o => o.Id, o => o.Name);
        return rows.Select(p => Map(p, orgNames)).ToList();
    }

    public async Task<PersonnelDto?> GetByIdAsync(int id)
    {
        var p = await _context.PersonnelRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
            return null;
        if (!IsAllowed(p))
            return null;
        var orgName = p.OrganizationId.HasValue
            ? await _context.Organizations.Where(o => o.Id == p.OrganizationId.Value).Select(o => o.Name).FirstOrDefaultAsync()
            : null;
        return Map(p, orgName is null ? new Dictionary<int, string?>() : new Dictionary<int, string?> { [p.OrganizationId!.Value] = orgName });
    }

    public async Task<PersonnelDto> CreateAsync(CreatePersonnelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new InvalidOperationException("Ho ten la bat buoc.");

        var entity = new Personnel
        {
            FullName = request.FullName.Trim(),
            IdentityNumber = request.IdentityNumber?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            OrganizationId = request.OrganizationId,
            PersonnelTypeId = request.PersonnelTypeId,
            UserAccountId = request.UserAccountId
        };
        if (!IsAllowed(entity))
            throw new InvalidOperationException("Khong co quyen voi don vi nay.");

        _context.PersonnelRecords.Add(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Them nhan su {entity.FullName}");
        return await GetByIdAsync(entity.Id) ?? Map(entity, new Dictionary<int, string?>());
    }

    public async Task<PersonnelDto?> UpdateAsync(int id, UpdatePersonnelRequest request)
    {
        var entity = await _context.PersonnelRecords.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null || !IsAllowed(entity))
            return null;

        if (request.FullName is not null)
            entity.FullName = request.FullName.Trim();
        if (request.IdentityNumber is not null)
            entity.IdentityNumber = request.IdentityNumber.Trim();
        if (request.Phone is not null)
            entity.Phone = request.Phone.Trim();
        if (request.Email is not null)
            entity.Email = request.Email.Trim();
        if (request.OrganizationId.HasValue)
            entity.OrganizationId = request.OrganizationId;
        if (request.PersonnelTypeId.HasValue)
            entity.PersonnelTypeId = request.PersonnelTypeId;
        if (request.UserAccountId.HasValue)
            entity.UserAccountId = request.UserAccountId;

        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Cap nhat nhan su ID={id}");
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.PersonnelRecords.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null || !IsAllowed(entity))
            return false;
        _context.PersonnelRecords.Remove(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Xoa nhan su ID={id}");
        return true;
    }

    private bool IsAllowed(Personnel p)
    {
        if (!_currentUser.MustFilterByOrganization)
            return true;
        if (p.OrganizationId is null)
            return false;
        return _currentUser.OrganizationIds.Contains(p.OrganizationId.Value);
    }

    private static PersonnelDto Map(Personnel p, Dictionary<int, string?> orgNames) => new()
    {
        Id = p.Id,
        FullName = p.FullName,
        IdentityNumber = p.IdentityNumber,
        Phone = p.Phone,
        Email = p.Email,
        OrganizationId = p.OrganizationId,
        OrganizationName = p.OrganizationId.HasValue ? orgNames.GetValueOrDefault(p.OrganizationId.Value) : null,
        PersonnelTypeId = p.PersonnelTypeId,
        UserAccountId = p.UserAccountId
    };
}
'''

PERSONNEL_CONTROLLER = r'''using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Personnel;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/personnel")]
[Authorize(Policy = "TechnicianAndAbove")]
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelService _personnelService;

    public PersonnelController(IPersonnelService personnelService)
    {
        _personnelService = personnelService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PersonnelDto>>> GetAll()
    {
        var list = await _personnelService.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<PersonnelDto>>.CreateSuccess(list));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PersonnelDto>>> GetById(int id)
    {
        var item = await _personnelService.GetByIdAsync(id);
        if (item is null)
            return NotFound(ApiResponse<PersonnelDto>.CreateFail("Khong tim thay nhan su."));
        return Ok(ApiResponse<PersonnelDto>.CreateSuccess(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PersonnelDto>>> Create([FromBody] CreatePersonnelRequest request)
    {
        try
        {
            var item = await _personnelService.CreateAsync(request);
            return Ok(ApiResponse<PersonnelDto>.CreateSuccess(item, "Them nhan su thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PersonnelDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PersonnelDto>>> Update(int id, [FromBody] UpdatePersonnelRequest request)
    {
        var item = await _personnelService.UpdateAsync(id, request);
        if (item is null)
            return NotFound(ApiResponse<PersonnelDto>.CreateFail("Khong tim thay nhan su."));
        return Ok(ApiResponse<PersonnelDto>.CreateSuccess(item, "Cap nhat nhan su thanh cong."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var ok = await _personnelService.DeleteAsync(id);
        if (!ok)
            return NotFound(ApiResponse<object>.CreateFail("Khong tim thay nhan su."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xoa nhan su thanh cong."));
    }
}
'''

USAGE_DTOS = r'''namespace QLTBYT.API.Models.DTOs.UsageLog;

public class UsageLogDto
{
    public int Id { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public int? Hours { get; set; }
    public int? Sessions { get; set; }
    public DateTime InputDate { get; set; }
    public int? UserId { get; set; }
}

public class CreateUsageLogRequest
{
    public string DeviceCode { get; set; } = string.Empty;
    public int? Hours { get; set; }
    public int? Sessions { get; set; }
    public DateTime InputDate { get; set; }
}

public class UsageLogQueryParams
{
    public string? DeviceCode { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
'''

USAGE_SERVICE = r'''using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.UsageLog;

namespace QLTBYT.API.Services;

public interface IUsageLogService
{
    Task<List<UsageLogDto>> GetLogsAsync(UsageLogQueryParams query);
    Task<UsageLogDto> CreateAsync(CreateUsageLogRequest request);
}

public class UsageLogService : IUsageLogService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISystemAuditService _audit;

    public UsageLogService(AppDbContext context, ICurrentUserService currentUser, ISystemAuditService audit)
    {
        _context = context;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<List<UsageLogDto>> GetLogsAsync(UsageLogQueryParams query)
    {
        var q = _context.UsageLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.DeviceCode))
        {
            var code = query.DeviceCode.Trim();
            q = q.Where(l => l.DeviceCode == code);
            await EnsureDeviceScopeAsync(code);
        }
        else if (_currentUser.MustFilterByOrganization)
        {
            var codes = await ScopedDeviceCodesAsync();
            q = q.Where(l => codes.Contains(l.DeviceCode));
        }

        if (query.FromDate.HasValue)
            q = q.Where(l => l.InputDate >= query.FromDate.Value);
        if (query.ToDate.HasValue)
            q = q.Where(l => l.InputDate <= query.ToDate.Value);

        var rows = await q.OrderByDescending(l => l.InputDate).Take(200).ToListAsync();
        return rows.Select(Map).ToList();
    }

    public async Task<UsageLogDto> CreateAsync(CreateUsageLogRequest request)
    {
        var code = request.DeviceCode?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException("Ma thiet bi la bat buoc.");

        await EnsureDeviceScopeAsync(code);

        var entity = new UsageLog
        {
            DeviceCode = code,
            Hours = request.Hours,
            Sessions = request.Sessions,
            InputDate = request.InputDate,
            UserId = _currentUser.UserId
        };
        _context.UsageLogs.Add(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Nhat ky su dung {code}", null);
        return Map(entity);
    }

    private async Task EnsureDeviceScopeAsync(string deviceCode)
    {
        var device = await _context.Devices.AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeviceCode == deviceCode);
        if (device is null)
            throw new InvalidOperationException("Khong tim thay thiet bi.");

        if (_currentUser.MustFilterByOrganization)
        {
            var allowed = _currentUser.OrganizationIds;
            if (device.OrganizationId is null || !allowed.Contains(device.OrganizationId.Value))
                throw new InvalidOperationException("Khong co quyen voi thiet bi nay.");
        }
    }

    private async Task<List<string>> ScopedDeviceCodesAsync()
    {
        var allowed = _currentUser.OrganizationIds;
        if (allowed.Count == 0)
            return new List<string>();
        return await _context.Devices.AsNoTracking()
            .Where(d => d.OrganizationId != null && allowed.Contains(d.OrganizationId.Value) && d.DeviceCode != null)
            .Select(d => d.DeviceCode!)
            .ToListAsync();
    }

    private static UsageLogDto Map(UsageLog l) => new()
    {
        Id = l.Id,
        DeviceCode = l.DeviceCode,
        Hours = l.Hours,
        Sessions = l.Sessions,
        InputDate = l.InputDate,
        UserId = l.UserId
    };
}
'''

USAGE_CONTROLLER = r'''using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.UsageLog;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/usage-logs")]
[Authorize(Policy = "AllUsers")]
public class UsageLogsController : ControllerBase
{
    private readonly IUsageLogService _usageLogService;

    public UsageLogsController(IUsageLogService usageLogService)
    {
        _usageLogService = usageLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UsageLogDto>>> GetLogs([FromQuery] UsageLogQueryParams query)
    {
        var list = await _usageLogService.GetLogsAsync(query);
        return Ok(ApiResponse<IReadOnlyList<UsageLogDto>>.CreateSuccess(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UsageLogDto>>> Create([FromBody] CreateUsageLogRequest request)
    {
        try
        {
            var item = await _usageLogService.CreateAsync(request);
            return Ok(ApiResponse<UsageLogDto>.CreateSuccess(item, "Ghi nhat ky thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UsageLogDto>.CreateFail(ex.Message));
        }
    }
}
'''

EVENT_KEYWORD_DTO = r'''namespace QLTBYT.API.Models.DTOs.Event;

public class EventKeywordDto
{
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
}

public class CreateEventKeywordRequest
{
    public string Keyword { get; set; } = string.Empty;
}
'''

INCIDENT_ALERT = r'''namespace QLTBYT.API.Services;

public interface IIncidentAlertService
{
    Task SendIncidentCreatedAsync(int deviceId, string deviceCode, string content);
    Task SendIncidentResolvedAsync(int eventId, string deviceCode);
}

public class DevelopmentIncidentAlertService : IIncidentAlertService
{
    public Task SendIncidentCreatedAsync(int deviceId, string deviceCode, string content) => Task.CompletedTask;
    public Task SendIncidentResolvedAsync(int eventId, string deviceCode) => Task.CompletedTask;
}
'''

EXPIRY_JOB = r'''using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QLTBYT.API.Data;

namespace QLTBYT.API.Infrastructure;

public class DeviceExpiryNotificationJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DeviceExpiryNotificationJob> _logger;

    public DeviceExpiryNotificationJob(IServiceProvider services, ILogger<DeviceExpiryNotificationJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(6);
            if (now > nextRun)
                nextRun = nextRun.AddDays(1);
            var delay = nextRun - now;
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                await RunCheckAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Device expiry notification job failed");
            }
        }
    }

    private async Task RunCheckAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var threshold = DateTime.Now.AddDays(30);
        var expiring = await context.Devices.AsNoTracking()
            .Where(d => d.InspectionExpiryDate != null && d.InspectionExpiryDate <= threshold)
            .Select(d => new { d.Id, d.DeviceCode, d.InspectionExpiryDate })
            .Take(50)
            .ToListAsync(cancellationToken);

        if (expiring.Count == 0)
            return;

        var adminIds = await context.UserAccounts.AsNoTracking()
            .Where(u => u.AccountType == 1 || u.AccountType == 2)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (adminIds.Count == 0)
            return;

        var summary = string.Join(", ", expiring.Select(e => $"{e.DeviceCode} ({e.InspectionExpiryDate:dd/MM/yyyy})"));
        await notifications.NotifyUsersAsync(
            "Thiet bi sap het han kiem dinh",
            summary,
            adminIds.First(),
            adminIds);
        _logger.LogInformation("Expiry notifications sent for {Count} devices", expiring.Count);
    }
}
'''

def patch_interface_device_event():
    p = API / "Services/IDeviceEventService.cs"
    t = p.read_text(encoding="utf-8")
    if "GetKeywordsAsync" in t:
        return
    insert = (
        "    Task<List<EventKeywordDto>> GetKeywordsAsync();\n"
        "    Task<EventKeywordDto> AddKeywordAsync(CreateEventKeywordRequest request);\n"
    )
    t = t.replace(
        "    Task<EventDetailDto?> ResolveAsync(int id, ResolveEventRequest request);\n",
        "    Task<EventDetailDto?> ResolveAsync(int id, ResolveEventRequest request);\n" + insert,
    )
    p.write_text(t, encoding="utf-8", newline="\n")
    print("patch IDeviceEventService")


def patch_device_event_service():
    p = API / "Services/DeviceEventService.cs"
    t = p.read_text(encoding="utf-8")
    if "GetKeywordsAsync" in t:
        return
    methods = r'''
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
            throw new InvalidOperationException("Keyword la bat buoc.");
        var entity = new EventKeyword { Keyword = kw };
        _context.EventKeywords.Add(entity);
        await _context.SaveChangesAsync();
        return new EventKeywordDto { Id = entity.Id, Keyword = kw };
    }
'''
    t = t.replace("\n    private IQueryable<DeviceEvent> ApplyDeviceScope", methods + "\n    private IQueryable<DeviceEvent> ApplyDeviceScope")
    p.write_text(t, encoding="utf-8", newline="\n")
    print("patch DeviceEventService")


def patch_events_controller():
    p = API / "Controllers/EventsController.cs"
    t = p.read_text(encoding="utf-8")
    if "keywords" in t:
        return
    block = r'''
    [HttpGet("keywords")]
  [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventKeywordDto>>> GetKeywords()
    {
        var list = await _eventService.GetKeywordsAsync();
        return Ok(ApiResponse<IReadOnlyList<EventKeywordDto>>.CreateSuccess(list));
    }

    [HttpPost("keywords")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<EventKeywordDto>>> AddKeyword([FromBody] CreateEventKeywordRequest request)
    {
        try
        {
            var dto = await _eventService.AddKeywordAsync(request);
            return Ok(ApiResponse<EventKeywordDto>.CreateSuccess(dto, "Them keyword thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EventKeywordDto>.CreateFail(ex.Message));
        }
    }

'''
    t = t.replace("    [HttpPut(\"{id:int}\")]", block + "    [HttpPut(\"{id:int}\")]")
    p.write_text(t, encoding="utf-8", newline="\n")
    print("patch EventsController")


def patch_program():
    p = API / "Program.cs"
    t = p.read_text(encoding="utf-8")
    if "IUserAccountService" not in t:
        t = t.replace(
            "builder.Services.AddScoped<INotificationService, NotificationService>();",
            "builder.Services.AddScoped<INotificationService, NotificationService>();\n"
            "builder.Services.AddScoped<IUserAccountService, UserAccountService>();\n"
            "builder.Services.AddScoped<IPersonnelService, PersonnelService>();\n"
            "builder.Services.AddScoped<IUsageLogService, UsageLogService>();\n"
            "builder.Services.AddSingleton<IIncidentAlertService, DevelopmentIncidentAlertService>();\n"
            "builder.Services.AddHostedService<DeviceExpiryNotificationJob>();",
        )
    p.write_text(t, encoding="utf-8", newline="\n")
    print("patch Program.cs")


def write_tests():
    tests_dir = ROOT / "QLTBYT.API.Tests" / "Phase2"
    tests_dir.mkdir(parents=True, exist_ok=True)
    content = r'''using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Event;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase2;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class Phase2EventsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Phase2EventsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionStrings.Resolve());
            builder.UseSetting("JwtSettings:SecretKey", "QLTBYT-TEST-Secret-Key-At-Least-32-Characters");
            builder.UseSetting("JwtSettings:Issuer", "QLTBYT-API");
            builder.UseSetting("JwtSettings:Audience", "QLTBYT-Frontend");
            builder.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.EnvironmentKey, "Development");
        });
    }

    private async Task<HttpClient> CreateClientAsync(string username)
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = username,
            Password = "Sample@123"
        });
        var body = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Data!.Token);
        return client;
    }

    [Fact]
    public async Task User_Can_Create_Event_And_Technician_Can_Resolve()
    {
        var userClient = await CreateClientAsync("user");
        var devices = await userClient.GetFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>(
            "/api/devices?search=TB-DEMO-002");
        var deviceId = devices!.Data!.Items.First().Id;

        var create = await userClient.PostAsJsonAsync($"/api/devices/{deviceId}/events", new CreateDeviceEventRequest
        {
            EventTypeId = 1,
            Content = "Test su co phase 2",
            Keyword = "Bao hong"
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ApiResponse<EventDetailDto>>();
        Assert.True(created!.Success);
        var eventId = created.Data!.Id;

        var techClient = await CreateClientAsync("kythuat");
        var list = await techClient.GetFromJsonAsync<ApiResponse<PagedResult<EventListDto>>>("/api/events?statusId=1");
        Assert.True(list!.Data!.TotalCount >= 1);

        var resolve = await techClient.PutAsJsonAsync($"/api/events/{eventId}", new ResolveEventRequest
        {
            ResolutionContent = "Da xu ly test",
            Cost = 0
        });
        Assert.Equal(HttpStatusCode.OK, resolve.StatusCode);
    }
}
'''
    (tests_dir / "Phase2EventsIntegrationTests.cs").write_text(content, encoding="utf-8", newline="\n")
    print("write Phase2 tests")


def main():
    write("Controllers/NotificationsController.cs", NOTIFICATIONS_CONTROLLER)
    write("Models/DTOs/User/UserDtos.cs", USER_DTOS)
    write("Services/UserAccountService.cs", USER_SERVICE)
    write("Controllers/UsersController.cs", USERS_CONTROLLER)
    write("Models/DTOs/Personnel/PersonnelDtos.cs", PERSONNEL_DTOS)
    write("Services/PersonnelService.cs", PERSONNEL_SERVICE)
    write("Controllers/PersonnelController.cs", PERSONNEL_CONTROLLER)
    write("Models/DTOs/UsageLog/UsageLogDtos.cs", USAGE_DTOS)
    write("Services/UsageLogService.cs", USAGE_SERVICE)
    write("Controllers/UsageLogsController.cs", USAGE_CONTROLLER)
    write("Models/DTOs/Event/EventKeywordDto.cs", EVENT_KEYWORD_DTO)
    write("Services/IncidentAlertService.cs", INCIDENT_ALERT)
    write("Infrastructure/DeviceExpiryNotificationJob.cs", EXPIRY_JOB)
    patch_interface_device_event()
    patch_device_event_service()
    patch_events_controller()
    patch_program()
    write_tests()


if __name__ == "__main__":
    main()
