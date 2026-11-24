using BlogAPI.Entity.Dtos.Category;
using BlogAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // Used by the frontend to fill the category dropdown on the article add/update forms.
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesNonDeletedAsync();
        return Ok(categories);
    }

    // Managing categories is restricted to Admin / Superadmin (same roles as ArticleService's override rule).
    [HttpPost]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<IActionResult> Create(CategoryAddDto dto)
    {
        var category = await _categoryService.CreateCategoryAsync(dto);
        return StatusCode(StatusCodes.Status201Created, category);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<IActionResult> Update(Guid id, CategoryUpdateDto dto)
    {
        await _categoryService.UpdateCategoryAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.SafeDeleteCategoryAsync(id);
        return NoContent();
    }
}
