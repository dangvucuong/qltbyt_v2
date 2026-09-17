from pathlib import Path

ROOT = Path(r"d:\DVC\PROJECT\QLTBYT_NEW")
API = ROOT / "QLTBYT.API"

def w(rel, content):
    p = API / rel.replace("/", "\\")
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content, encoding="utf-8", newline="\n")

w("Models/DTOs/Report/KiemkeDtos.cs", """namespace QLTBYT.API.Models.DTOs.Report;

public class KiemkeAssetQueryParams
{
    public int? OrganizationId { get; set; }
    public string? LocationCode { get; set; }
    public string? ProcurementSource { get; set; }
    public int? ReceivedYear { get; set; }
    public double? MinAmount { get; set; }
    public double? MaxAmount { get; set; }
    /// <summary>Department view excludes zero-value assets (legacy gridThietbi).</summary>
    public bool ExcludeZeroAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class KiemkeAssetRowDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? IdentityCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelCode { get; set; }
    public string? Serial { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? OrganizationName { get; set; }
    public string? LocationCode { get; set; }
    public string? ProcurementSourceLabel { get; set; }
    public double? TotalAmount { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public double? DepreciationAmount { get; set; }
    public string? Note { get; set; }
}
""")

w("Services/ReportService.Kiemke.cs", """using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Report;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Services;

public partial class ReportService
{
    public async Task<PagedResult<KiemkeAssetRowDto>> GetKiemkeAssetsAsync(KiemkeAssetQueryParams query)
    {
        var rows = await BuildKiemkeAssetRowsAsync(query);
        return Paginate(rows, query.Page, query.PageSize);
    }

    public async Task<byte[]> ExportKiemkeAssetsExcelAsync(KiemkeAssetQueryParams query)
    {
        var rows = await BuildKiemkeAssetRowsAsync(query);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Kiem ke TBYT");
        sheet.Cell(1, 1).Value = "Ten TB";
        sheet.Cell(1, 2).Value = "Ma tai san";
        sheet.Cell(1, 3).Value = "Hang SX";
        sheet.Cell(1, 4).Value = "Model";
        sheet.Cell(1, 5).Value = "Serial";
        sheet.Cell(1, 6).Value = "So luong";
        sheet.Cell(1, 7).Value = "DVT";
        sheet.Cell(1, 8).Value = "Khoa phong";
        sheet.Cell(1, 9).Value = "Vi tri";
        sheet.Cell(1, 10).Value = "Nguon mua sam";
        sheet.Cell(1, 11).Value = "Nguyen gia";
        sheet.Cell(1, 12).Value = "Ngay tiep nhan";
        sheet.Cell(1, 13).Value = "Khau hao";
        sheet.Cell(1, 14).Value = "Ghi chu";
        var r = 2;
        foreach (var row in rows)
        {
            sheet.Cell(r, 1).Value = row.Name;
            sheet.Cell(r, 2).Value = row.IdentityCode;
            sheet.Cell(r, 3).Value = row.Manufacturer;
            sheet.Cell(r, 4).Value = row.ModelCode;
            sheet.Cell(r, 5).Value = row.Serial;
            sheet.Cell(r, 6).Value = row.Quantity;
            sheet.Cell(r, 7).Value = row.Unit;
            sheet.Cell(r, 8).Value = row.OrganizationName;
            sheet.Cell(r, 9).Value = row.LocationCode;
            sheet.Cell(r, 10).Value = row.ProcurementSourceLabel;
            sheet.Cell(r, 11).Value = row.TotalAmount;
            sheet.Cell(r, 12).Value = row.ReceivedDate?.ToString("dd/MM/yyyy");
            sheet.Cell(r, 13).Value = row.DepreciationAmount;
            sheet.Cell(r, 14).Value = row.Note;
            r++;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private async Task<List<KiemkeAssetRowDto>> BuildKiemkeAssetRowsAsync(KiemkeAssetQueryParams query)
    {
        IQueryable<Device> queryable = ScopedDevices()
            .Include(d => d.Organization)
            .Where(d => d.TotalAmount != null && d.TotalAmount.Trim() != string.Empty);

        if (query.OrganizationId is > 0)
            queryable = queryable.Where(d => d.OrganizationId == query.OrganizationId);

        if (!string.IsNullOrWhiteSpace(query.ProcurementSource))
            queryable = queryable.Where(d => d.ProcurementSource == query.ProcurementSource.Trim());

        if (query.ReceivedYear is > 1900)
            queryable = queryable.Where(d => d.ReceivedDate != null && d.ReceivedDate.Value.Year == query.ReceivedYear);

        queryable = ApplyLocationFilter(queryable, query.LocationCode);

        var devices = await queryable.OrderBy(d => d.Name).ToListAsync();

        IEnumerable<Device> filtered = devices;
        if (query.ExcludeZeroAmount)
            filtered = filtered.Where(BaocaoHelper.HasAssetValue);

        if (query.MinAmount is > 0)
            filtered = filtered.Where(d => FinancialParseHelper.ParseLegacyAmount(d.TotalAmount) >= query.MinAmount.Value);

        if (query.MaxAmount is > 0)
            filtered = filtered.Where(d => FinancialParseHelper.ParseLegacyAmount(d.TotalAmount) <= query.MaxAmount.Value);

        return filtered.Select(MapKiemkeRow).ToList();
    }

    private static KiemkeAssetRowDto MapKiemkeRow(Device device)
    {
        var amount = FinancialParseHelper.ParseLegacyAmount(device.TotalAmount);
        return new KiemkeAssetRowDto
        {
            Id = device.Id,
            Name = device.Name,
            IdentityCode = device.IdentityCode,
            Manufacturer = device.Manufacturer,
            ModelCode = device.ModelCode,
            Serial = device.Serial,
            Quantity = device.Quantity,
            Unit = device.Unit,
            OrganizationName = device.Organization?.Name,
            LocationCode = device.LocationCode,
            ProcurementSourceLabel = BaocaoHelper.ResolveProcurementLabel(device.ProcurementSource),
            TotalAmount = amount,
            ReceivedDate = device.ReceivedDate,
            DepreciationAmount = BaocaoHelper.CalculateDepreciation(amount, device.ReceivedDate),
            Note = device.Note
        };
    }
}
""")

