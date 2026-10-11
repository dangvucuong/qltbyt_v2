using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Master;

namespace QLTBYT.API.Services;

public interface IAdminMasterDataService
{
    Task<MasterItemDto> CreateDeviceTypeAsync(string name);
    Task<MasterItemDto?> UpdateDeviceTypeAsync(int id, string name);
    Task<bool> DeleteDeviceTypeAsync(int id);

    Task<MasterItemDto> CreateDeviceStatusAsync(string name);
    Task<MasterItemDto?> UpdateDeviceStatusAsync(int id, string name);
    Task<bool> DeleteDeviceStatusAsync(int id);

    Task<MasterItemDto> CreateOrganizationTypeAsync(string name);
    Task<MasterItemDto?> UpdateOrganizationTypeAsync(int id, string name);
    Task<bool> DeleteOrganizationTypeAsync(int id);

    Task<MasterItemDto> CreateCountryAsync(string name);
    Task<MasterItemDto?> UpdateCountryAsync(int id, string name);
    Task<bool> DeleteCountryAsync(int id);

    Task<MasterItemDto> CreateTaskTypeAsync(string name);
    Task<MasterItemDto?> UpdateTaskTypeAsync(int id, string name);
    Task<bool> DeleteTaskTypeAsync(int id);

    Task<MasterItemDto> CreateEventKeywordAsync(string name);
    Task<MasterItemDto?> UpdateEventKeywordAsync(int id, string name);
    Task<bool> DeleteEventKeywordAsync(int id);

    Task<MasterItemDto> CreatePersonnelTypeAsync(string name);
    Task<MasterItemDto?> UpdatePersonnelTypeAsync(int id, string name);
    Task<bool> DeletePersonnelTypeAsync(int id);

    Task<MasterItemDto> CreateDocumentTypeAsync(string name);
    Task<MasterItemDto?> UpdateDocumentTypeAsync(int id, string name);
    Task<bool> DeleteDocumentTypeAsync(int id);
}

public class AdminMasterDataService : IAdminMasterDataService
{
    private readonly AppDbContext _context;

    public AdminMasterDataService(AppDbContext context)
    {
        _context = context;
    }

