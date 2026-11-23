using BlogAPI.Entity.Dtos.Auth;
using BlogAPI.Service.Extensions;
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
            return Problem(detail: "Bu e-posta adresi zaten kayıtlı.", statusCode: StatusCodes.Status409Conflict);

        if (!result.Succeeded)
        {
            // Identity errors (e.g. password rules) in the same "errors" format as FluentValidation.
            foreach (var error in result.Errors)
                ModelState.AddModelError("password", error);
            return ValidationProblem(ModelState);
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
            return Problem(detail: "E-posta veya şifre hatalı.", statusCode: StatusCodes.Status401Unauthorized);

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
            return Problem(detail: "Oturum bilgisi bulunamadı. Lütfen tekrar giriş yapın.", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _authService.RefreshAsync(refreshToken);
        if (result is null)
        {
            DeleteRefreshTokenCookie();
            return Problem(detail: "Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.", statusCode: StatusCodes.Status401Unauthorized);
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
            Id = User.GetLoggedInUserId(),
            Name = User.GetLoggedInUserName(),
            Email = User.GetLoggedInUserEmail(),
            Roles = User.GetLoggedInUserRoles()
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
