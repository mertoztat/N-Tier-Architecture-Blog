using Microsoft.AspNetCore.Http;

namespace BlogAPI.Entity.Dtos.Articles;

// The article id comes from the route (PUT api/articles/{id}), not from the body.
// Validation rules: BlogAPI.Service/FluentValidations/Articles/ArticleUpdateDtoValidator.cs
public class ArticleUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    // Optional: when sent, it replaces the article's current image.
    public IFormFile? Photo { get; set; }
}
