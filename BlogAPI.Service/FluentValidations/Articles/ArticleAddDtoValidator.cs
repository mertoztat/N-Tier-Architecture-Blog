using BlogAPI.Entity.Dtos.Articles;
using BlogAPI.Service.FluentValidations.Common;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Articles;

// Only checks the shape of the request. Rules that need the database
// (e.g. "does this category exist?") stay in ArticleService.
public class ArticleAddDtoValidator : AbstractValidator<ArticleAddDto>
{
    public ArticleAddDtoValidator()
    {
        // Photo is optional; the rules only run when one is sent.
        RuleFor(x => x.Photo)
            .SetValidator(new ImageFileValidator())
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
