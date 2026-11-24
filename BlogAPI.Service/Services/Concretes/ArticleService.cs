using AutoMapper;
using BlogAPI.Core.Exceptions;
using BlogAPI.Core.Services;
using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Dtos.Articles;
using BlogAPI.Entity.Entities;
using BlogAPI.Entity.Enums;
using BlogAPI.Service.Helpers.Images;
using BlogAPI.Service.Services.Abstractions;

namespace BlogAPI.Service.Services.Concretes;

public class ArticleService : IArticleService
{
    private const string AdminRole = "Admin";
    private const string SuperadminRole = "Superadmin";
    private const string ArticleImagesFolder = "article-images";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IImageHelper _imageHelper;

    public ArticleService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IImageHelper imageHelper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _imageHelper = imageHelper;
    }

    public async Task<List<ArticleDto>> GetAllArticlesWithCategoryAsync()
    {
        var articles = await _unitOfWork.GetRepository<Article>().GetAllAsync(x => !x.IsDeleted, x => x.Category, x => x.Image!);
        var articlesDto = _mapper.Map<List<ArticleDto>>(articles);
        return articlesDto;
    }

    public async Task<ArticleDto> CreateArticleAsync(ArticleAddDto dto)
    {
        // The endpoint requires [Authorize]; a missing user here is a programming error, hence a 500.
        var userId = _currentUser.UserId
                     ?? throw new InvalidOperationException("CreateArticleAsync requires an authenticated user.");

        await EnsureCategoryExistsAsync(dto.CategoryId);

        // Checked before the upload so a duplicate request never writes a file.
        var title = dto.Title.Trim();
        await EnsureTitleIsUniqueAsync(userId, title);

        var article = _mapper.Map<Article>(dto);
        article.Title = title;
        article.UserId = userId;
        article.CreatedBy = CurrentUserName;
        article.CreatedDate = DateTime.Now;

        // Photo is optional (Article.ImageId is nullable).
        string? uploadedFile = null;
        if (dto.Photo is { Length: > 0 })
        {
            var upload = await _imageHelper.UploadImage(dto.Title, dto.Photo, ImageType.Post, ArticleImagesFolder);
            uploadedFile = upload.FullName;

            // Setting the navigation lets EF insert the Image and fill Article.ImageId in the same SaveChanges.
            article.Image = new Image
            {
                FileName = upload.FullName,
                FileType = dto.Photo.ContentType,
                CreatedBy = CurrentUserName,
                CreatedDate = DateTime.Now
            };
        }

        var repository = _unitOfWork.GetRepository<Article>();
        try
        {
            await repository.AddAsync(article);
            await _unitOfWork.SaveAsync();
        }
        catch
        {
            // Don't leave an orphan file on disk if the DB write fails.
            if (uploadedFile is not null)
            {
                _imageHelper.DeleteImage(uploadedFile);
            }
            throw;
        }

        // Reload with the category so the response contains it.
        var created = await repository.GetAsync(x => x.Id == article.Id, x => x.Category, x => x.Image!);
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

        // Uniqueness is per author (not per editor), and the article itself is excluded.
        var title = dto.Title.Trim();
        if (!string.Equals(article.Title, title, StringComparison.Ordinal))
        {
            await EnsureTitleIsUniqueAsync(article.UserId, title, excludeId: article.Id);
        }

        _mapper.Map(dto, article);
        article.Title = title;
        article.ModifiedBy = CurrentUserName;
        article.ModifiedDate = DateTime.Now;

        // Photo is optional: without one the article keeps its current image.
        var oldImage = article.Image;
        string? newFile = null;
        string? oldFileToDelete = null;

        if (dto.Photo is { Length: > 0 })
        {
            var upload = await _imageHelper.UploadImage(title, dto.Photo, ImageType.Post, ArticleImagesFolder);
            newFile = upload.FullName;

            article.Image = new Image
            {
                FileName = upload.FullName,
                FileType = dto.Photo.ContentType,
                CreatedBy = CurrentUserName,
                CreatedDate = DateTime.Now
            };

            // Retire the old image only if this was an uploaded article image that no other article uses.
            // (Seed images and shared images are left alone.)
            if (oldImage is not null && oldImage.FileName.StartsWith(ArticleImagesFolder + "/", StringComparison.Ordinal))
            {
                var usedElsewhere = await repository.AnyAsync(x => x.Id != article.Id && x.ImageId == oldImage.Id);
                if (!usedElsewhere)
                {
                    oldImage.IsDeleted = true;
                    oldImage.DeletedBy = CurrentUserName;
                    oldImage.DeletedDate = DateTime.Now;
                    oldFileToDelete = oldImage.FileName;
                }
            }
        }

        try
        {
            repository.Update(article);
            await _unitOfWork.SaveAsync();
        }
        catch
        {
            // The old image is still referenced, so only the new file is an orphan.
            if (newFile is not null)
            {
                _imageHelper.DeleteImage(newFile);
            }
            throw;
        }

        // Only after the DB commit: a failed save must never cost the user their old image.
        if (oldFileToDelete is not null)
        {
            _imageHelper.DeleteImage(oldFileToDelete);
        }
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
        return await _unitOfWork.GetRepository<Article>().GetAsync(x => x.Id == id && !x.IsDeleted, x => x.Image!)
               ?? throw new NotFoundException("Makale bulunamadı.");
    }

    /// <summary>Only the author, an Admin or a Superadmin may change or delete an article.</summary>
    private void EnsureCanModify(Article article)
    {
        var isOwner = _currentUser.UserId == article.UserId;
        var isAdmin = _currentUser.IsInRole(AdminRole) || _currentUser.IsInRole(SuperadminRole);

        if (!isOwner && !isAdmin)
        {
            throw new ForbiddenException("Yalnızca kendi makalelerinizi düzenleyebilir veya silebilirsiniz.");
        }
    }

    private async Task EnsureTitleIsUniqueAsync(Guid authorId, string title, Guid? excludeId = null)
    {
        var duplicate = await _unitOfWork.GetRepository<Article>()
            .AnyAsync(x => x.UserId == authorId && x.Title == title && !x.IsDeleted && x.Id != excludeId);
        if (duplicate)
        {
            throw new ConflictException("Bu başlıkla zaten bir makale mevcut.");
        }
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId)
    {
        var exists = await _unitOfWork.GetRepository<Category>().AnyAsync(x => x.Id == categoryId && !x.IsDeleted);
        if (!exists)
        {
            throw new BadRequestException("Seçilen kategori bulunamadı.");
        }
    }
}
