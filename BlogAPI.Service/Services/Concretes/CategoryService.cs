using AutoMapper;
using BlogAPI.Core.Exceptions;
using BlogAPI.Core.Services;
using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Dtos.Category;
using BlogAPI.Entity.Entities;
using BlogAPI.Service.Services.Abstractions;

namespace BlogAPI.Service.Services.Concretes;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesNonDeletedAsync()
    {
        var categories = await _unitOfWork.GetRepository<Category>().GetAllAsync(x => !x.IsDeleted);
        return _mapper.Map<List<CategoryDto>>(categories.OrderBy(x => x.Name));
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryAddDto dto)
    {
        var name = dto.Name.Trim();
        await EnsureNameIsUniqueAsync(name);

        var category = _mapper.Map<Category>(dto);
        category.Name = name;
        category.CreatedBy = CurrentUserName;
        category.CreatedDate = DateTime.Now;

        await _unitOfWork.GetRepository<Category>().AddAsync(category);
        await _unitOfWork.SaveAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task UpdateCategoryAsync(Guid id, CategoryUpdateDto dto)
    {
        var repository = _unitOfWork.GetRepository<Category>();
        var category = await GetActiveCategoryAsync(id);

        var name = dto.Name.Trim();
        if (!string.Equals(category.Name, name, StringComparison.Ordinal))
        {
            await EnsureNameIsUniqueAsync(name, excludeId: category.Id);
        }

        category.Name = name;
        category.ModifiedBy = CurrentUserName;
        category.ModifiedDate = DateTime.Now;
        repository.Update(category);
        await _unitOfWork.SaveAsync();
    }

    public async Task SafeDeleteCategoryAsync(Guid id)
    {
        var repository = _unitOfWork.GetRepository<Category>();
        var category = await GetActiveCategoryAsync(id);

        // Article.CategoryId is required, so deleting a category that is still in use
        // would leave articles pointing at a deleted category.
        var inUse = await _unitOfWork.GetRepository<Article>()
            .AnyAsync(x => x.CategoryId == id && !x.IsDeleted);
        if (inUse)
        {
            throw new ConflictException("Bu kategoriye ait makaleler var. Önce makaleleri başka bir kategoriye taşıyın veya silin.");
        }

        category.IsDeleted = true;
        category.DeletedBy = CurrentUserName;
        category.DeletedDate = DateTime.Now;
        repository.Update(category);
        await _unitOfWork.SaveAsync();
    }

    // Name from the JWT ("FirstName LastName"); written into CreatedBy / ModifiedBy / DeletedBy.
    private string CurrentUserName => _currentUser.UserName ?? "System";

    private async Task<Category> GetActiveCategoryAsync(Guid id)
    {
        return await _unitOfWork.GetRepository<Category>().GetAsync(x => x.Id == id && !x.IsDeleted)
               ?? throw new NotFoundException("Kategori bulunamadı.");
    }

    private async Task EnsureNameIsUniqueAsync(string name, Guid? excludeId = null)
    {
        var exists = await _unitOfWork.GetRepository<Category>()
            .AnyAsync(x => x.Name == name && !x.IsDeleted && x.Id != excludeId);
        if (exists)
        {
            throw new ConflictException("Bu isimde bir kategori zaten mevcut.");
        }
    }
}
