using BlogAPI.Entity.Dtos.Category;

namespace BlogAPI.Service.Services.Abstractions;

/// <summary>
/// Failures are reported with exceptions from BlogAPI.Core.Exceptions
/// (NotFoundException, ConflictException, ...), not return values.
/// </summary>
public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesNonDeletedAsync();
    Task<CategoryDto> CreateCategoryAsync(CategoryAddDto dto);
    Task UpdateCategoryAsync(Guid id, CategoryUpdateDto dto);
    Task SafeDeleteCategoryAsync(Guid id);
}
