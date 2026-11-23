using AutoMapper;
using BlogAPI.Core.Exceptions;
using BlogAPI.Core.Services;
using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Dtos.Articles;
using BlogAPI.Entity.Entities;
using BlogAPI.Service.Services.Abstractions;

namespace BlogAPI.Service.Services.Concretes;

public class ArticleService : IArticleService
{
    private const string AdminRole = "Admin";
    private const string SuperadminRole = "Superadmin";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public ArticleService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<ArticleDto>> GetAllArticlesWithCategoryAsync()
    {
        var articles = await _unitOfWork.GetRepository<Article>().GetAllAsync(x => !x.IsDeleted, x => x.Category);
        var articlesDto = _mapper.Map<List<ArticleDto>>(articles);
        return articlesDto;
    }

    public async Task<ArticleDto> CreateArticleAsync(ArticleAddDto dto)
    {
        // The endpoint requires [Authorize]; a missing user here is a programming error, hence a 500.
        var userId = _currentUser.UserId
                     ?? throw new InvalidOperationException("CreateArticleAsync requires an authenticated user.");

        await EnsureCategoryExistsAsync(dto.CategoryId);

        var article = _mapper.Map<Article>(dto);
        article.UserId = userId;
        article.CreatedBy = CurrentUserName;
        article.CreatedDate = DateTime.Now;

        var repository = _unitOfWork.GetRepository<Article>();
        await repository.AddAsync(article);
        await _unitOfWork.SaveAsync();

        // Reload with the category so the response contains it.
        var created = await repository.GetAsync(x => x.Id == article.Id, x => x.Category);
        return _mapper.Map<ArticleDto>(created);
    }

    public async Task UpdateArticleAsync(Guid id, ArticleUpdateDto dto)
    {
        var repository = _unitOfWork.GetRepository<Article>();
        var article = await GetActiveArticleAsync(id);
        EnsureCanModify(article);

        if (article.CategoryId != dto.CategoryId)
        {
            await EnsureCategoryExistsAsync(dto.CategoryId);
        }

        _mapper.Map(dto, article);
        article.ModifiedBy = CurrentUserName;
        article.ModifiedDate = DateTime.Now;
        repository.Update(article);
        await _unitOfWork.SaveAsync();
    }

    public async Task SafeDeleteArticleAsync(Guid id)
    {
        var repository = _unitOfWork.GetRepository<Article>();
        var article = await GetActiveArticleAsync(id);
        EnsureCanModify(article);

        article.IsDeleted = true;
        article.DeletedBy = CurrentUserName;
        article.DeletedDate = DateTime.Now;
        repository.Update(article);
        await _unitOfWork.SaveAsync();
    }

    // Name from the JWT ("FirstName LastName"); written into CreatedBy / ModifiedBy / DeletedBy.
    private string CurrentUserName => _currentUser.UserName ?? "System";

    private async Task<Article> GetActiveArticleAsync(Guid id)
    {
        return await _unitOfWork.GetRepository<Article>().GetAsync(x => x.Id == id && !x.IsDeleted)
               ?? throw new NotFoundException($"Article '{id}' was not found.");
    }

    /// <summary>Only the author, an Admin or a Superadmin may change or delete an article.</summary>
    private void EnsureCanModify(Article article)
    {
        var isOwner = _currentUser.UserId == article.UserId;
        var isAdmin = _currentUser.IsInRole(AdminRole) || _currentUser.IsInRole(SuperadminRole);

        if (!isOwner && !isAdmin)
        {
            throw new ForbiddenException("You can only modify your own articles.");
        }
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId)
    {
        var exists = await _unitOfWork.GetRepository<Category>().AnyAsync(x => x.Id == categoryId && !x.IsDeleted);
        if (!exists)
        {
            throw new BadRequestException($"Category '{categoryId}' does not exist.");
        }
    }
}
