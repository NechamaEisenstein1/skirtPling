using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers.Admin;

[Route("api/admin/orders")]
public class AdminOrdersController(IOrderService orders) : AdminControllerBase
{
    [HttpGet]
    public Task<PagedResultDto<OrderSummaryDto>> Search([FromQuery] OrderQueryDto query, CancellationToken ct) =>
        orders.SearchAsync(query, ct);

    [HttpGet("{id:int}")]
    public Task<OrderDto> Get(int id, CancellationToken ct) => orders.GetByIdAsync(id, ct);

    [HttpPatch("{id:int}/status")]
    public Task<OrderDto> UpdateStatus(int id, UpdateOrderStatusDto dto, CancellationToken ct) =>
        orders.UpdateStatusAsync(id, dto.Status, ct);
}
