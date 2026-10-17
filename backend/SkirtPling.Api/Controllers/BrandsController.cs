using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController(IBrandService brands) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<BrandDto>> GetAll(CancellationToken ct) => brands.GetAllAsync(ct);
}
