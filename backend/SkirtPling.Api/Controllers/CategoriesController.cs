using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categories) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CategoryDto>> GetAll(CancellationToken ct) => categories.GetAllAsync(ct);
}
