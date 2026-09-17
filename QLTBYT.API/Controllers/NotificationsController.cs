using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Notification;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Policy = "AllUsers")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>>> GetNotifications()
    {
        var list = await _notificationService.GetForCurrentUserAsync();
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.CreateSuccess(list));
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead(int id)
    {
        await _notificationService.MarkReadAsync(id);
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Da danh dau da doc."));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllRead()
    {
        await _notificationService.MarkAllReadAsync();
        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Da danh dau tat ca da doc."));
    }
}

