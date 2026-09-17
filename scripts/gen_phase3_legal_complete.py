# -*- coding: utf-8 -*-
"""Generate Phase 3 legal docs + asset report backend (UTF-8)."""
from pathlib import Path

ROOT = Path("d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API")

def w(rel: str, content: str):
    p = ROOT / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content, encoding="utf-8", newline="\n")
    print("write", rel)

w(
    "Models/DTOs/Legal/LegalDocumentDtos.cs",
    """
namespace QLTBYT.API.Models.DTOs.Legal;

public class LegalDocumentDto
{
    public int Id { get; set; }
    public string? InspectionCertificate { get; set; }
    public string? ImportPermit { get; set; }
    public string? CirculationPermit { get; set; }
    public string? FilePath { get; set; }
    public string? BiddingInfo { get; set; }
    public double? PriceWithVat { get; set; }
    public string? DeviceCodes { get; set; }
}

public class CreateLegalDocumentRequest
{
    public string? InspectionCertificate { get; set; }
    public string? ImportPermit { get; set; }
    public string? CirculationPermit { get; set; }
    public string? BiddingInfo { get; set; }
    public double? PriceWithVat { get; set; }
    public string? DeviceCodes { get; set; }
}

public class UpdateLegalDocumentRequest
{
    public string? InspectionCertificate { get; set; }
    public string? ImportPermit { get; set; }
    public string? CirculationPermit { get; set; }
    public string? BiddingInfo { get; set; }
    public double? PriceWithVat { get; set; }
    public string? DeviceCodes { get; set; }
}

public class LinkDeviceLegalRequest
{
    public int? LegalDocumentId { get; set; }
}
""".strip() + "\n",
)

w(
    "Services/LegalDocumentService.cs",
    """
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

    public LegalDocumentService(
        AppDbContext context,
        IFileStorageService files,
        ICurrentUserService currentUser)
    {
        _context = context;
        _files = files;
        _currentUser = currentUser;
    }

    public async Task<List<LegalDocumentDto>> GetAllAsync()
    {
        var list = await _context.LegalDocuments.AsNoTracking()
            .OrderByDescending(l => l.Id)
            .ToListAsync();
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
        var devices = await _context.Devices
            .Where(d => d.LegalDocumentRef == id.ToString())
            .ToListAsync();
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
""".strip() + "\n",
)

w(
    "Controllers/LegalDocumentsController.cs",
    """
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Legal;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/legal-documents")]
[Authorize(Policy = "AllUsers")]
public class LegalDocumentsController : ControllerBase
{
    private readonly ILegalDocumentService _legalService;

    public LegalDocumentsController(ILegalDocumentService legalService)
    {
        _legalService = legalService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LegalDocumentDto>>>> GetAll()
    {
        var list = await _legalService.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<LegalDocumentDto>>.CreateSuccess(list));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> GetById(int id)
    {
        var item = await _legalService.GetByIdAsync(id);
        if (item is null)
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("Không tìm th¥y hÓ s¡ pháp lý."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(item));
    }

    [HttpPost]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> Create([FromBody] CreateLegalDocumentRequest request)
    {
        var item = await _legalService.CreateAsync(request);
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(item, "Thêm hÓ s¡ pháp lý thành công."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> Update(int id, [FromBody] UpdateLegalDocumentRequest request)
    {
        var item = await _legalService.UpdateAsync(id, request);
        if (item is null)
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("Không tìm th¥y hÓ s¡ pháp lý."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(item, "C­p nh­t hÓ s¡ thành công."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var ok = await _legalService.DeleteAsync(id);
        if (!ok)
            return NotFound(ApiResponse<object>.CreateFail("Không tìm th¥y hÓ s¡ pháp lý."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa hÓ s¡ thành công."));
    }

    [HttpPost("{id:int}/file")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> UploadFile(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<LegalDocumentDto>.CreateFail("File không hãp lÇ."));
        await using var stream = file.OpenReadStream();
        var item = await _legalService.UploadFileAsync(id, stream, file.FileName);
        if (item is null)
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("Không tìm th¥y hÓ s¡ pháp lý."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(item, "T£i file hÓ s¡ thành công."));
    }
}
""".strip() + "\n",
)

# IFileStorageService + LocalFileStorageService patch
ifs = ROOT / "Services/IFileStorageService.cs"
ifs.write_text(
    """
namespace QLTBYT.API.Services;

public interface IFileStorageService
{
    Task<string> SaveDeviceFileAsync(int deviceId, Stream content, string fileName);
    Task<string> SaveLegalDocumentFileAsync(int legalDocumentId, Stream content, string fileName);
    string GetRootPath();
}
""".strip() + "\n",
    encoding="utf-8",
    newline="\n",
)
print("patch IFileStorageService")

