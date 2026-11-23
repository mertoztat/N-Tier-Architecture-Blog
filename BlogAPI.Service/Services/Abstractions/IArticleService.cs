using BlogAPI.Entity.Dtos.Articles;

namespace BlogAPI.Service.Services.Abstractions;

/// <summary>
/// Failures are reported with exceptions from BlogAPI.Core.Exceptions
/// (NotFoundException, ForbiddenException, BadRequestException), not return values.
/// </summary>
public interface IArticleService
{
    Task<List<ArticleDto>> GetAllArticlesWithCategoryAsync();
    Task<ArticleDto> CreateArticleAsync(ArticleAddDto dto);
    Task UpdateArticleAsync(Guid id, ArticleUpdateDto dto);
    Task SafeDeleteArticleAsync(Guid id);
}
