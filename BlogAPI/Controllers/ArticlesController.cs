using BlogAPI.Entity.Dtos.Articles;
using BlogAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlogAPI.Controllers;

// Not-found / forbidden / bad-request cases are thrown by the service as AppExceptions
// and turned into ProblemDetails responses by GlobalExceptionHandler.
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var articles = await _articleService.GetAllArticlesWithCategoryAsync();
        return Ok(articles);
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("ArticleCreate")]
    public async Task<IActionResult> Create([FromForm] ArticleAddDto dto)
    {
        var article = await _articleService.CreateArticleAsync(dto);
        return StatusCode(StatusCodes.Status201Created, article);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromForm] ArticleUpdateDto dto)
    {
        await _articleService.UpdateArticleAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _articleService.SafeDeleteArticleAsync(id);
        return NoContent();
    }
}