lfs = ROOT / "Services/LocalFileStorageService.cs"
lfs_text = lfs.read_text(encoding="utf-8")
if "SaveLegalDocumentFileAsync" not in lfs_text:
    lfs_text = lfs_text.replace(
        "    public string GetRootPath() => _root;\n",
        "    public async Task<string> SaveLegalDocumentFileAsync(int legalDocumentId, Stream content, string fileName)\n"
        "    {\n"
        "        var safeName = Path.GetFileName(fileName);\n"
        "        var folder = Path.Combine(_root, \"legal\", legalDocumentId.ToString());\n"
        "        Directory.CreateDirectory(folder);\n"
        "        var fullPath = Path.Combine(folder, safeName);\n"
        "        await using var fs = File.Create(fullPath);\n"
        "        await content.CopyToAsync(fs);\n"
        "        return Path.Combine(\"FILELUU\", \"legal\", legalDocumentId.ToString(), safeName).Replace('\\\\', '/');\n"
        "    }\n\n"
        "    public string GetRootPath() => _root;\n",
    )
    lfs.write_text(lfs_text, encoding="utf-8", newline="\n")
    print("patch LocalFileStorageService")

# Program.cs register service
prog = Path("d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API/Program.cs")
pt = prog.read_text(encoding="utf-8")
if "ILegalDocumentService" not in pt:
    pt = pt.replace(
        "builder.Services.AddScoped<IAdminMasterDataService, AdminMasterDataService>();",
        "builder.Services.AddScoped<IAdminMasterDataService, AdminMasterDataService>();\n"
        "builder.Services.AddScoped<ILegalDocumentService, LegalDocumentService>();",
    )
    prog.write_text(pt, encoding="utf-8", newline="\n")
    print("patch Program.cs")

# DevicesController legal endpoints
dc = ROOT / "Controllers/DevicesController.cs"
dct = dc.read_text(encoding="utf-8")
if "legal-document" not in dct:
    dct = dct.replace(
        "using QLTBYT.API.Models.DTOs.Event;\n",
        "using QLTBYT.API.Models.DTOs.Event;\nusing QLTBYT.API.Models.DTOs.Legal;\n",
    )
    dct = dct.replace(
        "    private readonly IDeviceEventService _deviceEventService;\n",
        "    private readonly IDeviceEventService _deviceEventService;\n    private readonly ILegalDocumentService _legalDocumentService;\n",
    )
    dct = dct.replace(
        "    public DevicesController(IDeviceService deviceService, IDeviceEventService deviceEventService)\n"
        "    {\n"
        "        _deviceService = deviceService;\n"
        "        _deviceEventService = deviceEventService;\n"
        "    }\n",
        "    public DevicesController(\n"
        "        IDeviceService deviceService,\n"
        "        IDeviceEventService deviceEventService,\n"
        "        ILegalDocumentService legalDocumentService)\n"
        "    {\n"
        "        _deviceService = deviceService;\n"
        "        _deviceEventService = deviceEventService;\n"
        "        _legalDocumentService = legalDocumentService;\n"
        "    }\n",
    )
    insert = """
    [HttpGet("{id:int}/legal-document")]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> GetLegalDocument(int id)
    {
        var doc = await _legalDocumentService.GetForDeviceAsync(id);
        if (doc is null)
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("Ch°a có hÓ s¡ pháp lý cho thi¿t bË này."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(doc));
    }

    [HttpPut("{id:int}/legal-document")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<object>>> LinkLegalDocument(int id, [FromBody] LinkDeviceLegalRequest request)
    {
        var ok = await _legalDocumentService.LinkDeviceAsync(id, request.LegalDocumentId);
        if (!ok)
            return NotFound(ApiResponse<object>.CreateFail("Không liên k¿t °ãc hÓ s¡ / thi¿t bË."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Liên k¿t hÓ s¡ pháp lý thành công."));
    }

"""
    dct = dct.replace("\n    [HttpPost(\"{id:int}/transfer\")]", insert + "\n    [HttpPost(\"{id:int}/transfer\")]")
    dc.write_text(dct, encoding="utf-8", newline="\n")
    print("patch DevicesController")

