using BlogAPI.Entity.Dtos.Articles;

namespace BlogAPI.Service.Services.Abstractions;

public interface IArticleService
{
    Task<List<ArticleDto>> GetAllArticlesAsync();
}