using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId, CancellationToken ct);
    Task<Cart?> GetByGuestIdAsync(Guid guestId, CancellationToken ct);
    void Add(Cart cart);
    void Remove(Cart cart);
    Task SaveChangesAsync(CancellationToken ct);
}

public class CartRepository(AppDbContext db) : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(string userId, CancellationToken ct) =>
        CartsWithProducts().FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public Task<Cart?> GetByGuestIdAsync(Guid guestId, CancellationToken ct) =>
        CartsWithProducts().FirstOrDefaultAsync(c => c.GuestId == guestId, ct);

    public void Add(Cart cart) => db.Carts.Add(cart);

    public void Remove(Cart cart) => db.Carts.Remove(cart);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private IQueryable<Cart> CartsWithProducts() =>
        db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .AsSplitQuery();
}
