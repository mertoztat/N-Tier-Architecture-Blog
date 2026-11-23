using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Entity.Dtos.Articles;

public class ArticleAddDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }
}
