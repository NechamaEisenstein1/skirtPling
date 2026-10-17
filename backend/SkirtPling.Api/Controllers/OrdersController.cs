using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkirtPling.Api.Auth;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers;

/// <summary>Checkout and the signed-in customer's own order history.</summary>
[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Checkout(CheckoutDto dto, CancellationToken ct)
    {
        var order = await orders.PlaceOrderAsync(User.GetUserId(), User.GetEmail(), dto, ct);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet]
    public Task<PagedResultDto<OrderSummaryDto>> GetMine(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 50)] int pageSize = 10,
        CancellationToken ct = default) =>
        orders.GetForUserAsync(User.GetUserId(), page, pageSize, ct);

    [HttpGet("{id:int}")]
    public Task<OrderDto> Get(int id, CancellationToken ct) => orders.GetForUserAsync(User.GetUserId(), id, ct);
}
