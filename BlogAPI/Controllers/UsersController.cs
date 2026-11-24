using BlogAPI.Entity.Dtos.Users;
using BlogAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Controllers;

// Admin and Superadmin only. What each of them may do with Admin/Superadmin accounts
// is enforced in UserService (Admin can only create and delete plain "User" accounts).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Superadmin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserAddDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
