using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Audit;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/system-logs")]
[Authorize(Policy = "AdminOnly")]
public class SystemAuditLogsController : ControllerBase
{
    private readonly ISystemAuditService _auditService;

    public SystemAuditLogsController(ISystemAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>Xem nhật ký hệ thống (LogHethong).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SystemAuditLogDto>>>> GetLogs(
        [FromQuery] SystemAuditQueryParams query)
    {
        var result = await _auditService.GetLogsAsync(query);
        return Ok(ApiResponse<PagedResult<SystemAuditLogDto>>.CreateSuccess(result));
    }
}
