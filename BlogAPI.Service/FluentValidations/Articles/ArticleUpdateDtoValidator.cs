using BlogAPI.Entity.Dtos.Articles;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Articles;

public class ArticleUpdateDtoValidator : AbstractValidator<ArticleUpdateDto>
{
    public ArticleUpdateDtoValidator()
    {
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
