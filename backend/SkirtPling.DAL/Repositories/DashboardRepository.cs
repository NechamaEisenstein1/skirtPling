using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

public record SalesTotals(decimal Revenue, int PaidOrders);
public record TopProduct(int ProductId, string ProductName, int UnitsSold, decimal Revenue);

/// <summary>Read-only aggregate queries for the admin dashboard.</summary>
public interface IDashboardRepository
{
    Task<SalesTotals> GetSalesTotalsAsync(CancellationToken ct);
    Task<Dictionary<OrderStatus, int>> CountOrdersByStatusAsync(CancellationToken ct);
    Task<int> CountProductsAsync(CancellationToken ct);
    Task<int> CountLowStockProductsAsync(int threshold, CancellationToken ct);
    Task<List<TopProduct>> GetTopSellingProductsAsync(int count, CancellationToken ct);
}

public class DashboardRepository(AppDbContext db) : IDashboardRepository
{
    private static readonly OrderStatus[] SoldStatuses = [OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Delivered];

    public async Task<SalesTotals> GetSalesTotalsAsync(CancellationToken ct)
    {
        var sold = db.Orders.Where(o => SoldStatuses.Contains(o.Status));
        return new SalesTotals(await sold.SumAsync(o => o.Total, ct), await sold.CountAsync(ct));
    }

    public async Task<Dictionary<OrderStatus, int>> CountOrdersByStatusAsync(CancellationToken ct) =>
        await db.Orders.GroupBy(o => o.Status).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);

    public Task<int> CountProductsAsync(CancellationToken ct) => db.Products.CountAsync(ct);

    public Task<int> CountLowStockProductsAsync(int threshold, CancellationToken ct) =>
        db.Products.CountAsync(p => p.IsActive && p.StockQuantity <= threshold, ct);

    public async Task<List<TopProduct>> GetTopSellingProductsAsync(int count, CancellationToken ct)
    {
        var top = await db.Set<OrderItem>()
            .Where(i => db.Orders.Any(o => o.Id == i.OrderId && SoldStatuses.Contains(o.Status)))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, UnitsSold = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.UnitPrice * i.Quantity) })
            .OrderByDescending(x => x.UnitsSold)
            .Take(count)
            .ToListAsync(ct);

        var ids = top.Select(x => x.ProductId).ToList();
        var names = await db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name, ct);
        return top.Select(x => new TopProduct(x.ProductId, names.GetValueOrDefault(x.ProductId, "(deleted)"), x.UnitsSold, x.Revenue)).ToList();
    }
}
