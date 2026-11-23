using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Entity.Dtos.Articles;

/// <summary>The article id comes from the route (PUT api/articles/{id}), not from the body.</summary>
public class ArticleUpdateDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }
}
