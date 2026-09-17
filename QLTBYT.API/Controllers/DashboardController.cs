using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Dashboard;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "AllUsers")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary([FromQuery] int expiringWithinDays = 30)
    {
        var data = await _dashboardService.GetSummaryAsync(expiringWithinDays);
        return Ok(ApiResponse<DashboardSummaryDto>.CreateSuccess(data));
    }
}
