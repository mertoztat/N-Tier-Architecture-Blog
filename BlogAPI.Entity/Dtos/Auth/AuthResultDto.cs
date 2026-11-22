namespace BlogAPI.Entity.Dtos.Auth;

/// <summary>Internal result of login/refresh. The controller puts the refresh token into a cookie.</summary>
public class AuthResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
