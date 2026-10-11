using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;

namespace QLTBYT.API.Controllers;

/// <summary>
/// Legacy NhatkyTB / CaptureNhatky — deprecated. Use /api/usage-logs and device events.
/// </summary>
[ApiController]
[Route("api/legacy/nhatky-thietbi")]
[Authorize(Policy = "AllUsers")]
public class LegacyUsageJournalController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> GetDeprecatedInfo()
    {
        return Ok(ApiResponse<object>.CreateSuccess(new
        {
            deprecated = true,
            message = "Nhật ký text/ảnh legacy (NhatkyTB.aspx, CaptureNhatky) không được hỗ trợ trên API mới.",
            replacement = new
            {
                usageLogsByDate = "/api/usage-logs",
                deviceEvents = "/api/events",
                uiUsageLogs = "/usage-logs"
            },
            since = "Phase 2.6"
        }));
    }

    [HttpPost("capture")]
    public ActionResult<ApiResponse<object>> CaptureDeprecated()
    {
        return BadRequest(ApiResponse<object>.CreateFail(
            "Chức năng chụp ảnh nhật ký legacy đã deprecated. Sử dụng POST /api/usage-logs hoặc báo sự cố thiết bị."));
    }

    [HttpPost]
    public ActionResult<ApiResponse<object>> InsertDeprecated()
    {
        return BadRequest(ApiResponse<object>.CreateFail(
            "InsertNhatkyTB legacy da deprecated. Su dung POST /api/usage-logs."));
    }
}
