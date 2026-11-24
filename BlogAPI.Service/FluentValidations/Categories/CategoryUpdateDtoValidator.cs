using BlogAPI.Entity.Dtos.Category;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Categories;

public class CategoryUpdateDtoValidator : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithName("Kategori adı");
    }
}
