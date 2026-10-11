using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Warehouse;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/warehouse")]
[Authorize(Policy = "TechnicianAndAbove")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouse;

    public WarehouseController(IWarehouseService warehouse)
    {
        _warehouse = warehouse;
    }

    [HttpGet("commodities")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CommodityDto>>>> GetCommodities()
    {
        var list = await _warehouse.GetCommoditiesAsync();
        return Ok(ApiResponse<IReadOnlyList<CommodityDto>>.CreateSuccess(list));
    }

    [HttpPost("commodities")]
    public async Task<ActionResult<ApiResponse<CommodityDto>>> CreateCommodity([FromBody] CreateCommodityRequest request)
    {
        var item = await _warehouse.CreateCommodityAsync(request);
        return Ok(ApiResponse<CommodityDto>.CreateSuccess(item, "Thêm hàng hóa thành công."));
    }

    [HttpGet("batches")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BatchDto>>>> GetBatches()
    {
        var list = await _warehouse.GetBatchesAsync();
        return Ok(ApiResponse<IReadOnlyList<BatchDto>>.CreateSuccess(list));
    }

    [HttpPost("batches")]
    public async Task<ActionResult<ApiResponse<BatchDto>>> CreateBatch([FromBody] CreateBatchRequest request)
    {
        var item = await _warehouse.CreateBatchAsync(request);
        return Ok(ApiResponse<BatchDto>.CreateSuccess(item, "Thêm lô hàng thành công."));
    }

    [HttpGet("imports")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StockImportDto>>>> GetImports()
    {
        var list = await _warehouse.GetImportsAsync();
        return Ok(ApiResponse<IReadOnlyList<StockImportDto>>.CreateSuccess(list));
    }

    [HttpPost("imports")]
    public async Task<ActionResult<ApiResponse<StockImportDto>>> Import([FromBody] CreateStockImportRequest request)
    {
        try
        {
            var item = await _warehouse.ImportStockAsync(request);
            return Ok(ApiResponse<StockImportDto>.CreateSuccess(item, "Nhập kho thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<StockImportDto>.CreateFail(ex.Message));
        }
    }

    [HttpGet("exports")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StockExportDto>>>> GetExports()
    {
        var list = await _warehouse.GetExportsAsync();
        return Ok(ApiResponse<IReadOnlyList<StockExportDto>>.CreateSuccess(list));
    }

    [HttpPost("exports")]
    public async Task<ActionResult<ApiResponse<StockExportDto>>> Export([FromBody] CreateStockExportRequest request)
    {
        try
        {
            var item = await _warehouse.ExportStockAsync(request);
            return Ok(ApiResponse<StockExportDto>.CreateSuccess(item, "Xuất kho thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<StockExportDto>.CreateFail(ex.Message));
        }
    }

    [HttpGet("report")]
    public async Task<ActionResult<ApiResponse<WarehouseReportDto>>> Report()
    {
        var data = await _warehouse.GetReportAsync();
        return Ok(ApiResponse<WarehouseReportDto>.CreateSuccess(data));
    }
}