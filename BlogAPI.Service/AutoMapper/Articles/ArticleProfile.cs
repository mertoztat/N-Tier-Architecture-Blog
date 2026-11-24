using AutoMapper;
using BlogAPI.Entity.Dtos.Articles;
using BlogAPI.Entity.Entities;

namespace BlogAPI.Service.AutoMapper.Articles;

public class ArticleProfile : Profile
{
    
    public ArticleProfile()
    {
        // Mapping configuration for Article and ArticleDto
        CreateMap<Article, ArticleDto>()
            .ForMember(d => d.ImageFileName, o => o.MapFrom(a => a.Image != null ? a.Image.FileName : null))
            .ReverseMap();
        // Mapping configuration for ArticleAddDto and Article
        CreateMap<ArticleAddDto, Article>().ReverseMap();
        // Mapping configuration for ArticleUpdateDto and Article
        CreateMap<ArticleUpdateDto, Article>().ReverseMap();
    }
} 