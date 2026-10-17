using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.Tests.TestSupport;

/// <summary>
/// A throwaway in-memory SQLite database with a tiny catalog. Business services are tested against
/// real repositories on it, so tests exercise the actual queries instead of hand-written mocks.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");

    public AppDbContext Context { get; }
    public Product Skirt { get; }
    public Product LastOne { get; }
    public Product Hidden { get; }
    public AppUser Customer { get; }

    public TestDatabase()
    {
        connection.Open();
        Context = new SqliteAppDbContext(new DbContextOptionsBuilder<SqliteAppDbContext>().UseSqlite(connection).Options);
        Context.Database.EnsureCreated();

        var brand = new Brand { Name = "Test Brand", Slug = "test-brand" };
        var category = new Category { Name = "Pleated", Slug = "pleated" };
        Skirt = NewProduct("T-001", "Pleated Skirt", 100m, 10, brand, category);
        LastOne = NewProduct("T-002", "Last One Skirt", 250m, 1, brand, category);
        Hidden = NewProduct("T-003", "Hidden Skirt", 80m, 5, brand, category, isActive: false);
        Customer = new AppUser { Id = "user-1", UserName = "dina@example.com", Email = "dina@example.com", FullName = "Dina" };

        Context.AddRange(Skirt, LastOne, Hidden, Customer);
        Context.SaveChanges();
    }

    private static Product NewProduct(string sku, string name, decimal price, int stock, Brand brand, Category category, bool isActive = true) => new()
    {
        Sku = sku,
        Name = name,
        Price = price,
        StockQuantity = stock,
        IsActive = isActive,
        Brand = brand,
        Category = category,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    /// <summary>Reads a product's stock with a fresh query, bypassing the change tracker.</summary>
    public int StockOf(Product product) =>
        Context.Products.AsNoTracking().Single(p => p.Id == product.Id).StockQuantity;

    public void Dispose()
    {
        Context.Dispose();
        connection.Dispose();
    }
}
