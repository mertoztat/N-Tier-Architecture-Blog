using BlogAPI.Core.Entities;

namespace BlogAPI.Entity.Entities;

public class RefreshToken : IEntityBase
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>SHA-256 hash of the token. The raw token is only sent to the client, never stored.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash of the token that replaced this one (refresh token rotation).</summary>
    public string? ReplacedByTokenHash { get; set; }

    public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
