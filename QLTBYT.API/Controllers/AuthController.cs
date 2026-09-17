using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService authService, IWebHostEnvironment environment)
    {
        _authService = authService;
        _environment = environment;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.CreateFail("Sai ten dang nhap hoac mat khau."));
        }

        return Ok(ApiResponse<LoginResponse>.CreateSuccess(result, "Dang nhap thanh cong."));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> Me()
    {
        var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : 0;
        if (userId <= 0)
        {
            return Unauthorized(ApiResponse<UserInfoDto>.CreateFail("Khong hop le."));
        }

        var info = await _authService.GetUserInfoAsync(userId);
        if (info is null)
        {
            return NotFound(ApiResponse<UserInfoDto>.CreateFail("Khong tim thay tai khoan."));
        }

        return Ok(ApiResponse<UserInfoDto>.CreateSuccess(info));
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : 0;
        if (userId <= 0)
        {
            return Unauthorized(ApiResponse<object>.CreateFail("Khong hop le."));
        }

        var ok = await _authService.ChangePasswordAsync(userId, request);
        if (!ok)
        {
            return BadRequest(ApiResponse<object>.CreateFail("Mat khau cu khong dung."));
        }

        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Doi mat khau thanh cong."));
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponse>>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request, _environment.IsDevelopment());
        if (result is null)
        {
            return NotFound(ApiResponse<ForgotPasswordResponse>.CreateFail("Khong tim thay tai khoan."));
        }

        return Ok(ApiResponse<ForgotPasswordResponse>.CreateSuccess(result));
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var ok = await _authService.ResetPasswordAsync(request);
        if (!ok)
        {
            return BadRequest(ApiResponse<object>.CreateFail("Ma dat lai khong hop le hoac da het han."));
        }

        return Ok(ApiResponse<object>.CreateSuccess(new { }, "Dat lai mat khau thanh cong."));
    }
}
