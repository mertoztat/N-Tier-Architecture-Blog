using BlogAPI.Entity.Dtos.Category;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Categories;

// Only checks the shape of the request. "Is this name already taken?" needs the database
// and stays in CategoryService.
public class CategoryAddDtoValidator : AbstractValidator<CategoryAddDto>
{
    public CategoryAddDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithName("Kategori adı");
    }
}
