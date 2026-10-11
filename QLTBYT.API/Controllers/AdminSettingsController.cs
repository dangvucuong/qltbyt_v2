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

    // --- Loại thiết bị ---
    [HttpPost("device-types")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateType([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateDeviceTypeAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Thêm loại thiết bị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("device-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateType(int id, [FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.UpdateDeviceTypeAsync(id, request.Name);
            if (item is null)
                return NotFound(ApiResponse<MasterItemDto>.CreateFail("Không tìm thấy loại thiết bị."));
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Cập nhật loại thiết bị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("device-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteType(int id)
    {
        try
        {
            var ok = await _adminMaster.DeleteDeviceTypeAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy loại thiết bị."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa loại thiết bị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }

    // --- Trạng thái thiết bị ---
    [HttpPost("device-statuses")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateStatus([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateDeviceStatusAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Thêm trạng thái thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("device-statuses/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateStatus(int id, [FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.UpdateDeviceStatusAsync(id, request.Name);
            if (item is null)
                return NotFound(ApiResponse<MasterItemDto>.CreateFail("Không tìm thấy trạng thái."));
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Cập nhật trạng thái thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("device-statuses/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStatus(int id)
    {
        try
        {
            var ok = await _adminMaster.DeleteDeviceStatusAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy trạng thái."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa trạng thái thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }

    // --- Loại đơn vị ---
    [HttpPost("org-types")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateOrgType([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateOrganizationTypeAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Thêm loại đơn vị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("org-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateOrgType(int id, [FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.UpdateOrganizationTypeAsync(id, request.Name);
            if (item is null)
                return NotFound(ApiResponse<MasterItemDto>.CreateFail("Không tìm thấy loại đơn vị."));
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Cập nhật loại đơn vị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("org-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOrgType(int id)
    {
        try
        {
            var ok = await _adminMaster.DeleteOrganizationTypeAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy loại đơn vị."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa loại đơn vị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }

    // --- Nước sản xuất ---
    [HttpPost("countries")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateCountry([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateCountryAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Thêm nước sản xuất thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("countries/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateCountry(int id, [FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.UpdateCountryAsync(id, request.Name);
            if (item is null)
                return NotFound(ApiResponse<MasterItemDto>.CreateFail("Không tìm thấy nước sản xuất."));
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Cập nhật nước sản xuất thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("countries/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCountry(int id)
    {
        try
        {
            var ok = await _adminMaster.DeleteCountryAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy nước sản xuất."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa nước sản xuất thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }

    // --- Loại nhiệm vụ ---
    [HttpPost("task-types")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateTaskType([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateTaskTypeAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Thêm loại nhiệm vụ thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("task-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateTaskType(int id, [FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.UpdateTaskTypeAsync(id, request.Name);
            if (item is null)
                return NotFound(ApiResponse<MasterItemDto>.CreateFail("Không tìm thấy loại nhiệm vụ."));
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Cập nhật loại nhiệm vụ thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("task-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTaskType(int id)
    {
        try
        {
            var ok = await _adminMaster.DeleteTaskTypeAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy loại nhiệm vụ."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa loại nhiệm vụ thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }

    // --- Từ khóa sự cố ---
    [HttpPost("event-keywords")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateEventKeyword([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateEventKeywordAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Thêm từ khóa thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("event-keywords/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateEventKeyword(int id, [FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.UpdateEventKeywordAsync(id, request.Name);
            if (item is null)
                return NotFound(ApiResponse<MasterItemDto>.CreateFail("Không tìm thấy từ khóa."));
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Cập nhật từ khóa thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("event-keywords/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEventKeyword(int id)
    {
        try
        {
            var ok = await _adminMaster.DeleteEventKeywordAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy từ khóa."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa từ khóa thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }

    // --- Loại nhân sự ---
    [HttpPost("personnel-types")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreatePersonnelType([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreatePersonnelTypeAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Thêm loại nhân sự thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("personnel-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdatePersonnelType(int id, [FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.UpdatePersonnelTypeAsync(id, request.Name);
            if (item is null)
                return NotFound(ApiResponse<MasterItemDto>.CreateFail("Không tìm thấy loại nhân sự."));
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Cập nhật loại nhân sự thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("personnel-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePersonnelType(int id)
    {
        try
        {
            var ok = await _adminMaster.DeletePersonnelTypeAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy loại nhân sự."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa loại nhân sự thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }

    // --- Loại hồ sơ ---
    [HttpPost("document-types")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> CreateDocumentType([FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.CreateDocumentTypeAsync(request.Name);
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Thêm loại hồ sơ thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("document-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<MasterItemDto>>> UpdateDocumentType(int id, [FromBody] MasterNameRequest request)
    {
        try
        {
            var item = await _adminMaster.UpdateDocumentTypeAsync(id, request.Name);
            if (item is null)
                return NotFound(ApiResponse<MasterItemDto>.CreateFail("Không tìm thấy loại hồ sơ."));
            return Ok(ApiResponse<MasterItemDto>.CreateSuccess(item, "Cập nhật loại hồ sơ thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MasterItemDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("document-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDocumentType(int id)
    {
        try
        {
            var ok = await _adminMaster.DeleteDocumentTypeAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy loại hồ sơ."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa loại hồ sơ thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }
}

public class MasterNameRequest
{
    public string Name { get; set; } = string.Empty;
}
