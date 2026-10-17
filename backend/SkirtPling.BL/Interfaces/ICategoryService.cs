using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);
    Task<CategoryDto> CreateAsync(CategoryUpsertDto dto, CancellationToken ct);
    Task<CategoryDto> UpdateAsync(int id, CategoryUpsertDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
