using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Bidding;
using QLTBYT.API.Models.DTOs.Device;

namespace QLTBYT.API.Services;

public interface IBiddingService
{
    Task<List<BiddingPackageDto>> GetAllAsync();
    Task<BiddingPackageDto?> GetByIdAsync(int id);
    Task<BiddingPackageDto> CreateAsync(CreateBiddingPackageRequest request);
    Task<BiddingPackageDto?> UpdateAsync(int id, UpdateBiddingPackageRequest request);
    Task<bool> DeleteAsync(int id);
    Task<List<DeviceListDto>> GetDevicesAsync(int packageId);
}

public class BiddingService : IBiddingService
{
    private readonly AppDbContext _context;

    public BiddingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BiddingPackageDto>> GetAllAsync()
    {
        var list = await _context.BiddingPackages.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        return await MapWithCountsAsync(list);
    }

    public async Task<BiddingPackageDto?> GetByIdAsync(int id)
    {
        var b = await _context.BiddingPackages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (b is null) return null;
        var dto = Map(b);
        dto.DeviceCount = await CountDevicesAsync(b.PackageCode);
        return dto;
    }

    public async Task<BiddingPackageDto> CreateAsync(CreateBiddingPackageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PackageCode))
            throw new InvalidOperationException("Mã gói thầu là bắt buộc.");

        var entity = new BiddingPackage
        {
            PackageCode = request.PackageCode.Trim(),
            Name = request.Name?.Trim(),
            Status = request.Status?.Trim(),
            WinningVendor = request.WinningVendor?.Trim(),
            TaxCode = request.TaxCode?.Trim(),
            ContractNumber = request.ContractNumber?.Trim(),
            Value = request.Value
        };
        _context.BiddingPackages.Add(entity);
        await _context.SaveChangesAsync();
        var dto = Map(entity);
        dto.DeviceCount = 0;
        return dto;
    }

    public async Task<BiddingPackageDto?> UpdateAsync(int id, UpdateBiddingPackageRequest request)
    {
        var entity = await _context.BiddingPackages.FirstOrDefaultAsync(b => b.Id == id);
        if (entity is null) return null;
        if (request.Name is not null) entity.Name = request.Name.Trim();
        if (request.Status is not null) entity.Status = request.Status.Trim();
        if (request.WinningVendor is not null) entity.WinningVendor = request.WinningVendor.Trim();
        if (request.TaxCode is not null) entity.TaxCode = request.TaxCode.Trim();
        if (request.ContractNumber is not null) entity.ContractNumber = request.ContractNumber.Trim();
        if (request.Value.HasValue) entity.Value = request.Value;
        await _context.SaveChangesAsync();
        var dto = Map(entity);
        dto.DeviceCount = await CountDevicesAsync(entity.PackageCode);
        return dto;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.BiddingPackages.FirstOrDefaultAsync(b => b.Id == id);
        if (entity is null) return false;
        _context.BiddingPackages.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DeviceListDto>> GetDevicesAsync(int packageId)
    {
        var pkg = await _context.BiddingPackages.AsNoTracking().FirstOrDefaultAsync(b => b.Id == packageId);
        if (pkg?.PackageCode is null) return new List<DeviceListDto>();
        return await _context.Devices.AsNoTracking()
            .Where(d => d.BiddingPackageCode == pkg.PackageCode)
            .Select(d => new DeviceListDto
            {
                Id = d.Id,
                DeviceCode = d.DeviceCode ?? d.Id.ToString(),
                Name = d.Name
            })
            .ToListAsync();
    }

    private async Task<List<BiddingPackageDto>> MapWithCountsAsync(List<BiddingPackage> list)
    {
        var result = new List<BiddingPackageDto>();
        foreach (var b in list)
        {
            var dto = Map(b);
            dto.DeviceCount = await CountDevicesAsync(b.PackageCode);
            result.Add(dto);
        }
        return result;
    }

    private async Task<int> CountDevicesAsync(string? packageCode)
    {
        if (string.IsNullOrWhiteSpace(packageCode)) return 0;
        return await _context.Devices.CountAsync(d => d.BiddingPackageCode == packageCode);
    }

    private static BiddingPackageDto Map(BiddingPackage b) => new()
    {
        Id = b.Id,
        PackageCode = b.PackageCode,
        Name = b.Name,
        Status = b.Status,
        WinningVendor = b.WinningVendor,
        TaxCode = b.TaxCode,
        ContractNumber = b.ContractNumber,
        Value = b.Value
    };
}
