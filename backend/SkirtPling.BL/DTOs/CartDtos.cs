using System.ComponentModel.DataAnnotations;

namespace SkirtPling.BL.DTOs;

public record CartItemDto(int ProductId, string Sku, string Name, string? ImageUrl, decimal UnitPrice, int Quantity, int AvailableStock)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

public record CartDto(IReadOnlyList<CartItemDto> Items, decimal Subtotal, decimal ShippingCost, decimal Total)
{
    public int ItemCount => Items.Sum(i => i.Quantity);
}

public record AddCartItemDto([Range(1, int.MaxValue)] int ProductId, [Range(1, 20)] int Quantity);

public record UpdateCartItemDto([Range(1, 20)] int Quantity);

/// <summary>Identifies whose cart an operation applies to: a signed-in user, otherwise a guest token.</summary>
public record CartOwner(string? UserId, Guid? GuestId)
{
    public bool IsKnown => UserId is not null || GuestId is not null;
}
