using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Bidding;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/bidding")]
[Authorize]
public class BiddingController : ControllerBase
{
    private readonly IBiddingService _biddingService;

    public BiddingController(IBiddingService biddingService)
    {
        _biddingService = biddingService;
    }

    [HttpGet]
    [Authorize(Policy = "AllUsers")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiddingPackageDto>>>> GetAll()
    {
        var list = await _biddingService.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<BiddingPackageDto>>.CreateSuccess(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "AllUsers")]
    public async Task<ActionResult<ApiResponse<BiddingPackageDto>>> GetById(int id)
    {
        var item = await _biddingService.GetByIdAsync(id);
        if (item is null)
            return NotFound(ApiResponse<BiddingPackageDto>.CreateFail("Không tìm thấy gói thầu."));
        return Ok(ApiResponse<BiddingPackageDto>.CreateSuccess(item));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<BiddingPackageDto>>> Create([FromBody] CreateBiddingPackageRequest request)
    {
        try
        {
            var item = await _biddingService.CreateAsync(request);
            return Ok(ApiResponse<BiddingPackageDto>.CreateSuccess(item, "Thêm gói thầu thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<BiddingPackageDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<BiddingPackageDto>>> Update(int id, [FromBody] UpdateBiddingPackageRequest request)
    {
        var item = await _biddingService.UpdateAsync(id, request);
        if (item is null)
            return NotFound(ApiResponse<BiddingPackageDto>.CreateFail("Không tìm thấy gói thầu."));
        return Ok(ApiResponse<BiddingPackageDto>.CreateSuccess(item, "Cập nhật gói thầu thành công."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var ok = await _biddingService.DeleteAsync(id);
        if (!ok)
            return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy gói thầu."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa gói thầu thành công."));
    }

    [HttpGet("{id:int}/devices")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DeviceListDto>>>> GetDevices(int id)
    {
        var list = await _biddingService.GetDevicesAsync(id);
        return Ok(ApiResponse<IReadOnlyList<DeviceListDto>>.CreateSuccess(list));
    }
}
