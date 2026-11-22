using AutoMapper;
using BlogAPI.Entity.Dtos.Articles;
using BlogAPI.Entity.Entities;

namespace BlogAPI.Service.AutoMapper.Articles;

public class ArticleProfile : Profile
{
    
    public ArticleProfile()
    {
        // Mapping configuration for Article and ArticleDto
        CreateMap<Article, ArticleDto>().ReverseMap();
    }
} 