using BlogAPI.Entity.Dtos.Profile;

namespace BlogAPI.Service.Services.Abstractions;

/// <summary>
/// Self-service for the logged-in user. Failures are reported with exceptions from
/// BlogAPI.Core.Exceptions, not return values.
/// </summary>
public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync();

    /// <summary>Changes the password and signs the user out of every session (refresh tokens are revoked).</summary>
    Task ChangePasswordAsync(ChangePasswordDto dto);

    Task<ProfileDto> UpdatePhotoAsync(ProfilePhotoDto dto);
    Task RemovePhotoAsync();
}
