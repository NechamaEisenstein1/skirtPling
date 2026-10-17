using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;
using SkirtPling.DAL.Repositories;

namespace SkirtPling.BL.Services;

public class DashboardService(IDashboardRepository dashboard) : IDashboardService
{
    public const int LowStockThreshold = 5;
    private const int TopProductCount = 5;

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct)
    {
        var sales = await dashboard.GetSalesTotalsAsync(ct);
        var byStatus = await dashboard.CountOrdersByStatusAsync(ct);
        var productCount = await dashboard.CountProductsAsync(ct);
        var lowStock = await dashboard.CountLowStockProductsAsync(LowStockThreshold, ct);
        var top = await dashboard.GetTopSellingProductsAsync(TopProductCount, ct);

        return new DashboardStatsDto(
            sales.Revenue, sales.PaidOrders, productCount, lowStock,
            byStatus.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            top.Select(p => new TopProductDto(p.ProductId, p.ProductName, p.UnitsSold, p.Revenue)).ToList());
    }
}
