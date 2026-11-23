using BlogAPI.Entity.Dtos.Category;

namespace BlogAPI.Service.Services.Abstractions;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesNonDeletedAsync();
}
