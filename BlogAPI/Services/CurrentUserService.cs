using System.Security.Claims;
using BlogAPI.Core.Services;

namespace BlogAPI.Services;

/// <summary>Reads the current user from the JWT claims created in TokenService.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
