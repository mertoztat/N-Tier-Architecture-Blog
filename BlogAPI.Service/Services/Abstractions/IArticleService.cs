using BlogAPI.Entity.Dtos.Articles;

namespace BlogAPI.Service.Services.Abstractions;

/// <summary>
/// Failures are reported with exceptions from BlogAPI.Core.Exceptions
/// (NotFoundException, ForbiddenException, BadRequestException), not return values.
/// </summary>
public interface IArticleService
{
    Task<List<ArticleDto>> GetAllArticlesWithCategoryAsync();
    /// <summary>Returns one article and counts the view (ViewCount + 1). Throws NotFoundException if it does not exist.</summary>
    Task<ArticleDto> GetArticleByIdAsync(Guid id);
    Task<ArticleDto> CreateArticleAsync(ArticleAddDto dto);
    Task UpdateArticleAsync(Guid id, ArticleUpdateDto dto);
    Task SafeDeleteArticleAsync(Guid id);
}
