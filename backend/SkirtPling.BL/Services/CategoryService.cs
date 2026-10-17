using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.BL.Mapping;
using SkirtPling.DAL.Entities;
using SkirtPling.DAL.Repositories;

namespace SkirtPling.BL.Services;

public class CategoryService(ICategoryRepository categories) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct) =>
        (await categories.GetAllAsync(ct)).Select(c => c.ToDto()).ToList();

    public async Task<CategoryDto> CreateAsync(CategoryUpsertDto dto, CancellationToken ct)
    {
        var category = new Category();
        await ApplyAsync(category, dto, ct);
        categories.Add(category);
        await categories.SaveChangesAsync(ct);
        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryUpsertDto dto, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(id, ct) ?? throw new NotFoundException("Category not found.");
        await ApplyAsync(category, dto, ct);
        await categories.SaveChangesAsync(ct);
        return category.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(id, ct) ?? throw new NotFoundException("Category not found.");
        if (await categories.HasProductsAsync(id, ct))
            throw new ConflictException("This category still has products. Move or delete them first.");

        categories.Remove(category);
        await categories.SaveChangesAsync(ct);
    }

    private async Task ApplyAsync(Category category, CategoryUpsertDto dto, CancellationToken ct)
    {
        var slug = SlugGenerator.FromName(dto.Name);
        if (await categories.SlugExistsAsync(slug, category.Id == 0 ? null : category.Id, ct))
            throw new ConflictException($"A category named '{dto.Name}' already exists.");

        category.Name = dto.Name.Trim();
        category.Slug = slug;
    }
}
