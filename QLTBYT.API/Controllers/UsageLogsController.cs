using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.UsageLog;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/usage-logs")]
[Authorize(Policy = "AllUsers")]
public class UsageLogsController : ControllerBase
{
    private readonly IUsageLogService _usageLogService;

    public UsageLogsController(IUsageLogService usageLogService)
    {
        _usageLogService = usageLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UsageLogDto>>>> GetLogs([FromQuery] UsageLogQueryParams query)
    {
        var list = await _usageLogService.GetLogsAsync(query);
        return Ok(ApiResponse<IReadOnlyList<UsageLogDto>>.CreateSuccess(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UsageLogDto>>> Create([FromBody] CreateUsageLogRequest request)
    {
        try
        {
            var item = await _usageLogService.CreateAsync(request);
            return Ok(ApiResponse<UsageLogDto>.CreateSuccess(item, "Ghi nhat ky thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UsageLogDto>.CreateFail(ex.Message));
        }
    }
}

