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
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("Không tìm thấy hồ sơ pháp lý."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(item));
    }

    [HttpPost]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> Create([FromBody] CreateLegalDocumentRequest request)
    {
        var item = await _legalService.CreateAsync(request);
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(item, "Thêm hồ sơ pháp lý thành công."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> Update(int id, [FromBody] UpdateLegalDocumentRequest request)
    {
        var item = await _legalService.UpdateAsync(id, request);
        if (item is null)
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("Không tìm thấy hồ sơ pháp lý."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(item, "Cập nhật hồ sơ thành công."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var ok = await _legalService.DeleteAsync(id);
        if (!ok)
            return NotFound(ApiResponse<object>.CreateFail("Không tìm thấy hồ sơ pháp lý."));
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xóa hồ sơ thành công."));
    }

    [HttpPost("{id:int}/file")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<LegalDocumentDto>>> UploadFile(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<LegalDocumentDto>.CreateFail("File không hợp lệ."));
        await using var stream = file.OpenReadStream();
        var item = await _legalService.UploadFileAsync(id, stream, file.FileName);
        if (item is null)
            return NotFound(ApiResponse<LegalDocumentDto>.CreateFail("Không tìm thấy hồ sơ pháp lý."));
        return Ok(ApiResponse<LegalDocumentDto>.CreateSuccess(item, "Tải file hồ sơ thành công."));
    }
}