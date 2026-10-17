namespace SkirtPling.BL.DTOs;

public record TopProductDto(int ProductId, string ProductName, int UnitsSold, decimal Revenue);

public record DashboardStatsDto(
    decimal TotalRevenue, int PaidOrders, int TotalProducts, int LowStockProducts,
    IReadOnlyDictionary<string, int> OrdersByStatus, IReadOnlyList<TopProductDto> TopProducts);

public record AdminUserDto(string Id, string Email, string FullName, IReadOnlyList<string> Roles, bool IsLocked, DateTime CreatedAt);

public record UpdateUserAccessDto(bool IsAdmin, bool IsLocked);

public record LegacyStockDto(string Sku, int ShopStock, int WarehouseOnHand, string WarehouseCode, DateTime LastCountedAt)
{
    public int Difference => WarehouseOnHand - ShopStock;
}
