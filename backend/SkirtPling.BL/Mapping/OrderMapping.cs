using SkirtPling.BL.DTOs;
using SkirtPling.DAL.Entities;

namespace SkirtPling.BL.Mapping;

public static class OrderMapping
{
    public static OrderDto ToDto(this Order order) => new(
        order.Id, order.OrderNumber, order.Status.ToString(), order.CustomerEmail, order.CreatedAt,
        order.ShippingAddress.ToDto(),
        order.Items.Select(i => new OrderItemDto(i.ProductId, i.Sku, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
        order.Subtotal, order.ShippingCost, order.Total, order.PaymentReference);

    public static OrderSummaryDto ToSummaryDto(this Order order) => new(
        order.Id, order.OrderNumber, order.Status.ToString(), order.CustomerEmail, order.CreatedAt,
        order.Items.Sum(i => i.Quantity), order.Total);

    public static ShippingAddressDto ToDto(this ShippingAddress a) => new(a.FullName, a.Phone, a.Street, a.City, a.PostalCode, a.Country);

    public static ShippingAddress ToEntity(this ShippingAddressDto a) => new()
    {
        FullName = a.FullName.Trim(),
        Phone = a.Phone.Trim(),
        Street = a.Street.Trim(),
        City = a.City.Trim(),
        PostalCode = a.PostalCode.Trim(),
        Country = a.Country.Trim()
    };
}