w("Controllers/AssetInventoryController.cs", """using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Report;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/asset-inventory")]
[Authorize(Policy = "AllUsers")]
public class AssetInventoryController : ControllerBase
{
    private readonly IReportService _reportService;

    public AssetInventoryController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<KiemkeAssetRowDto>>>> GetInventory(
        [FromQuery] KiemkeAssetQueryParams query)
    {
        var data = await _reportService.GetKiemkeAssetsAsync(query);
        return Ok(ApiResponse<PagedResult<KiemkeAssetRowDto>>.CreateSuccess(data));
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportInventory([FromQuery] KiemkeAssetQueryParams query)
    {
        var bytes = await _reportService.ExportKiemkeAssetsExcelAsync(query);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Kiemke_TBYT.xlsx");
    }
}
""")

# patch IReportService
rs = (API / "Services/ReportService.cs").read_text(encoding="utf-8")
if "GetKiemkeAssetsAsync" not in rs:
    rs = rs.replace(
        "    Task<byte[]> ExportBaocaoOperationalStatisticsExcelAsync(BaocaoOperationalQueryParams query);\n}",
        "    Task<byte[]> ExportBaocaoOperationalStatisticsExcelAsync(BaocaoOperationalQueryParams query);\n    Task<PagedResult<KiemkeAssetRowDto>> GetKiemkeAssetsAsync(KiemkeAssetQueryParams query);\n    Task<byte[]> ExportKiemkeAssetsExcelAsync(KiemkeAssetQueryParams query);\n}",
    )
    (API / "Services/ReportService.cs").write_text(rs, encoding="utf-8", newline="\n")

print("kiemke backend ok")
