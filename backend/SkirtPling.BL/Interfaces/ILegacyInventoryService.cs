using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface ILegacyInventoryService
{
    bool IsAvailable { get; }

    /// <summary>Compares the shop's stock for a SKU with the physical count in the legacy warehouse system.</summary>
    Task<LegacyStockDto> CompareStockAsync(string sku, CancellationToken ct);
}
