using AutoMapper;
using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Dtos.Articles;
using BlogAPI.Entity.Entities;
using BlogAPI.Service.Services.Abstractions;

namespace BlogAPI.Service.Services.Concretes;

public class ArticleService : IArticleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ArticleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<List<ArticleDto>> GetAllArticlesAsync()
    {
        var articles = await _unitOfWork.GetRepository<Article>().GetAllAsync();
        var articlesDto = _mapper.Map<List<ArticleDto>>(articles);
        return articlesDto;
    } 
}