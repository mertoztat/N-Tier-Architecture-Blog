using System.Security.Claims;
using BlogAPI.Core.Services;
using BlogAPI.Service.Extensions;

namespace BlogAPI.Services;

/// <summary>
/// Gives services the current user without making them depend on HttpContext.
/// Reading the claims themselves is done by LoggedInUserExtensions.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => User?.GetLoggedInUserId();

    public string? UserName => User?.GetLoggedInUserName();

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
