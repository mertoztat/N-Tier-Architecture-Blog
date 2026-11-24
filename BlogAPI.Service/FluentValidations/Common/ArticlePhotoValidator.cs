using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace BlogAPI.Service.FluentValidations.Common;

// Shared by ArticleAddDtoValidator and ArticleUpdateDtoValidator.
public class ArticlePhotoValidator : AbstractValidator<IFormFile>
{
    private const long MaxPhotoBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

    public ArticlePhotoValidator()
    {
        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Görsel dosyası boş olamaz.")
            .LessThanOrEqualTo(MaxPhotoBytes).WithMessage("Görsel boyutu en fazla 5 MB olabilir.");

        RuleFor(x => x.FileName)
            .Must(name => AllowedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
            .WithMessage("Görsel yalnızca .jpg, .jpeg, .png veya .webp olabilir.");

        RuleFor(x => x.ContentType)
            .Must(t => AllowedContentTypes.Contains(t.ToLowerInvariant()))
            .WithMessage("Geçersiz görsel türü.");
    }
}
