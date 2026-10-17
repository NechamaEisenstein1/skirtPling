using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface IBrandService
{
    Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken ct);
    Task<BrandDto> CreateAsync(BrandUpsertDto dto, CancellationToken ct);
    Task<BrandDto> UpdateAsync(int id, BrandUpsertDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
