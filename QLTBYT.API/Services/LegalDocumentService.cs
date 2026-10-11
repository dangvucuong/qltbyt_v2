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
        // Giống thongtinhs.aspx: SELECT * FROM ThongtinHS (thứ tự ID tăng dần)
        var list = await _context.LegalDocuments.AsNoTracking().OrderBy(l => l.Id).ToListAsync();
        var deviceMap = await BuildDeviceLookupAsync(list.Select(l => l.DeviceCodes));
        return list.Select(l => Map(l, deviceMap)).ToList();
    }

    public async Task<LegalDocumentDto?> GetByIdAsync(int id)
    {
        var e = await _context.LegalDocuments.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (e is null) return null;
        var deviceMap = await BuildDeviceLookupAsync(new[] { e.DeviceCodes });
        return Map(e, deviceMap);
    }

    public async Task<LegalDocumentDto?> GetForDeviceAsync(int deviceId)
    {
        var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceId);
        if (device is null || !IsDeviceAllowed(device))
            return null;

        // 1) IDHoso chứa ID hồ sơ (số). Bỏ qua nếu là tên file HDBaotri.
        if (!string.IsNullOrWhiteSpace(device.LegalDocumentRef)
            && int.TryParse(device.LegalDocumentRef.Trim(), out var legalId))
        {
            var linked = await _context.LegalDocuments.AsNoTracking().FirstOrDefaultAsync(l => l.Id == legalId);
            if (linked is not null)
            {
                var map = await BuildDeviceLookupAsync(new[] { linked.DeviceCodes });
                return Map(linked, map);
            }
        }

        // 2) Legacy: MaThietbi = ID thiết bị
        var byDeviceId = await _context.LegalDocuments.AsNoTracking()
            .Where(l => l.DeviceCodes != null && l.DeviceCodes == deviceId.ToString())
            .OrderByDescending(l => l.Id)
            .FirstOrDefaultAsync();
        if (byDeviceId is not null)
        {
            var map = await BuildDeviceLookupAsync(new[] { byDeviceId.DeviceCodes });
            return Map(byDeviceId, map);
        }

        // 3) Fallback: MaThietbi chứa MaTB
        var code = device.DeviceCode?.Trim();
        if (!string.IsNullOrEmpty(code))
        {
            var match = await _context.LegalDocuments.AsNoTracking()
                .Where(l => l.DeviceCodes != null && l.DeviceCodes.Contains(code))
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();
            if (match is not null)
            {
                var map = await BuildDeviceLookupAsync(new[] { match.DeviceCodes });
                return Map(match, map);
            }
        }

        return null;
    }

    public async Task<LegalDocumentDto> CreateAsync(CreateLegalDocumentRequest request)
    {
        var entity = ApplyRequest(new LegalDocument(), request);
        _context.LegalDocuments.Add(entity);
        await _context.SaveChangesAsync();
        await SyncDeviceLinksAsync(entity);
        var map = await BuildDeviceLookupAsync(new[] { entity.DeviceCodes });
        return Map(entity, map);
    }

    public async Task<LegalDocumentDto?> UpdateAsync(int id, UpdateLegalDocumentRequest request)
    {
        var entity = await _context.LegalDocuments.FirstOrDefaultAsync(l => l.Id == id);
        if (entity is null) return null;
        ApplyRequest(entity, request);
        await _context.SaveChangesAsync();
        await SyncDeviceLinksAsync(entity);
        var map = await BuildDeviceLookupAsync(new[] { entity.DeviceCodes });
        return Map(entity, map);
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
        var map = await BuildDeviceLookupAsync(new[] { entity.DeviceCodes });
        return Map(entity, map);
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

        var legal = await _context.LegalDocuments.FirstOrDefaultAsync(l => l.Id == legalDocumentId);
        if (legal is null) return false;

        device.LegalDocumentRef = legalDocumentId.Value.ToString();
        // Legacy: MaThietbi = ID thiết bị
        legal.DeviceCodes = deviceId.ToString();
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task SyncDeviceLinksAsync(LegalDocument entity)
    {
        if (string.IsNullOrWhiteSpace(entity.DeviceCodes))
            return;

        var token = entity.DeviceCodes.Trim();
        Device? device = null;
        if (int.TryParse(token, out var deviceId) && deviceId > 0)
            device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device is null)
            device = await _context.Devices.FirstOrDefaultAsync(d => d.DeviceCode == token);

        if (device is null) return;

        // Chỉ ghi IDHoso khi đang trống hoặc đang là số (ID hồ sơ). Không ghi đè tên file HDBaotri.
        if (string.IsNullOrWhiteSpace(device.LegalDocumentRef)
            || int.TryParse(device.LegalDocumentRef.Trim(), out _))
        {
            device.LegalDocumentRef = entity.Id.ToString();
            await _context.SaveChangesAsync();
        }
    }

    private async Task<Dictionary<int, (string? Code, string? Name)>> BuildDeviceLookupAsync(
        IEnumerable<string?> rawValues)
    {
        var ids = new HashSet<int>();
        foreach (var raw in rawValues)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            foreach (var part in raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (int.TryParse(part, out var id) && id > 0)
                    ids.Add(id);
            }
        }

        if (ids.Count == 0)
            return new Dictionary<int, (string? Code, string? Name)>();

        var devices = await _context.Devices.AsNoTracking()
            .Where(d => ids.Contains(d.Id))
            .Select(d => new { d.Id, d.DeviceCode, d.Name })
            .ToListAsync();

        return devices.ToDictionary(d => d.Id, d => (d.DeviceCode, d.Name));
    }

    private bool IsDeviceAllowed(Device device)
    {
        if (!_currentUser.MustFilterByOrganization) return true;
        return device.OrganizationId != null && _currentUser.OrganizationIds.Contains(device.OrganizationId.Value);
    }

    private static LegalDocument ApplyRequest(LegalDocument entity, CreateLegalDocumentRequest request)
    {
        entity.InspectionCertificate = TrimOrNull(request.InspectionCertificate);
        entity.ImportPermit = TrimOrNull(request.ImportPermit);
        entity.ImportPermitExpiry = TrimOrNull(request.ImportPermitExpiry);
        entity.CirculationPermit = TrimOrNull(request.CirculationPermit);
        entity.CirculationPermitExpiry = TrimOrNull(request.CirculationPermitExpiry);
        entity.CertificateOfOrigin = TrimOrNull(request.CertificateOfOrigin);
        entity.OriginCountryId = TrimOrNull(request.OriginCountryId);
        entity.QualityCertificate = TrimOrNull(request.QualityCertificate);
        entity.ManufactureYear = TrimOrNull(request.ManufactureYear);
        entity.CustomsDeclaration = TrimOrNull(request.CustomsDeclaration);
        entity.PackingList = TrimOrNull(request.PackingList);
        entity.BillOfLading = TrimOrNull(request.BillOfLading);
        entity.ImportInvoice = TrimOrNull(request.ImportInvoice);
        entity.SalesInvoice = TrimOrNull(request.SalesInvoice);
        entity.BiddingInfo = TrimOrNull(request.BiddingInfo);
        entity.PriceWithVat = request.PriceWithVat;
        entity.PriceWithoutVat = request.PriceWithoutVat;
        entity.DepreciationInfo = TrimOrNull(request.DepreciationInfo);
        entity.FundingSource = TrimOrNull(request.FundingSource);
        entity.DeviceCodes = TrimOrNull(request.DeviceCodes);
        return entity;
    }

    private static string? TrimOrNull(string? value)
    {
        var t = value?.Trim();
        return string.IsNullOrEmpty(t) ? null : t;
    }

    private static LegalDocumentDto Map(
        LegalDocument l,
        IReadOnlyDictionary<int, (string? Code, string? Name)> deviceMap)
    {
        string? deviceCode = null;
        string? deviceDisplay = null;
        var raw = l.DeviceCodes?.Trim();
        if (!string.IsNullOrEmpty(raw) && int.TryParse(raw, out var deviceId) && deviceId > 0
            && deviceMap.TryGetValue(deviceId, out var info))
        {
            deviceCode = info.Code;
            // Giống cũ: hiển thị TenTB; kèm MaTB nếu có
            deviceDisplay = string.IsNullOrWhiteSpace(info.Code)
                ? info.Name
                : $"{info.Code} — {info.Name}";
        }
        else if (!string.IsNullOrEmpty(raw) && raw != "0")
        {
            deviceDisplay = raw;
            deviceCode = raw;
        }

        return new LegalDocumentDto
        {
            Id = l.Id,
            InspectionCertificate = l.InspectionCertificate,
            ImportPermit = l.ImportPermit,
            ImportPermitExpiry = l.ImportPermitExpiry,
            CirculationPermit = l.CirculationPermit,
            CirculationPermitExpiry = l.CirculationPermitExpiry,
            CertificateOfOrigin = l.CertificateOfOrigin,
            OriginCountryId = l.OriginCountryId,
            QualityCertificate = l.QualityCertificate,
            ManufactureYear = l.ManufactureYear,
            CustomsDeclaration = l.CustomsDeclaration,
            PackingList = l.PackingList,
            BillOfLading = l.BillOfLading,
            ImportInvoice = l.ImportInvoice,
            SalesInvoice = l.SalesInvoice,
            FilePath = l.FilePath,
            BiddingInfo = l.BiddingInfo,
            PriceWithVat = l.PriceWithVat,
            PriceWithoutVat = l.PriceWithoutVat,
            DepreciationInfo = l.DepreciationInfo,
            FundingSource = l.FundingSource,
            DeviceCodes = l.DeviceCodes,
            DeviceCode = deviceCode,
            DeviceDisplayName = deviceDisplay
        };
    }
}
