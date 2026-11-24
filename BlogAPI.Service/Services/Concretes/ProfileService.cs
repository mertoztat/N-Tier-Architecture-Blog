using BlogAPI.Core.Exceptions;
using BlogAPI.Core.Services;
using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Dtos.Profile;
using BlogAPI.Entity.Entities;
using BlogAPI.Entity.Enums;
using BlogAPI.Service.Helpers.Images;
using BlogAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Service.Services.Concretes;

public class ProfileService : IProfileService
{
    private const string UserImagesFolder = "users-images";

    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IImageHelper _imageHelper;

    public ProfileService(
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IImageHelper imageHelper)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _imageHelper = imageHelper;
    }

    public async Task<ProfileDto> GetProfileAsync()
    {
        var user = await GetCurrentUserAsync();
        return await ToDtoAsync(user);
    }

    public async Task ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await GetCurrentUserAsync();

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            throw new BadRequestException(result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordMismatch))
                ? "Mevcut şifre hatalı."
                : Describe(result));
        }

        // Anyone who knew the old password (e.g. a stolen session) must not stay signed in:
        // revoke every active refresh token. Access tokens simply expire; after that this user logs in again.
        var repository = _unitOfWork.GetRepository<RefreshToken>();
        var now = DateTime.UtcNow;
        var active = await repository.GetAllAsync(t => t.UserId == user.Id && t.RevokedAt == null && t.ExpiresAt > now);
        foreach (var token in active)
        {
            token.RevokedAt = now;
            repository.Update(token);
        }
        await _unitOfWork.SaveAsync();
    }

    public async Task<ProfileDto> UpdatePhotoAsync(ProfilePhotoDto dto)
    {
        var photo = dto.Photo ?? throw new BadRequestException("Bir fotoğraf seçmelisiniz.");
        var user = await GetCurrentUserAsync();

        var upload = await _imageHelper.UploadImage(user.Id.ToString("N"), photo, ImageType.User, UserImagesFolder);

        var imageRepository = _unitOfWork.GetRepository<Image>();
        var oldImage = await GetOwnedImageAsync(user);

        var newImage = new Image
        {
            Id = Guid.NewGuid(),
            FileName = upload.FullName,
            FileType = photo.ContentType,
            CreatedBy = CurrentUserName,
            CreatedDate = DateTime.Now
        };
        await imageRepository.AddAsync(newImage);
        user.ImageId = newImage.Id;

        var oldFileToDelete = await RetireImageAsync(user.Id, oldImage);

        // UpdateAsync saves the user together with the new / retired Image rows in one SaveChanges.
        IdentityResult result;
        try
        {
            result = await _userManager.UpdateAsync(user);
        }
        catch
        {
            _imageHelper.DeleteImage(upload.FullName);
            throw;
        }

        if (!result.Succeeded)
        {
            _imageHelper.DeleteImage(upload.FullName);
            throw new BadRequestException(Describe(result));
        }

        // Only after the DB commit: a failed save must never cost the user their old photo.
        if (oldFileToDelete is not null)
        {
            _imageHelper.DeleteImage(oldFileToDelete);
        }

        return await ToDtoAsync(user);
    }

    public async Task RemovePhotoAsync()
    {
        var user = await GetCurrentUserAsync();
        var oldImage = await GetOwnedImageAsync(user);
        if (oldImage is null)
        {
            return;
        }

        user.ImageId = null;
        var oldFileToDelete = await RetireImageAsync(user.Id, oldImage);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new BadRequestException(Describe(result));
        }

        if (oldFileToDelete is not null)
        {
            _imageHelper.DeleteImage(oldFileToDelete);
        }
    }

    private string CurrentUserName => _currentUser.UserName ?? "System";

    private async Task<AppUser> GetCurrentUserAsync()
    {
        // The endpoints require [Authorize]; a missing user here is a programming error, hence a 500.
        var userId = _currentUser.UserId
                     ?? throw new InvalidOperationException("ProfileService requires an authenticated user.");

        return await _userManager.FindByIdAsync(userId.ToString())
               ?? throw new NotFoundException("Kullanıcı bulunamadı.");
    }

    private async Task<Image?> GetOwnedImageAsync(AppUser user) =>
        user.ImageId is { } imageId
            ? await _unitOfWork.GetRepository<Image>().GetByIdAsync(imageId)
            : null;

    /// <summary>
    /// Soft-deletes the old image row and returns its file name when the file may be removed after the save.
    /// Only files uploaded through this feature (users-images/) that no other user shares are removed;
    /// seed images are left alone.
    /// </summary>
    private async Task<string?> RetireImageAsync(Guid userId, Image? oldImage)
    {
        if (oldImage is null || !oldImage.FileName.StartsWith(UserImagesFolder + "/", StringComparison.Ordinal))
        {
            return null;
        }

        var usedElsewhere = await _userManager.Users.AnyAsync(u => u.Id != userId && u.ImageId == oldImage.Id);
        if (usedElsewhere)
        {
            return null;
        }

        oldImage.IsDeleted = true;
        oldImage.DeletedBy = CurrentUserName;
        oldImage.DeletedDate = DateTime.Now;
        return oldImage.FileName;
    }

    private async Task<ProfileDto> ToDtoAsync(AppUser user)
    {
        var image = await GetOwnedImageAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new ProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Roles = roles.OrderBy(r => r).ToList(),
            ImageFileName = image is { IsDeleted: false } ? image.FileName : null
        };
    }

    private static string Describe(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(e => e.Description));
}
