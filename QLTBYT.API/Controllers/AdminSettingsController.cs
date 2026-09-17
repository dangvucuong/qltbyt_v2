using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Master;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/admin/master")]
[Authorize(Policy = "AdminOnly")]
public class AdminSettingsController : ControllerBase
{
    private readonly IAdminMasterDataService _adminMaster;

    public AdminSettingsController(IAdminMasterDataService adminMaster)
    {
        _adminMaster = adminMaster;
    }

    [HttpPost("device-types")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateType([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateDeviceTypeAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("device-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateType(int id, [FromBody] MasterNameRequest request)
    {
        var item = await _adminMaster.UpdateDeviceTypeAsync(id, request.Name);
        if (item is null)
            return NotFound(ApiResponse<MasterItemDto>.CreateFail("Khong tim thay loai TB."));
        return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item));
    }

    [HttpDelete("device-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteType(int id)
    {
        var ok = await _adminMaster.DeleteDeviceTypeAsync(id);
        if (!ok) return NotFound(ApiResponse<object>.CreateFail("Khong tim thay loai TB."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }));
    }

    [HttpPost("device-statuses")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateStatus([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateDeviceStatusAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("device-statuses/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateStatus(int id, [FromBody] MasterNameRequest request)
    {
        var item = await _adminMaster.UpdateDeviceStatusAsync(id, request.Name);
        if (item is null)
            return NotFound(ApiResponse<MasterItemDto>.CreateFail("Khong tim thay trang thai."));
        return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item));
    }

    [HttpDelete("device-statuses/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStatus(int id)
    {
        var ok = await _adminMaster.DeleteDeviceStatusAsync(id);
        if (!ok) return NotFound(ApiResponse<object>.CreateFail("Khong tim thay trang thai."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }));
    }
}

public class MasterNameRequest
{
    public string Name { get; set; } = string.Empty;
}
