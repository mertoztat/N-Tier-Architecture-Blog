using AutoMapper;
using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Dtos.Category;
using BlogAPI.Entity.Entities;
using BlogAPI.Service.Services.Abstractions;

namespace BlogAPI.Service.Services.Concretes;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesNonDeletedAsync()
    {
        var categories = await _unitOfWork.GetRepository<Category>().GetAllAsync(x => !x.IsDeleted);
        return _mapper.Map<List<CategoryDto>>(categories.OrderBy(x => x.Name));
    }
}
