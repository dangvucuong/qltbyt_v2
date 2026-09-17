using Microsoft.AspNetCore.Authorization;
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
