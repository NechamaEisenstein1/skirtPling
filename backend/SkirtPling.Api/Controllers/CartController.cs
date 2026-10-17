using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkirtPling.Api.Auth;
using SkirtPling.Api.Extensions;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers;

/// <summary>
/// The cart of the signed-in user, or, for guests, the cart identified by the X-Guest-Cart-Id header.
/// </summary>
[ApiController]
[Route("api/cart")]
public class CartController(ICartService cart) : ControllerBase
{
    [HttpGet]
    public Task<CartDto> Get(CancellationToken ct) => cart.GetAsync(GetOwner(), ct);

    [HttpPost("items")]
    public Task<CartDto> AddItem(AddCartItemDto dto, CancellationToken ct) => cart.AddItemAsync(GetOwner(), dto, ct);

    [HttpPut("items/{productId:int}")]
    public Task<CartDto> UpdateItem(int productId, UpdateCartItemDto dto, CancellationToken ct) =>
        cart.UpdateItemAsync(GetOwner(), productId, dto.Quantity, ct);

    [HttpDelete("items/{productId:int}")]
    public Task<CartDto> RemoveItem(int productId, CancellationToken ct) => cart.RemoveItemAsync(GetOwner(), productId, ct);

    /// <summary>Called right after sign-in to move the guest cart into the user's cart.</summary>
    [Authorize]
    [HttpPost("merge")]
    public async Task<ActionResult<CartDto>> Merge(CancellationToken ct)
    {
        var guestId = ReadGuestId();
        if (guestId is null)
            return await cart.GetAsync(GetOwner(), ct);
        return await cart.MergeGuestCartAsync(User.GetUserId(), guestId.Value, ct);
    }

    private CartOwner GetOwner() =>
        User.Identity?.IsAuthenticated == true
            ? new CartOwner(User.GetUserId(), null)
            : new CartOwner(null, ReadGuestId());

    private Guid? ReadGuestId() =>
        Guid.TryParse(Request.Headers[CartHeaders.GuestCartId], out var id) && id != Guid.Empty ? id : null;
}