    private static string RequireName(string? name, string label)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException($"{label} là bắt buộc.");
        return trimmed;
    }

    private static MasterItemDto Map(int id, string? name) => new() { Id = id, Name = name };

    // --- Loại thiết bị ---
    public async Task<MasterItemDto> CreateDeviceTypeAsync(string name)
    {
        var entity = new DeviceType { Name = RequireName(name, "Tên loại TB") };
        _context.DeviceTypes.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<MasterItemDto?> UpdateDeviceTypeAsync(int id, string name)
    {
        var entity = await _context.DeviceTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return null;
        entity.Name = RequireName(name, "Tên loại TB");
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteDeviceTypeAsync(int id)
    {
        var entity = await _context.DeviceTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return false;
        if (await _context.Devices.AnyAsync(d => d.TypeId == id))
            throw new InvalidOperationException("Không xóa được: còn thiết bị thuộc loại này.");
        _context.DeviceTypes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- Trạng thái (PK không IDENTITY) ---
    public async Task<MasterItemDto> CreateDeviceStatusAsync(string name)
    {
        var nextId = await _context.DeviceStatuses.MaxAsync(s => (int?)s.Id) ?? 0;
        var entity = new DeviceStatus
        {
            Id = nextId + 1,
            Name = RequireName(name, "Tên trạng thái"),
        };
        _context.DeviceStatuses.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<MasterItemDto?> UpdateDeviceStatusAsync(int id, string name)
    {
        var entity = await _context.DeviceStatuses.FirstOrDefaultAsync(s => s.Id == id);
        if (entity is null) return null;
        entity.Name = RequireName(name, "Tên trạng thái");
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteDeviceStatusAsync(int id)
    {
        var entity = await _context.DeviceStatuses.FirstOrDefaultAsync(s => s.Id == id);
        if (entity is null) return false;
        if (await _context.Devices.AnyAsync(d => d.StatusId == id))
            throw new InvalidOperationException("Không xóa được: còn thiết bị ở trạng thái này.");
        _context.DeviceStatuses.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- Loại đơn vị ---
    public async Task<MasterItemDto> CreateOrganizationTypeAsync(string name)
    {
        var entity = new OrganizationType { Name = RequireName(name, "Tên loại đơn vị") };
        _context.OrganizationTypes.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<MasterItemDto?> UpdateOrganizationTypeAsync(int id, string name)
    {
        var entity = await _context.OrganizationTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return null;
        entity.Name = RequireName(name, "Tên loại đơn vị");
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteOrganizationTypeAsync(int id)
    {
        var entity = await _context.OrganizationTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return false;
        if (await _context.Organizations.AnyAsync(o => o.TypeId == id))
            throw new InvalidOperationException("Không xóa được: còn đơn vị thuộc loại này.");
        _context.OrganizationTypes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- Nước sản xuất (PK không IDENTITY) ---
    public async Task<MasterItemDto> CreateCountryAsync(string name)
    {
        var trimmed = RequireName(name, "Tên nước");
        if (trimmed.Length > 50)
            throw new InvalidOperationException("Tên nước tối đa 50 ký tự.");
        var nextId = await _context.Countries.MaxAsync(c => (int?)c.Id) ?? 0;
        var entity = new Country { Id = nextId + 1, Name = trimmed };
        _context.Countries.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<MasterItemDto?> UpdateCountryAsync(int id, string name)
    {
        var entity = await _context.Countries.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return null;
        var trimmed = RequireName(name, "Tên nước");
        if (trimmed.Length > 50)
            throw new InvalidOperationException("Tên nước tối đa 50 ký tự.");
        entity.Name = trimmed;
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteCountryAsync(int id)
    {
        var entity = await _context.Countries.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return false;
        _context.Countries.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- Loại nhiệm vụ / tác vụ ---
    public async Task<MasterItemDto> CreateTaskTypeAsync(string name)
    {
        var entity = new TaskType { Name = RequireName(name, "Tên loại nhiệm vụ") };
        _context.TaskTypes.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<MasterItemDto?> UpdateTaskTypeAsync(int id, string name)
    {
        var entity = await _context.TaskTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return null;
        entity.Name = RequireName(name, "Tên loại nhiệm vụ");
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteTaskTypeAsync(int id)
    {
        var entity = await _context.TaskTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return false;
        _context.TaskTypes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- Từ khóa sự cố ---
    public async Task<MasterItemDto> CreateEventKeywordAsync(string name)
    {
        var trimmed = RequireName(name, "Từ khóa");
        if (trimmed.Length > 50)
            throw new InvalidOperationException("Từ khóa tối đa 50 ký tự.");
        var entity = new EventKeyword { Keyword = trimmed };
        _context.EventKeywords.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Keyword.Trim());
    }

    public async Task<MasterItemDto?> UpdateEventKeywordAsync(int id, string name)
    {
        var entity = await _context.EventKeywords.FirstOrDefaultAsync(k => k.Id == id);
        if (entity is null) return null;
        var trimmed = RequireName(name, "Từ khóa");
        if (trimmed.Length > 50)
            throw new InvalidOperationException("Từ khóa tối đa 50 ký tự.");
        entity.Keyword = trimmed;
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Keyword.Trim());
    }

    public async Task<bool> DeleteEventKeywordAsync(int id)
    {
        var entity = await _context.EventKeywords.FirstOrDefaultAsync(k => k.Id == id);
        if (entity is null) return false;
        _context.EventKeywords.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- Loại nhân sự ---
    public async Task<MasterItemDto> CreatePersonnelTypeAsync(string name)
    {
        var entity = new PersonnelType { Name = RequireName(name, "Tên loại nhân sự") };
        _context.PersonnelTypes.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<MasterItemDto?> UpdatePersonnelTypeAsync(int id, string name)
    {
        var entity = await _context.PersonnelTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return null;
        entity.Name = RequireName(name, "Tên loại nhân sự");
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<bool> DeletePersonnelTypeAsync(int id)
    {
        var entity = await _context.PersonnelTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return false;
        if (await _context.PersonnelRecords.AnyAsync(p => p.PersonnelTypeId == id))
            throw new InvalidOperationException("Không xóa được: còn nhân sự thuộc loại này.");
        _context.PersonnelTypes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- Loại hồ sơ ---
    public async Task<MasterItemDto> CreateDocumentTypeAsync(string name)
    {
        var entity = new DocumentType { Name = RequireName(name, "Tên loại hồ sơ") };
        _context.DocumentTypes.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<MasterItemDto?> UpdateDocumentTypeAsync(int id, string name)
    {
        var entity = await _context.DocumentTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return null;
        entity.Name = RequireName(name, "Tên loại hồ sơ");
        await _context.SaveChangesAsync();
        return Map(entity.Id, entity.Name);
    }

    public async Task<bool> DeleteDocumentTypeAsync(int id)
    {
        var entity = await _context.DocumentTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return false;
        _context.DocumentTypes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
