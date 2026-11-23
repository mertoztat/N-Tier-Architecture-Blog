namespace BlogAPI.Entity.Dtos.Articles;

// Validation rules: BlogAPI.Service/FluentValidations/Articles/ArticleAddDtoValidator.cs
public class ArticleAddDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}
