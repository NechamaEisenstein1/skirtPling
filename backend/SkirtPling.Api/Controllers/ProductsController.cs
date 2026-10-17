using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    public Task<PagedResultDto<ProductListItemDto>> Search([FromQuery] ProductQueryDto query, CancellationToken ct) =>
        products.SearchAsync(query, includeInactive: false, ct);

    [HttpGet("{id:int}")]
    public Task<ProductDetailsDto> Get(int id, CancellationToken ct) =>
        products.GetDetailsAsync(id, includeInactive: false, ct);
}
