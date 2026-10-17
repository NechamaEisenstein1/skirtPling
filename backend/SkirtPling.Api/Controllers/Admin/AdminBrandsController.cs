using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers.Admin;

[Route("api/admin/brands")]
public class AdminBrandsController(IBrandService brands) : AdminControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BrandDto>> Create(BrandUpsertDto dto, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await brands.CreateAsync(dto, ct));

    [HttpPut("{id:int}")]
    public Task<BrandDto> Update(int id, BrandUpsertDto dto, CancellationToken ct) => brands.UpdateAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await brands.DeleteAsync(id, ct);
        return NoContent();
    }
}
