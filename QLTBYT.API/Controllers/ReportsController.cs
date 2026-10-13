using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Report;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = "ReportsAccess")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IWarehouseService _warehouse;

    public ReportsController(IReportService reportService, IWarehouseService warehouse)
    {
        _reportService = reportService;
        _warehouse = warehouse;
    }

    [HttpGet("devices/summary")]
    public async Task<ActionResult<ApiResponse<DeviceReportSummaryDto>>> GetSummary([FromQuery] int expiringWithinDays = 30)
    {
        var data = await _reportService.GetDeviceReportSummaryAsync(expiringWithinDays);
        return Ok(ApiResponse<DeviceReportSummaryDto>.CreateSuccess(data));
    }

    [HttpGet("devices/grid")]
    public async Task<ActionResult<ApiResponse<PagedResult<DeviceReportRowDto>>>> GetDeviceGrid([FromQuery] DeviceQueryParams query)
    {
        var data = await _reportService.GetDeviceReportGridAsync(query);
        return Ok(ApiResponse<PagedResult<DeviceReportRowDto>>.CreateSuccess(data));
    }

    [HttpGet("devices/export")]
    public async Task<IActionResult> ExportDevices()
    {
        var bytes = await _reportService.ExportDevicesExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Danh-sach-thiet-bi.xlsx");
    }

    [HttpGet("assets/export")]
    public async Task<IActionResult> ExportAssets()
    {
        var bytes = await _reportService.ExportAssetsExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bao-cao-tai-san-thiet-bi.xlsx");
    }

    [HttpGet("devices/expiring/export")]
    public async Task<IActionResult> ExportExpiring([FromQuery] int withinDays = 90)
    {
        var bytes = await _reportService.ExportExpiringInspectionExcelAsync(withinDays);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Thiet-bi-sap-het-han-kiem-dinh.xlsx");
    }

    [HttpGet("events/export")]
    public async Task<IActionResult> ExportEvents()
    {
        var bytes = await _reportService.ExportEventsExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Su-co-yeu-cau.xlsx");
    }

    [HttpGet("warehouse/export")]
    public async Task<IActionResult> ExportWarehouse([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var start = from?.Date ?? DateTime.Today.AddMonths(-1);
        var end = to?.Date ?? DateTime.Today;
        var bytes = await _warehouse.ExportStockReportExcelAsync(start, end);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bao-cao-kho-TVT.xlsx");
    }

    /// <summary>viewrp.aspx?type=0 — Phiếu ghi nhận tình trạng thiết bị (PSC / rpPhieusuachua)</summary>
    [HttpGet("devices/{id:int}/print/repair")]
    public async Task<IActionResult> PrintRepair(int id)
    {
        var html = await _reportService.GetDeviceRepairPrintHtmlAsync(id);
        if (html is null) return NotFound();
        return Content(html, "text/html; charset=utf-8");
    }

    /// <summary>viewrp.aspx?type=1 — Biên bản bàn giao (BCKT / prBCKT)</summary>
    [HttpGet("devices/{id:int}/print/handover")]
    public async Task<IActionResult> PrintHandover(int id)
    {
        var html = await _reportService.GetDeviceHandoverPrintHtmlAsync(id);
        if (html is null) return NotFound();
        return Content(html, "text/html; charset=utf-8");
    }

    /// <summary>Alias cũ — cùng biên bản bàn giao BCKT.</summary>
    [HttpGet("devices/{id:int}/print/inspection")]
    public async Task<IActionResult> PrintInspection(int id) => await PrintHandover(id);

    /// <summary>viewrp.aspx?type=2 — Phiếu ghi nhận xử lý / nghiệm thu (PNT / rpPhieuNT)</summary>
    [HttpGet("devices/{id:int}/print/acceptance")]
    public async Task<IActionResult> PrintAcceptance(int id)
    {
        var html = await _reportService.GetDeviceAcceptancePrintHtmlAsync(id);
        if (html is null) return NotFound();
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpGet("baocao/assets")]
    public async Task<ActionResult<ApiResponse<PagedResult<BaocaoAssetRowDto>>>> GetBaocaoAssets(
        [FromQuery] BaocaoAssetQueryParams query)
    {
        var data = await _reportService.GetBaocaoAssetStatisticsAsync(query);
        return Ok(ApiResponse<PagedResult<BaocaoAssetRowDto>>.CreateSuccess(data));
    }

    [HttpGet("baocao/assets/export")]
    public async Task<IActionResult> ExportBaocaoAssets([FromQuery] BaocaoAssetQueryParams query)
    {
        var bytes = await _reportService.ExportBaocaoAssetStatisticsExcelAsync(query);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bao-cao-tai-san.xlsx");
    }

    [HttpGet("baocao/operational")]
    public async Task<ActionResult<ApiResponse<PagedResult<BaocaoOperationalRowDto>>>> GetBaocaoOperational(
        [FromQuery] BaocaoOperationalQueryParams query)
    {
        var data = await _reportService.GetBaocaoOperationalStatisticsAsync(query);
        return Ok(ApiResponse<PagedResult<BaocaoOperationalRowDto>>.CreateSuccess(data));
    }

    [HttpGet("baocao/operational/export")]
    public async Task<IActionResult> ExportBaocaoOperational([FromQuery] BaocaoOperationalQueryParams query)
    {
        var bytes = await _reportService.ExportBaocaoOperationalStatisticsExcelAsync(query);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bao-cao-chuyen-mon.xlsx");
    }

    [HttpGet("baocao/usage/amount-options")]
    public async Task<ActionResult<ApiResponse<List<BaocaoAmountOptionDto>>>> GetUsageAmountOptions(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var start = from?.Date ?? DateTime.Today.AddDays(-30);
        var end = to?.Date ?? DateTime.Today.AddDays(1);
        var data = await _reportService.GetBaocaoUsageAmountOptionsAsync(start, end);
        return Ok(ApiResponse<List<BaocaoAmountOptionDto>>.CreateSuccess(data));
    }
}