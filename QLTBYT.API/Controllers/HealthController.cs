using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Health;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDatabaseHealthService _databaseHealthService;

    public HealthController(IDatabaseHealthService databaseHealthService)
    {
        _databaseHealthService = databaseHealthService;
    }

    /// <summary>Ping nhanh — kiểm tra IIS/API đã lên.</summary>
    [AllowAnonymous]
    [HttpGet]
    [HttpGet("ping")]
    public ActionResult<ApiResponse<object>> Ping()
    {
        return Ok(ApiResponse<object>.CreateSuccess(
            new { status = "ok", utc = DateTime.UtcNow },
            "API đang chạy."));
    }

    [AllowAnonymous]
    [HttpGet("database")]
    [ProducesResponseType(typeof(ApiResponse<DatabaseHealthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DatabaseHealthDto>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResponse<DatabaseHealthDto>>> GetDatabaseHealth()
    {
        var health = await _databaseHealthService.CheckAsync();

        if (!health.CanConnect || !health.CoreTablesPresent)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                ApiResponse<DatabaseHealthDto>.CreateFail(
                    "Database chưa sẵn sàng hoặc thiếu bảng core.",
                    health.MissingCoreTables.ToList()));
        }

        return Ok(ApiResponse<DatabaseHealthDto>.CreateSuccess(health, "Database sẵn sàng."));
    }
}