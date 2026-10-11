using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Event;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/events")]
[Authorize(Policy = "AllUsers")]
public class EventsController : ControllerBase
{
    private readonly IDeviceEventService _eventService;

    public EventsController(IDeviceEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<EventListDto>>>> GetEvents(
        [FromQuery] EventQueryParams query)
    {
        var result = await _eventService.GetEventsAsync(query);
        return Ok(ApiResponse<PagedResult<EventListDto>>.CreateSuccess(result));
    }

    [HttpGet("keywords")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventKeywordDto>>>> GetKeywords()
    {
        var list = await _eventService.GetKeywordsAsync();
        return Ok(ApiResponse<IReadOnlyList<EventKeywordDto>>.CreateSuccess(list));
    }

    [HttpPost("keywords")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<EventKeywordDto>>> AddKeyword([FromBody] CreateEventKeywordRequest request)
    {
        try
        {
            var dto = await _eventService.AddKeywordAsync(request);
            return Ok(ApiResponse<EventKeywordDto>.CreateSuccess(dto, "Thêm từ khóa thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EventKeywordDto>.CreateFail(ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> GetEvent(int id)
    {
        var item = await _eventService.GetEventByIdAsync(id);
        if (item is null)
        {
            return NotFound(ApiResponse<EventDetailDto>.CreateFail("Không tìm thấy sự cố/yêu cầu."));
        }

        return Ok(ApiResponse<EventDetailDto>.CreateSuccess(item));
    }

    [HttpGet("{id:int}/files")]
    public async Task<ActionResult<ApiResponse<List<DeviceFileDto>>>> GetEventFiles(int id)
    {
        var list = await _eventService.GetIncidentFilesForEventAsync(id);
        return Ok(ApiResponse<List<DeviceFileDto>>.CreateSuccess(list));
    }

    [HttpPost("{id:int}/files")]
    [Authorize(Policy = "TechnicianAndAbove")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<DeviceFileDto>>> UploadEventFile(
        int id,
        IFormFile file,
        [FromForm] string kind = "report")
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail("File không hợp lệ."));

        try
        {
            await using var stream = file.OpenReadStream();
            var dto = await _eventService.AddIncidentFileForEventAsync(id, stream, file.FileName, kind);
            return Ok(ApiResponse<DeviceFileDto>.CreateSuccess(dto, "Upload file sự cố thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DeviceFileDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "TechnicianAndAbove")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> ResolveEvent(
        int id,
        [FromBody] ResolveEventRequest request)
    {
        var item = await _eventService.ResolveAsync(id, request);
        if (item is null)
        {
            return NotFound(ApiResponse<EventDetailDto>.CreateFail("Không tìm thấy sự cố/yêu cầu."));
        }

        return Ok(ApiResponse<EventDetailDto>.CreateSuccess(item, "Cập nhật xử lý thành công."));
    }
}
