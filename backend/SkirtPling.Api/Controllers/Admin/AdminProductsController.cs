using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers.Admin;

[Route("api/admin/products")]
public class AdminProductsController(IProductService products) : AdminControllerBase
{
    [HttpGet]
    public Task<PagedResultDto<ProductListItemDto>> Search([FromQuery] ProductQueryDto query, CancellationToken ct) =>
        products.SearchAsync(query, includeInactive: true, ct);

    [HttpGet("{id:int}")]
    public Task<ProductDetailsDto> Get(int id, CancellationToken ct) => products.GetDetailsAsync(id, includeInactive: true, ct);

    [HttpPost]
    public async Task<ActionResult<ProductDetailsDto>> Create(ProductUpsertDto dto, CancellationToken ct)
    {
        var product = await products.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public Task<ProductDetailsDto> Update(int id, ProductUpsertDto dto, CancellationToken ct) => products.UpdateAsync(id, dto, ct);

    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(int id, StockUpdateDto dto, CancellationToken ct)
    {
        await products.UpdateStockAsync(id, dto.StockQuantity, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await products.DeleteAsync(id, ct);
        return NoContent();
    }
}