# ReportService asset export
rs = ROOT / "Services/ReportService.cs"
rst = rs.read_text(encoding="utf-8")
if "ExportAssetsExcelAsync" not in rst:
    rst = rst.replace(
        "    Task<byte[]> ExportExpiringInspectionExcelAsync(int withinDays = 90);\n",
        "    Task<byte[]> ExportExpiringInspectionExcelAsync(int withinDays = 90);\n"
        "    Task<byte[]> ExportAssetsExcelAsync();\n",
    )
    asset_method = '''
    public async Task<byte[]> ExportAssetsExcelAsync()
    {
        var rows = await ScopedDevices()
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .OrderBy(d => d.DeviceCode)
            .ToListAsync();

        var legalList = await _context.LegalDocuments.AsNoTracking().ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Tai san TB");
        sheet.Cell(1, 1).Value = "Mã TB";
        sheet.Cell(1, 2).Value = "Tên TB";
        sheet.Cell(1, 3).Value = "Lo¡i";
        sheet.Cell(1, 4).Value = "¡n vË";
        sheet.Cell(1, 5).Value = "Ngày ti¿p nh­n";
        sheet.Cell(1, 6).Value = "Gói th§u";
        sheet.Cell(1, 7).Value = "Giá có VAT (HS)";
        int r = 2;
        foreach (var d in rows)
        {
            var legal = ResolveLegal(d, legalList);
            sheet.Cell(r, 1).Value = d.DeviceCode;
            sheet.Cell(r, 2).Value = d.Name;
            sheet.Cell(r, 3).Value = d.DeviceType?.Name;
            sheet.Cell(r, 4).Value = d.Organization?.Name;
            sheet.Cell(r, 5).Value = d.ReceivedDate?.ToString("dd/MM/yyyy");
            sheet.Cell(r, 6).Value = d.BiddingPackageCode;
            sheet.Cell(r, 7).Value = legal?.PriceWithVat;
            r++;
        }
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static LegalDocument? ResolveLegal(Models.Domain.Device d, List<LegalDocument> legalList)
    {
        if (!string.IsNullOrWhiteSpace(d.LegalDocumentRef)
            && int.TryParse(d.LegalDocumentRef.Trim(), out var id))
        {
            var linked = legalList.FirstOrDefault(l => l.Id == id);
            if (linked is not null) return linked;
        }
        var code = d.DeviceCode?.Trim();
        if (string.IsNullOrEmpty(code)) return null;
        return legalList.FirstOrDefault(l => l.DeviceCodes != null && l.DeviceCodes.Contains(code));
    }
'''
    rst = rst.replace(
        "    private IQueryable<Models.Domain.Device> ScopedDevices()",
        asset_method + "\n    private IQueryable<Models.Domain.Device> ScopedDevices()",
    )
    rst = rst.replace(
        "using QLTBYT.API.Models.DTOs.Device;\n",
        "using QLTBYT.API.Models.DTOs.Device;\nusing QLTBYT.API.Models.Domain;\n",
    )
    rs.write_text(rst, encoding="utf-8", newline="\n")
    print("patch ReportService")

rc = ROOT / "Controllers/ReportsController.cs"
rct = rc.read_text(encoding="utf-8")
if "assets/export" not in rct:
    rct = rct.replace(
        "    [HttpGet(\"devices/expiring/export\")]",
        "    [HttpGet(\"assets/export\")]\n"
        "    public async Task<IActionResult> ExportAssets()\n"
        "    {\n"
        "        var bytes = await _reportService.ExportAssetsExcelAsync();\n"
        "        return File(bytes, \"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet\", \"Bao-cao-tai-san-thiet-bi.xlsx\");\n"
        "    }\n\n"
        "    [HttpGet(\"devices/expiring/export\")]",
    )
    rc.write_text(rct, encoding="utf-8", newline="\n")
    print("patch ReportsController")

# Sample legal seed
ref = Path("d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API/Infrastructure/SampleDataVietnameseRefresher.cs")
rt = ref.read_text(encoding="utf-8")
if "EnsureDemoLegalDocumentAsync" not in rt:
    rt = rt.replace(
        "        await EnsureDemoBiddingAsync(context, cancellationToken);",
        "        await EnsureDemoBiddingAsync(context, cancellationToken);\n        await EnsureDemoLegalDocumentAsync(context, cancellationToken);",
    )
    legal_fn = '''
    private static async Task EnsureDemoLegalDocumentAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var device = await context.Devices.FirstOrDefaultAsync(d => d.DeviceCode == "TB-DEMO-001", cancellationToken);
        if (device is null)
            return;

        var existing = await context.LegalDocuments
            .FirstOrDefaultAsync(l => l.DeviceCodes != null && l.DeviceCodes.Contains("TB-DEMO-001"), cancellationToken);
        if (existing is null)
        {
            existing = new LegalDocument
            {
                InspectionCertificate = "KD-DEMO-2024-001",
                ImportPermit = "NK-DEMO-2024-001",
                CirculationPermit = "LHH-DEMO-2024-001",
                BiddingInfo = "Gói th§u mua máy thß oxy demo (GT-DEMO-2024)",
                PriceWithVat = 125000000,
                DeviceCodes = "TB-DEMO-001"
            };
            context.LegalDocuments.Add(existing);
            await context.SaveChangesAsync(cancellationToken);
        }

        device.LegalDocumentRef = existing.Id.ToString();
        await context.SaveChangesAsync(cancellationToken);
    }

'''
    rt = rt.replace("\n    private static async Task EnsureDemoEventsAsync", legal_fn + "\n    private static async Task EnsureDemoEventsAsync")
    ref.write_text(rt, encoding="utf-8", newline="\n")
    print("patch SampleData legal seed")

print("done")
