using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

public interface IProductRepository
{
    Task<PagedList<Product>> SearchAsync(ProductFilter filter, CancellationToken ct);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct);
    Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);
    Task<List<Product>> GetRelatedAsync(Product product, int count, CancellationToken ct);
    Task<bool> SkuExistsAsync(string sku, int? excludeProductId, CancellationToken ct);
    Task<bool> HasOrdersAsync(int productId, CancellationToken ct);
    void Add(Product product);
    void Remove(Product product);
    Task SaveChangesAsync(CancellationToken ct);
}

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<PagedList<Product>> SearchAsync(ProductFilter filter, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (!filter.IncludeInactive)
            query = query.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Sku.Contains(term) || p.Brand.Name.Contains(term));
        }
        if (filter.BrandIds.Count > 0)
            query = query.Where(p => filter.BrandIds.Contains(p.BrandId));
        if (filter.CategoryIds.Count > 0)
            query = query.Where(p => filter.CategoryIds.Contains(p.CategoryId));
        if (filter.MinPrice is not null)
            query = query.Where(p => p.Price >= filter.MinPrice);
        if (filter.MaxPrice is not null)
            query = query.Where(p => p.Price <= filter.MaxPrice);
        if (filter.InStockOnly)
            query = query.Where(p => p.StockQuantity > 0);

        query = filter.Sort switch
        {
            ProductSort.PriceAsc => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            ProductSort.PriceDesc => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            ProductSort.Name => query.OrderBy(p => p.Name).ThenBy(p => p.Id),
            _ => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsSplitQuery()
            .ToListAsync(ct);

        return new PagedList<Product>(items, total);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Specifications)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
        db.Products.Include(p => p.Images).Where(p => ids.Contains(p.Id)).ToListAsync(ct);

    public Task<List<Product>> GetRelatedAsync(Product product, int count, CancellationToken ct) =>
        db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.Id != product.Id && (p.CategoryId == product.CategoryId || p.BrandId == product.BrandId))
            .OrderByDescending(p => p.CategoryId == product.CategoryId)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .AsSplitQuery()
            .ToListAsync(ct);

    public Task<bool> SkuExistsAsync(string sku, int? excludeProductId, CancellationToken ct) =>
        db.Products.AnyAsync(p => p.Sku == sku && p.Id != excludeProductId, ct);

    public Task<bool> HasOrdersAsync(int productId, CancellationToken ct) =>
        db.Set<OrderItem>().AnyAsync(i => i.ProductId == productId, ct);

    public void Add(Product product) => db.Products.Add(product);

    public void Remove(Product product) => db.Products.Remove(product);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
