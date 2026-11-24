using BlogAPI.Entity.Dtos.Profile;
using BlogAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Controllers;

// Every logged-in user can read their profile and change their own password and photo (name and e-mail are
// not editable by the user); there is no id in the routes on purpose,
// the user always comes from the access token.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _profileService.GetProfileAsync());

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        await _profileService.ChangePasswordAsync(dto);
        return NoContent();
    }

    [HttpPut("photo")]
    public async Task<IActionResult> UpdatePhoto([FromForm] ProfilePhotoDto dto) =>
        Ok(await _profileService.UpdatePhotoAsync(dto));

    [HttpDelete("photo")]
    public async Task<IActionResult> RemovePhoto()
    {
        await _profileService.RemovePhotoAsync();
        return NoContent();
    }
}
