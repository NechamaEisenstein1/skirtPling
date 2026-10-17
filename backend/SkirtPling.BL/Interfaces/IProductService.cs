using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface IProductService
{
    /// <param name="includeInactive">True only for admin listings; shoppers never see hidden products.</param>
    Task<PagedResultDto<ProductListItemDto>> SearchAsync(ProductQueryDto query, bool includeInactive, CancellationToken ct);
    Task<ProductDetailsDto> GetDetailsAsync(int id, bool includeInactive, CancellationToken ct);
    Task<ProductDetailsDto> CreateAsync(ProductUpsertDto dto, CancellationToken ct);
    Task<ProductDetailsDto> UpdateAsync(int id, ProductUpsertDto dto, CancellationToken ct);
    Task UpdateStockAsync(int id, int stockQuantity, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
