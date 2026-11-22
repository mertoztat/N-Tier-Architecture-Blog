namespace BlogAPI.Entity.Dtos.Auth;

/// <summary>What the client receives in the response body. The refresh token is sent as an HttpOnly cookie.</summary>
public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
}
