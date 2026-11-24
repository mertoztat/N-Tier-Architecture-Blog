using BlogAPI.Entity.Dtos.Category;

namespace BlogAPI.Entity.Dtos.Articles;

public class ArticleDto
{
 public Guid Id { get; set; }
 public string Title { get; set; }
 public string Content { get; set; }
 public int ViewCount { get; set; }
 public string CreatedBy { get; set; }
 public CategoryDto Category { get; set; }
 // Relative path under /images, e.g. "article-images/baslik_123.jpg"; null when the article has no photo.
 public string? ImageFileName { get; set; }
 public DateTime CreatedDate { get; set; }
}