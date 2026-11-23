using System.Security.Claims;

namespace BlogAPI.Service.Extensions;

/// <summary>
/// The single place that knows which claim holds which piece of user information
/// (the claims are created in TokenService.CreateAccessToken).
/// Usage: <c>User.GetLoggedInUserId()</c> in a controller, or <c>principal.GetLoggedInUserName()</c>.
/// Every method returns null / empty instead of throwing when the user is anonymous.
/// </summary>
public static class LoggedInUserExtensions
{
    public static Guid? GetLoggedInUserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>"FirstName LastName"</summary>
    public static string? GetLoggedInUserName(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.Name);

    public static string? GetLoggedInUserEmail(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.Email);

    public static IReadOnlyList<string> GetLoggedInUserRoles(this ClaimsPrincipal principal)
        => principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
}
