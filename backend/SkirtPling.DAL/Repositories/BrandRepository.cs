using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

public interface IBrandRepository
{
    Task<List<Brand>> GetAllAsync(CancellationToken ct);
    Task<Brand?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct);
    Task<bool> HasProductsAsync(int id, CancellationToken ct);
    void Add(Brand brand);
    void Remove(Brand brand);
    Task SaveChangesAsync(CancellationToken ct);
}

public class BrandRepository(AppDbContext db) : IBrandRepository
{
    public Task<List<Brand>> GetAllAsync(CancellationToken ct) =>
        db.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync(ct);

    public Task<Brand?> GetByIdAsync(int id, CancellationToken ct) => db.Brands.FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct) =>
        db.Brands.AnyAsync(b => b.Slug == slug && b.Id != excludeId, ct);

    public Task<bool> HasProductsAsync(int id, CancellationToken ct) => db.Products.AnyAsync(p => p.BrandId == id, ct);

    public void Add(Brand brand) => db.Brands.Add(brand);

    public void Remove(Brand brand) => db.Brands.Remove(brand);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
