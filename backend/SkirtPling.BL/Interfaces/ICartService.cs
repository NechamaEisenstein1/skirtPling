using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface ICartService
{
    Task<CartDto> GetAsync(CartOwner owner, CancellationToken ct);
    Task<CartDto> AddItemAsync(CartOwner owner, AddCartItemDto dto, CancellationToken ct);
    Task<CartDto> UpdateItemAsync(CartOwner owner, int productId, int quantity, CancellationToken ct);
    Task<CartDto> RemoveItemAsync(CartOwner owner, int productId, CancellationToken ct);

    /// <summary>Moves a guest's cart lines into the user's cart after sign-in, then deletes the guest cart.</summary>
    Task<CartDto> MergeGuestCartAsync(string userId, Guid guestId, CancellationToken ct);
}
