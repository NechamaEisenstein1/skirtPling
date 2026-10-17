using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

public interface IOrderRepository
{
    Task<PagedList<Order>> SearchAsync(OrderFilter filter, CancellationToken ct);
    Task<Order?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Atomically decrements stock for every line and inserts the order.
    /// Returns false (and changes nothing) if any product is inactive or no longer has enough stock.
    /// </summary>
    Task<bool> CreateWithStockReservationAsync(Order order, CancellationToken ct);

    /// <summary>Marks the order cancelled and returns its quantities to stock, in one transaction.</summary>
    Task CancelAndRestockAsync(Order order, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task<PagedList<Order>> SearchAsync(OrderFilter filter, CancellationToken ct)
    {
        var query = db.Orders.AsNoTracking().AsQueryable();

        if (filter.UserId is not null)
            query = query.Where(o => o.UserId == filter.UserId);
        if (filter.Status is not null)
            query = query.Where(o => o.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(o => o.OrderNumber.Contains(term) || o.CustomerEmail.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(o => o.Items)
            .AsSplitQuery()
            .ToListAsync(ct);

        return new PagedList<Order>(items, total);
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<bool> CreateWithStockReservationAsync(Order order, CancellationToken ct) =>
        // With retry-on-failure enabled, a manual transaction must run inside the execution strategy
        // so the whole unit is retried together.
        db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            foreach (var item in order.Items)
            {
                // A conditional UPDATE is atomic in the database, so two concurrent checkouts can't both take the last unit.
                var updatedRows = await db.Products
                    .Where(p => p.Id == item.ProductId && p.IsActive && p.StockQuantity >= item.Quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity - item.Quantity), ct);

                if (updatedRows == 0)
                {
                    await transaction.RollbackAsync(ct);
                    return false;
                }
            }

            db.Orders.Add(order);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return true;
        });

    public Task CancelAndRestockAsync(Order order, CancellationToken ct) =>
        db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            foreach (var item in order.Items)
            {
                await db.Products
                    .Where(p => p.Id == item.ProductId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity + item.Quantity), ct);
            }

            order.Status = OrderStatus.Cancelled;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        });

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
