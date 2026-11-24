namespace BlogAPI.Entity.Dtos.Category;

// The category id comes from the route (PUT api/categories/{id}), not from the body.
// Validation rules: BlogAPI.Service/FluentValidations/Categories/CategoryUpdateDtoValidator.cs
public class CategoryUpdateDto
{
    public string Name { get; set; } = string.Empty;
}
