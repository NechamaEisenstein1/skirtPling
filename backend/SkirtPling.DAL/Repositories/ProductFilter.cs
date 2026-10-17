namespace SkirtPling.DAL.Repositories;

public enum ProductSort
{
    Newest,
    PriceAsc,
    PriceDesc,
    Name,
    StockAsc
}

/// <summary>Search criteria for products. Page is 1-based.</summary>
public record ProductFilter
{
    public string? Search { get; init; }
    public IReadOnlyCollection<int> BrandIds { get; init; } = [];
    public IReadOnlyCollection<int> CategoryIds { get; init; } = [];
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool InStockOnly { get; init; }
    public bool IncludeInactive { get; init; }
    public ProductSort Sort { get; init; } = ProductSort.Newest;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
