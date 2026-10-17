using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.DAL.External;
using SkirtPling.DAL.Repositories;

namespace SkirtPling.BL.Services;

/// <summary>
/// Reconciles shop stock with the legacy warehouse. Business logic only sees ILegacyWarehouseClient,
/// never WCF types, so the legacy system can be replaced without touching this class.
/// </summary>
public class LegacyInventoryService(ILegacyWarehouseClient warehouse, IProductRepository products) : ILegacyInventoryService
{
    public bool IsAvailable => warehouse.IsConfigured;

    public async Task<LegacyStockDto> CompareStockAsync(string sku, CancellationToken ct)
    {
        var product = await products.GetBySkuAsync(sku.Trim().ToUpperInvariant(), ct)
            ?? throw new NotFoundException($"No product with SKU '{sku}'.");

        LegacyStockLevel? stock;
        try
        {
            stock = await warehouse.GetStockLevelAsync(product.Sku, ct);
        }
        catch (LegacyWarehouseUnavailableException)
        {
            throw new ServiceUnavailableException("The legacy warehouse service is not responding. Is it running?");
        }

        if (stock is null)
            throw new NotFoundException($"The legacy warehouse has no record of SKU '{product.Sku}'.");

        return new LegacyStockDto(product.Sku, product.StockQuantity, stock.OnHand, stock.WarehouseCode, stock.LastCountedAt);
    }
}
