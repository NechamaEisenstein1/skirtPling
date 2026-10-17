using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(string userId, string customerEmail, CheckoutDto dto, CancellationToken ct);
    Task<PagedResultDto<OrderSummaryDto>> GetForUserAsync(string userId, int page, int pageSize, CancellationToken ct);

    /// <summary>Throws NotFound when the order belongs to someone else, so other customers' order ids are not revealed.</summary>
    Task<OrderDto> GetForUserAsync(string userId, int orderId, CancellationToken ct);

    Task<PagedResultDto<OrderSummaryDto>> SearchAsync(OrderQueryDto query, CancellationToken ct);
    Task<OrderDto> GetByIdAsync(int orderId, CancellationToken ct);
    Task<OrderDto> UpdateStatusAsync(int orderId, string status, CancellationToken ct);
}
