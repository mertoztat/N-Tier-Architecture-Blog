using Microsoft.AspNetCore.Http;

namespace BlogAPI.Entity.Dtos.Profile;

// Validation rules: BlogAPI.Service/FluentValidations/Profile/ProfilePhotoDtoValidator.cs
public class ProfilePhotoDto
{
    public IFormFile? Photo { get; set; }
}
