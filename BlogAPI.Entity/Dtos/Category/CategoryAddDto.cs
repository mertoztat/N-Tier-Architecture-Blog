namespace BlogAPI.Entity.Dtos.Category;

// Validation rules: BlogAPI.Service/FluentValidations/Categories/CategoryAddDtoValidator.cs
public class CategoryAddDto
{
    public string Name { get; set; } = string.Empty;
}
