using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.DTOs.Legal;

namespace QLTBYT.API.Services;

public interface ILegalDocumentService
{
    Task<List<LegalDocumentDto>> GetAllAsync();
    Task<LegalDocumentDto?> GetByIdAsync(int id);
    Task<LegalDocumentDto?> GetForDeviceAsync(int deviceId);
    Task<LegalDocumentDto> CreateAsync(CreateLegalDocumentRequest request);
    Task<LegalDocumentDto?> UpdateAsync(int id, UpdateLegalDocumentRequest request);
    Task<bool> DeleteAsync(int id);
    Task<LegalDocumentDto?> UploadFileAsync(int id, Stream stream, string fileName);
    Task<bool> LinkDeviceAsync(int deviceId, int? legalDocumentId);
}

public class LegalDocumentService : ILegalDocumentService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _files;
    private readonly ICurrentUserService _currentUser;

    public LegalDocumentService(AppDbContext context, IFileStorageService files, ICurrentUserService currentUser)
    {
        _context = context;
        _files = files;
        _currentUser = currentUser;
    }

    public async Task<List<LegalDocumentDto>> GetAllAsync()
    {
        var list = await _context.LegalDocuments.AsNoTracking().OrderByDescending(l => l.Id).ToListAsync();
        return list.Select(Map).ToList();
    }

    public async Task<LegalDocumentDto?> GetByIdAsync(int id)
    {
        var e = await _context.LegalDocuments.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        return e is null ? null : Map(e);
    }

    public async Task<LegalDocumentDto?> GetForDeviceAsync(int deviceId)
    {
        var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceId);
        if (device is null || !IsDeviceAllowed(device))
            return null;

        if (!string.IsNullOrWhiteSpace(device.LegalDocumentRef)
            && int.TryParse(device.LegalDocumentRef.Trim(), out var legalId))
        {
            var linked = await _context.LegalDocuments.AsNoTracking().FirstOrDefaultAsync(l => l.Id == legalId);
            if (linked is not null)
                return Map(linked);
        }

        var code = device.DeviceCode?.Trim();
        if (string.IsNullOrEmpty(code))
            return null;

        var match = await _context.LegalDocuments.AsNoTracking()
            .Where(l => l.DeviceCodes != null && l.DeviceCodes.Contains(code))
            .OrderByDescending(l => l.Id)
            .FirstOrDefaultAsync();
        return match is null ? null : Map(match);
    }

    public async Task<LegalDocumentDto> CreateAsync(CreateLegalDocumentRequest request)
    {
        var entity = new LegalDocument
        {
            InspectionCertificate = request.InspectionCertificate?.Trim(),
            ImportPermit = request.ImportPermit?.Trim(),
            CirculationPermit = request.CirculationPermit?.Trim(),
            BiddingInfo = request.BiddingInfo?.Trim(),
            PriceWithVat = request.PriceWithVat,
            DeviceCodes = request.DeviceCodes?.Trim()
        };
        _context.LegalDocuments.Add(entity);
        await _context.SaveChangesAsync();
        await SyncDeviceLinksAsync(entity);
        return Map(entity);
    }

    public async Task<LegalDocumentDto?> UpdateAsync(int id, UpdateLegalDocumentRequest request)
    {
        var entity = await _context.LegalDocuments.FirstOrDefaultAsync(l => l.Id == id);
        if (entity is null) return null;
        if (request.InspectionCertificate is not null) entity.InspectionCertificate = request.InspectionCertificate.Trim();
        if (request.ImportPermit is not null) entity.ImportPermit = request.ImportPermit.Trim();
        if (request.CirculationPermit is not null) entity.CirculationPermit = request.CirculationPermit.Trim();
        if (request.BiddingInfo is not null) entity.BiddingInfo = request.BiddingInfo.Trim();
        if (request.PriceWithVat.HasValue) entity.PriceWithVat = request.PriceWithVat;
        if (request.DeviceCodes is not null) entity.DeviceCodes = request.DeviceCodes.Trim();
        await _context.SaveChangesAsync();
        await SyncDeviceLinksAsync(entity);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.LegalDocuments.FirstOrDefaultAsync(l => l.Id == id);
        if (entity is null) return false;
        var devices = await _context.Devices.Where(d => d.LegalDocumentRef == id.ToString()).ToListAsync();
        foreach (var d in devices)
            d.LegalDocumentRef = null;
        _context.LegalDocuments.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LegalDocumentDto?> UploadFileAsync(int id, Stream stream, string fileName)
    {
        var entity = await _context.LegalDocuments.FirstOrDefaultAsync(l => l.Id == id);
        if (entity is null) return null;
        var path = await _files.SaveLegalDocumentFileAsync(id, stream, fileName);
        entity.FilePath = path;
        await _context.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<bool> LinkDeviceAsync(int deviceId, int? legalDocumentId)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
        if (device is null || !IsDeviceAllowed(device))
            return false;

        if (legalDocumentId is null)
        {
            device.LegalDocumentRef = null;
            await _context.SaveChangesAsync();
            return true;
        }

        var legal = await _context.LegalDocuments.AsNoTracking().FirstOrDefaultAsync(l => l.Id == legalDocumentId);
        if (legal is null) return false;

        device.LegalDocumentRef = legalDocumentId.Value.ToString();
        if (!string.IsNullOrWhiteSpace(device.DeviceCode))
        {
            var codes = (legal.DeviceCodes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            if (!codes.Contains(device.DeviceCode, StringComparer.OrdinalIgnoreCase))
            {
                codes.Add(device.DeviceCode);
                var tracked = await _context.LegalDocuments.FirstAsync(l => l.Id == legalDocumentId);
                tracked.DeviceCodes = string.Join(",", codes);
            }
        }
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task SyncDeviceLinksAsync(LegalDocument entity)
    {
        if (string.IsNullOrWhiteSpace(entity.DeviceCodes))
            return;
        var codes = entity.DeviceCodes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var devices = await _context.Devices.Where(d => d.DeviceCode != null && codes.Contains(d.DeviceCode)).ToListAsync();
        foreach (var d in devices)
            d.LegalDocumentRef = entity.Id.ToString();
        await _context.SaveChangesAsync();
    }

    private bool IsDeviceAllowed(Device device)
    {
        if (!_currentUser.MustFilterByOrganization) return true;
        return device.OrganizationId != null && _currentUser.OrganizationIds.Contains(device.OrganizationId.Value);
    }

    private static LegalDocumentDto Map(LegalDocument l) => new()
    {
        Id = l.Id,
        InspectionCertificate = l.InspectionCertificate,
        ImportPermit = l.ImportPermit,
        CirculationPermit = l.CirculationPermit,
        FilePath = l.FilePath,
        BiddingInfo = l.BiddingInfo,
        PriceWithVat = l.PriceWithVat,
        DeviceCodes = l.DeviceCodes
    };
}