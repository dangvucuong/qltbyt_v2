using Microsoft.EntityFrameworkCore;
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
            throw new InvalidOperationException("Họ tên là bắt buộc.");

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
            throw new InvalidOperationException("Không có quyền với đơn vị này.");

        _context.PersonnelRecords.Add(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Thêm nhân sự {entity.FullName}");
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
        await _audit.WriteAsync(_currentUser.UserId, $"Cập nhật nhân sự ID={id}");
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.PersonnelRecords.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null || !IsAllowed(entity))
            return false;
        _context.PersonnelRecords.Remove(entity);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(_currentUser.UserId, $"Xóa nhân sự ID={id}");
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

