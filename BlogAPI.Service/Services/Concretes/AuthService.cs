using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Dtos.Auth;
using BlogAPI.Entity.Entities;
using BlogAPI.Service.Options;
using BlogAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BlogAPI.Service.Services.Concretes;

public class AuthService : IAuthService
{
    private const string DefaultRole = "User";

    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwt;

    public AuthService(
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _jwt = jwtOptions.Value;
    }

    public async Task<RegisterResultDto> RegisterAsync(RegisterDto registerDto)
    {
        var email = registerDto.Email.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
            return new RegisterResultDto { EmailTaken = true, Errors = new[] { "Bu e-posta adresi zaten kayıtlı." } };

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = registerDto.FirstName.Trim(),
            LastName = registerDto.LastName.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, registerDto.Password);
        if (!createResult.Succeeded)
            return new RegisterResultDto { Errors = createResult.Errors.Select(e => e.Description).ToList() };

        var roleResult = await _userManager.AddToRoleAsync(user, DefaultRole);
        if (!roleResult.Succeeded)
        {
            // Do not leave a user without a role behind.
            await _userManager.DeleteAsync(user);
            return new RegisterResultDto { Errors = roleResult.Errors.Select(e => e.Description).ToList() };
        }

        return new RegisterResultDto { Succeeded = true, UserId = user.Id };
    }

    public async Task<AuthResultDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user is null)
            return null;

        if (await _userManager.IsLockedOutAsync(user))
            return null;

        if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return null;
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var result = await CreateTokensAsync(user);
        await _unitOfWork.SaveAsync();
        return result;
    }

    public async Task<AuthResultDto?> RefreshAsync(string refreshToken)
    {
        var repo = _unitOfWork.GetRepository<RefreshToken>();
        var tokenHash = _tokenService.HashToken(refreshToken);

        var stored = await repo.GetAsync(t => t.TokenHash == tokenHash);
        if (stored is null)
            return null;

        // A revoked token being presented again means it was probably stolen:
        // revoke every active token of this user so both the thief and the owner must log in again.
        if (stored.IsRevoked)
        {
            await RevokeAllActiveTokensAsync(stored.UserId);
            await _unitOfWork.SaveAsync();
            return null;
        }

        if (stored.IsExpired)
            return null;

        var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null || await _userManager.IsLockedOutAsync(user))
            return null;

        var result = await CreateTokensAsync(user);

        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenHash = _tokenService.HashToken(result.RefreshToken);

        await _unitOfWork.SaveAsync();
        return result;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var repo = _unitOfWork.GetRepository<RefreshToken>();
        var tokenHash = _tokenService.HashToken(refreshToken);

        var stored = await repo.GetAsync(t => t.TokenHash == tokenHash);
        if (stored is null || stored.IsRevoked)
            return;

        stored.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();
    }

    private async Task<AuthResultDto> CreateTokensAsync(AppUser user)
    {
        var now = DateTime.UtcNow;
        var refreshExpires = now.AddDays(_jwt.RefreshTokenDays);

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, accessExpires) = _tokenService.CreateAccessToken(user, roles);

        var rawRefreshToken = _tokenService.CreateRefreshToken();

        await _unitOfWork.GetRepository<RefreshToken>().AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(rawRefreshToken),
            CreatedAt = now,
            ExpiresAt = refreshExpires
        });

        return new AuthResultDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpires,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpiresAt = refreshExpires
        };
    }

    private async Task RevokeAllActiveTokensAsync(Guid userId)
    {
        var repo = _unitOfWork.GetRepository<RefreshToken>();
        var now = DateTime.UtcNow;

        var active = await repo.GetAllAsync(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now);
        foreach (var token in active)
        {
            token.RevokedAt = now;
            repo.Update(token);
        }
    }
}
