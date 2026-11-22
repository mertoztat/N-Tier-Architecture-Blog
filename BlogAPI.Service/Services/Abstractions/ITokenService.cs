using BlogAPI.Entity.Entities;

namespace BlogAPI.Service.Services.Abstractions;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(AppUser user, IEnumerable<string> roles);

    /// <summary>Creates a cryptographically random refresh token (raw value, sent to the client).</summary>
    string CreateRefreshToken();

    /// <summary>SHA-256 hash used to store refresh tokens. The raw token is never persisted.</summary>
    string HashToken(string token);
}
