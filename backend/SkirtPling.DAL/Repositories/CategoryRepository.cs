using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct);
    Task<bool> HasProductsAsync(int id, CancellationToken ct);
    void Add(Category category);
    void Remove(Category category);
    Task SaveChangesAsync(CancellationToken ct);
}

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync(CancellationToken ct) =>
        db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct) => db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct) =>
        db.Categories.AnyAsync(c => c.Slug == slug && c.Id != excludeId, ct);

    public Task<bool> HasProductsAsync(int id, CancellationToken ct) => db.Products.AnyAsync(p => p.CategoryId == id, ct);

    public void Add(Category category) => db.Categories.Add(category);

    public void Remove(Category category) => db.Categories.Remove(category);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
