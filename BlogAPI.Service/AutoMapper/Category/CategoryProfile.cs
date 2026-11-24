using AutoMapper;
using BlogAPI.Entity.Dtos.Category;
namespace BlogAPI.Service.AutoMapper.Category;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        // Mapping configuration for Category and CategoryDto
        CreateMap<CategoryDto, Entity.Entities.Category>().ReverseMap();
        CreateMap<CategoryAddDto, Entity.Entities.Category>();
        CreateMap<CategoryUpdateDto, Entity.Entities.Category>();
    }
}