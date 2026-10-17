using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.BL.Mapping;
using SkirtPling.DAL.Entities;
using SkirtPling.DAL.Repositories;

namespace SkirtPling.BL.Services;

public class BrandService(IBrandRepository brands) : IBrandService
{
    public async Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken ct) =>
        (await brands.GetAllAsync(ct)).Select(b => b.ToDto()).ToList();

    public async Task<BrandDto> CreateAsync(BrandUpsertDto dto, CancellationToken ct)
    {
        var brand = new Brand();
        await ApplyAsync(brand, dto, ct);
        brands.Add(brand);
        await brands.SaveChangesAsync(ct);
        return brand.ToDto();
    }

    public async Task<BrandDto> UpdateAsync(int id, BrandUpsertDto dto, CancellationToken ct)
    {
        var brand = await brands.GetByIdAsync(id, ct) ?? throw new NotFoundException("Brand not found.");
        await ApplyAsync(brand, dto, ct);
        await brands.SaveChangesAsync(ct);
        return brand.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var brand = await brands.GetByIdAsync(id, ct) ?? throw new NotFoundException("Brand not found.");
        if (await brands.HasProductsAsync(id, ct))
            throw new ConflictException("This brand still has products. Move or delete them first.");

        brands.Remove(brand);
        await brands.SaveChangesAsync(ct);
    }

    private async Task ApplyAsync(Brand brand, BrandUpsertDto dto, CancellationToken ct)
    {
        var slug = SlugGenerator.FromName(dto.Name);
        if (await brands.SlugExistsAsync(slug, brand.Id == 0 ? null : brand.Id, ct))
            throw new ConflictException($"A brand named '{dto.Name}' already exists.");

        brand.Name = dto.Name.Trim();
        brand.Slug = slug;
        brand.Description = dto.Description?.Trim() ?? string.Empty;
    }
}
