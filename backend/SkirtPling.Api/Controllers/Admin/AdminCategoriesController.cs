using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers.Admin;

[Route("api/admin/categories")]
public class AdminCategoriesController(ICategoryService categories) : AdminControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryUpsertDto dto, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await categories.CreateAsync(dto, ct));

    [HttpPut("{id:int}")]
    public Task<CategoryDto> Update(int id, CategoryUpsertDto dto, CancellationToken ct) => categories.UpdateAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await categories.DeleteAsync(id, ct);
        return NoContent();
    }
}
