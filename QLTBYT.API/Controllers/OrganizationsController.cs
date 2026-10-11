using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Organization;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize(Policy = "AllUsers")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrganizations()
    {
        var list = await _organizationService.GetOrganizationsAsync();
        return Ok(ApiResponse<List<OrganizationListDto>>.CreateSuccess(list));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrganization(int id)
    {
        var item = await _organizationService.GetOrganizationAsync(id);
        if (item is null)
            return NotFound(ApiResponse<OrganizationListDto>.CreateFail("Không tìm thấy đơn vị."));
        return Ok(ApiResponse<OrganizationListDto>.CreateSuccess(item));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
    {
        try
        {
            var item = await _organizationService.CreateOrganizationAsync(request);
            return Ok(ApiResponse<OrganizationListDto>.CreateSuccess(item, "Thêm đơn vị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<OrganizationListDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] UpdateOrganizationRequest request)
    {
        var item = await _organizationService.UpdateOrganizationAsync(id, request);
        if (item is null)
            return NotFound(ApiResponse<OrganizationListDto>.CreateFail("Không tìm thấy đơn vị."));
        return Ok(ApiResponse<OrganizationListDto>.CreateSuccess(item, "Cập nhật đơn vị thành công."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        try
        {
            var ok = await _organizationService.DeleteOrganizationAsync(id);
            if (!ok)
                return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy đơn vị."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa đơn vị thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }
}
