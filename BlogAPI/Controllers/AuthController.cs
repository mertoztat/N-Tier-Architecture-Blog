using System.Security.Claims;
using BlogAPI.Entity.Dtos.Auth;
using BlogAPI.Service.Options;
using BlogAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookie = "refreshToken";
    private const string CookiePath = "/api/auth";

    private readonly IAuthService _authService;
    private readonly JwtSettings _jwt;

    public AuthController(IAuthService authService, IOptions<JwtSettings> jwtOptions)
    {
        _authService = authService;
        _jwt = jwtOptions.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);

        if (result.EmailTaken)
            return Conflict(new ProblemDetails { Title = "E-mail address is already registered.", Status = 409 });

        if (!result.Succeeded)
        {
            var problem = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errors"] = result.Errors.ToArray()
            }) { Status = 400 };
            return BadRequest(problem);
        }

        return StatusCode(StatusCodes.Status201Created, new { result.UserId });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result is null)
            return Unauthorized(new ProblemDetails { Title = "Invalid email or password.", Status = 401 });

        SetRefreshTokenCookie(result);
        return Ok(ToTokenDto(result));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new ProblemDetails { Title = "Refresh token is missing.", Status = 401 });

        var result = await _authService.RefreshAsync(refreshToken);
        if (result is null)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(new ProblemDetails { Title = "Refresh token is invalid or expired.", Status = 401 });
        }

        SetRefreshTokenCookie(result);
        return Ok(ToTokenDto(result));
    }

    // AllowAnonymous on purpose: logging out must work even if the access token has already expired.
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
            await _authService.LogoutAsync(refreshToken);

        DeleteRefreshTokenCookie();
        return NoContent();
    }

    /// <summary>Returns the identity inside the access token. Handy for the React app to check the session.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Name = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
        });
    }

    private static TokenDto ToTokenDto(AuthResultDto result) => new()
    {
        AccessToken = result.AccessToken,
        AccessTokenExpiresAt = result.AccessTokenExpiresAt
    };

    private void SetRefreshTokenCookie(AuthResultDto result)
    {
        Response.Cookies.Append(RefreshTokenCookie, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiePath,
            Expires = result.RefreshTokenExpiresAt
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiePath
        });
    }
}
