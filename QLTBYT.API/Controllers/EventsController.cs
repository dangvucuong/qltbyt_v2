using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> GetEvent(int id)
    {
        var item = await _eventService.GetEventByIdAsync(id);
        if (item is null)
        {
            return NotFound(ApiResponse<EventDetailDto>.CreateFail("Kh\u00f4ng t\u00ecm th\u1ea5y s\u1ef1 c\u1ed1/y\u00eau c\u1ea7u."));
        }

        return Ok(ApiResponse<EventDetailDto>.CreateSuccess(item));
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
            return Ok(ApiResponse<EventKeywordDto>.CreateSuccess(dto, "Them keyword thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EventKeywordDto>.CreateFail(ex.Message));
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
            return NotFound(ApiResponse<EventDetailDto>.CreateFail("Kh\u00f4ng t\u00ecm th\u1ea5y s\u1ef1 c\u1ed1/y\u00eau c\u1ea7u."));
        }

        return Ok(ApiResponse<EventDetailDto>.CreateSuccess(item, "C\u1eadp nh\u1eadt x\u1eed l\u00fd th\u00e0nh c\u00f4ng."));
    }
}
