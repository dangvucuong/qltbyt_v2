using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.User;
using QLTBYT.API.Services;

namespace QLTBYT.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserAccountService _userService;

    public UsersController(IUserAccountService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserListDto>>>> GetUsers()
    {
        var list = await _userService.GetUsersAsync();
        return Ok(ApiResponse<IReadOnlyList<UserListDto>>.CreateSuccess(list));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserListDto>>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        if (user is null)
            return NotFound(ApiResponse<UserListDto>.CreateFail("Khong tim thay tai khoan."));
        return Ok(ApiResponse<UserListDto>.CreateSuccess(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserListDto>>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            return Ok(ApiResponse<UserListDto>.CreateSuccess(user, "Tao tai khoan thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserListDto>.CreateFail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserListDto>>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            if (user is null)
                return NotFound(ApiResponse<UserListDto>.CreateFail("Khong tim thay tai khoan."));
            return Ok(ApiResponse<UserListDto>.CreateSuccess(user, "Cap nhat tai khoan thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserListDto>.CreateFail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
    {
        try
        {
            var ok = await _userService.DeleteUserAsync(id);
            if (!ok)
                return NotFound(ApiResponse<object>.CreateFail("Khong tim thay tai khoan."));
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Xoa tai khoan thanh cong."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFail(ex.Message));
        }
    }
}

