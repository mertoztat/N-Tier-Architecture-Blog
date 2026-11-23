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
}
