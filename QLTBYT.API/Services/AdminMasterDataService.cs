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
}

public class AdminMasterDataService : IAdminMasterDataService
{
    private readonly AppDbContext _context;

    public AdminMasterDataService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MasterItemDto> CreateDeviceTypeAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Ten loai TB la bat buoc.");
        var entity = new DeviceType { Name = name.Trim() };
        _context.DeviceTypes.Add(entity);
        await _context.SaveChangesAsync();
        return new MasterItemDto { Id = entity.Id, Name = entity.Name };
    }

    public async Task<MasterItemDto?> UpdateDeviceTypeAsync(int id, string name)
    {
        var entity = await _context.DeviceTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return null;
        entity.Name = name.Trim();
        await _context.SaveChangesAsync();
        return new MasterItemDto { Id = entity.Id, Name = entity.Name };
    }

    public async Task<bool> DeleteDeviceTypeAsync(int id)
    {
        var entity = await _context.DeviceTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return false;
        _context.DeviceTypes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MasterItemDto> CreateDeviceStatusAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Ten trang thai la bat buoc.");
        var entity = new DeviceStatus { Name = name.Trim() };
        _context.DeviceStatuses.Add(entity);
        await _context.SaveChangesAsync();
        return new MasterItemDto { Id = entity.Id, Name = entity.Name };
    }

    public async Task<MasterItemDto?> UpdateDeviceStatusAsync(int id, string name)
    {
        var entity = await _context.DeviceStatuses.FirstOrDefaultAsync(s => s.Id == id);
        if (entity is null) return null;
        entity.Name = name.Trim();
        await _context.SaveChangesAsync();
        return new MasterItemDto { Id = entity.Id, Name = entity.Name };
    }

    public async Task<bool> DeleteDeviceStatusAsync(int id)
    {
        var entity = await _context.DeviceStatuses.FirstOrDefaultAsync(s => s.Id == id);
        if (entity is null) return false;
        _context.DeviceStatuses.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
