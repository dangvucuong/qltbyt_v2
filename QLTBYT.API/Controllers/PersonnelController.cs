using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Personnel;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/personnel")]
[Authorize(Policy = "TechnicianAndAbove")]
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelService _personnelService;

    public PersonnelController(IPersonnelService personnelService)
    {
        _personnelService = personnelService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PersonnelDto>>>> GetAll()
    {
        var list = await _personnelService.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<PersonnelDto>>.CreateSuccess(list));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PersonnelDto>>> GetById(int id)
    {
        var item = await _personnelService.GetByIdAsync(id);
        if (item is null)
            return NotFound(ApiResponse<PersonnelDto>.CreateFail("Không tìm thấy nhân sự."));
        return Ok(ApiResponse<PersonnelDto>.CreateSuccess(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PersonnelDto>>> Create([FromBody] CreatePersonnelRequest request)
    {
        try
        {
            var item = await _personnelService.CreateAsync(request);
            return Ok(ApiResponse<PersonnelDto>.CreateSuccess(item, "Thêm nhân sự thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PersonnelDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PersonnelDto>>> Update(int id, [FromBody] UpdatePersonnelRequest request)
    {
        var item = await _personnelService.UpdateAsync(id, request);
        if (item is null)
            return NotFound(ApiResponse<PersonnelDto>.CreateFail("Không tìm thấy nhân sự."));
        return Ok(ApiResponse<PersonnelDto>.CreateSuccess(item, "Cập nhật nhân sự thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var ok = await _personnelService.DeleteAsync(id);
        if (!ok)
            return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy nhân sự."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa nhân sự thành công."));
    }
}

