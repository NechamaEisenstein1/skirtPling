using System.Security.Cryptography;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.BL.Mapping;
using SkirtPling.DAL.Entities;
using SkirtPling.DAL.External;
using SkirtPling.DAL.Repositories;

namespace SkirtPling.BL.Services;

public class OrderService(
    IOrderRepository orders,
    ICartRepository carts,
    IPaymentGateway paymentGateway,
    TimeProvider clock) : IOrderService
{
    /// <summary>Which status changes an admin may make. Anything not listed is rejected.</summary>
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.PendingPayment] = [OrderStatus.Cancelled],
        [OrderStatus.Paid] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = []
    };

    /// <summary>
    /// Checkout in three steps: reserve stock and save the order (one DB transaction), charge the payment provider,
    /// then confirm the order. A declined payment cancels the order and returns the stock.
    /// </summary>
    public async Task<OrderDto> PlaceOrderAsync(string userId, string customerEmail, CheckoutDto dto, CancellationToken ct)
    {
        var cart = await carts.GetByUserIdAsync(userId, ct);
        if (cart is null || cart.Items.Count == 0)
            throw new BusinessRuleException("Your cart is empty.");

        EnsureCartIsPurchasable(cart);
        var order = BuildOrder(userId, customerEmail, dto.ShippingAddress, cart);

        if (!await orders.CreateWithStockReservationAsync(order, ct))
            throw new ConflictException("Some items sold out while you were checking out. Please review your cart.");

        var payment = await paymentGateway.ChargeAsync(
            new PaymentRequest(order.OrderNumber, order.Total, PricingCalculator.Currency, dto.PaymentToken), ct);

        if (!payment.Succeeded)
        {
            await orders.CancelAndRestockAsync(order, ct);
            throw new BusinessRuleException($"Payment failed: {payment.FailureReason}");
        }

        order.Status = OrderStatus.Paid;
        order.PaymentReference = payment.TransactionId;
        await orders.SaveChangesAsync(ct);

        cart.Items.Clear();
        await carts.SaveChangesAsync(ct);

        return order.ToDto();
    }

    public async Task<PagedResultDto<OrderSummaryDto>> GetForUserAsync(string userId, int page, int pageSize, CancellationToken ct)
    {
        var result = await orders.SearchAsync(new OrderFilter { UserId = userId, Page = page, PageSize = pageSize }, ct);
        return new PagedResultDto<OrderSummaryDto>(result.Items.Select(o => o.ToSummaryDto()).ToList(), page, pageSize, result.TotalCount);
    }

    public async Task<OrderDto> GetForUserAsync(string userId, int orderId, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(orderId, ct);
        if (order is null || order.UserId != userId)
            throw new NotFoundException("Order not found.");
        return order.ToDto();
    }

    public async Task<PagedResultDto<OrderSummaryDto>> SearchAsync(OrderQueryDto query, CancellationToken ct)
    {
        var filter = new OrderFilter
        {
            Status = string.IsNullOrWhiteSpace(query.Status) ? null : ParseStatus(query.Status),
            Search = query.Search,
            Page = query.Page,
            PageSize = query.PageSize
        };
        var result = await orders.SearchAsync(filter, ct);
        return new PagedResultDto<OrderSummaryDto>(result.Items.Select(o => o.ToSummaryDto()).ToList(), query.Page, query.PageSize, result.TotalCount);
    }

    public async Task<OrderDto> GetByIdAsync(int orderId, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(orderId, ct) ?? throw new NotFoundException("Order not found.");
        return order.ToDto();
    }

    public async Task<OrderDto> UpdateStatusAsync(int orderId, string status, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(orderId, ct) ?? throw new NotFoundException("Order not found.");
        var newStatus = ParseStatus(status);

        if (!AllowedTransitions[order.Status].Contains(newStatus))
            throw new BusinessRuleException($"An order cannot move from {order.Status} to {newStatus}.");

        if (newStatus == OrderStatus.Cancelled)
        {
            await orders.CancelAndRestockAsync(order, ct);
        }
        else
        {
            order.Status = newStatus;
            await orders.SaveChangesAsync(ct);
        }
        return order.ToDto();
    }

    private static void EnsureCartIsPurchasable(Cart cart)
    {
        foreach (var item in cart.Items)
        {
            if (!item.Product.IsActive)
                throw new ConflictException($"'{item.Product.Name}' is no longer available. Please remove it from your cart.");
            if (item.Quantity > item.Product.StockQuantity)
                throw new ConflictException($"Only {item.Product.StockQuantity} of '{item.Product.Name}' left in stock.");
        }
    }

    private Order BuildOrder(string userId, string customerEmail, ShippingAddressDto address, Cart cart)
    {
        var items = cart.Items.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            ProductName = i.Product.Name,
            Sku = i.Product.Sku,
            UnitPrice = i.Product.Price,
            Quantity = i.Quantity
        }).ToList();

        var subtotal = PricingCalculator.Subtotal(items.Select(i => (i.UnitPrice, i.Quantity)));
        var shipping = PricingCalculator.ShippingCost(subtotal);
        var now = clock.GetUtcNow().UtcDateTime;

        return new Order
        {
            OrderNumber = $"SP-{now:yyyyMMdd}-{RandomNumberGenerator.GetHexString(6)}",
            UserId = userId,
            CustomerEmail = customerEmail,
            Status = OrderStatus.PendingPayment,
            ShippingAddress = address.ToEntity(),
            Items = items,
            Subtotal = subtotal,
            ShippingCost = shipping,
            Total = subtotal + shipping,
            CreatedAt = now
        };
    }

    private static OrderStatus ParseStatus(string status) =>
        Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new BusinessRuleException($"Unknown order status '{status}'.");
}
