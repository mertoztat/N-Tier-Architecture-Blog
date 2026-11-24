using BlogAPI.Entity.Dtos.Articles;
using BlogAPI.Service.FluentValidations.Common;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Articles;

public class ArticleUpdateDtoValidator : AbstractValidator<ArticleUpdateDto>
{
    public ArticleUpdateDtoValidator()
    {
        // Photo is optional; when omitted the article keeps its current image.
        RuleFor(x => x.Photo)
            .SetValidator(new ArticlePhotoValidator())
            .When(x => x.Photo is not null);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithName("Başlık");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithName("İçerik");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithName("Kategori");
    }
}
