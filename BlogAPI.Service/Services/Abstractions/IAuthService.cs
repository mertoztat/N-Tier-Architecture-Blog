using BlogAPI.Entity.Dtos.Auth;

namespace BlogAPI.Service.Services.Abstractions;

public interface IAuthService
{
    /// <summary>Returns null when the credentials are wrong or the account is locked.</summary>
    Task<AuthResultDto?> LoginAsync(LoginDto loginDto);

    /// <summary>Rotates the refresh token. Returns null when it is invalid, expired or already used.</summary>
    Task<AuthResultDto?> RefreshAsync(string refreshToken);

    /// <summary>Revokes the given refresh token. Safe to call with an unknown token.</summary>
    Task LogoutAsync(string refreshToken);
}
