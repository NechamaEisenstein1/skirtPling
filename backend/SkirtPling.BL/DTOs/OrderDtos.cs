using System.ComponentModel.DataAnnotations;

namespace SkirtPling.BL.DTOs;

public record ShippingAddressDto(
    [Required, StringLength(150)] string FullName,
    [Required, Phone, StringLength(30)] string Phone,
    [Required, StringLength(200)] string Street,
    [Required, StringLength(100)] string City,
    [Required, StringLength(20)] string PostalCode,
    [Required, StringLength(100)] string Country);

public record CheckoutDto(
    [Required] ShippingAddressDto ShippingAddress,
    [Required, StringLength(100)] string PaymentToken);

public record OrderItemDto(int ProductId, string Sku, string ProductName, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

public record OrderDto(
    int Id, string OrderNumber, string Status, string CustomerEmail, DateTime CreatedAt,
    ShippingAddressDto ShippingAddress, IReadOnlyList<OrderItemDto> Items,
    decimal Subtotal, decimal ShippingCost, decimal Total, string? PaymentReference);

public record OrderSummaryDto(int Id, string OrderNumber, string Status, string CustomerEmail, DateTime CreatedAt, int ItemCount, decimal Total);

public record OrderQueryDto
{
    public string? Status { get; init; }
    [StringLength(100)] public string? Search { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 100)] public int PageSize { get; init; } = 20;
}

public record UpdateOrderStatusDto([Required] string Status);
