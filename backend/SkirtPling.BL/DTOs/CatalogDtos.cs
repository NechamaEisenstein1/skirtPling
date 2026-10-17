using System.ComponentModel.DataAnnotations;

namespace SkirtPling.BL.DTOs;

public record BrandDto(int Id, string Name, string Slug, string Description);

public record CategoryDto(int Id, string Name, string Slug);

public record BrandUpsertDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [StringLength(1000)] string? Description);

public record CategoryUpsertDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name);

public record ProductImageDto(int Id, string Url, string AltText);

public record ProductSpecificationDto(string Name, string Value);

public record ProductListItemDto(
    int Id, string Sku, string Name, decimal Price, int StockQuantity, bool IsActive,
    string BrandName, string CategoryName, string? ImageUrl)
{
    public bool InStock => StockQuantity > 0;
}

public record ProductDetailsDto(
    int Id, string Sku, string Name, string Description, decimal Price, int StockQuantity, bool IsActive,
    BrandDto Brand, CategoryDto Category,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<ProductSpecificationDto> Specifications,
    IReadOnlyList<ProductListItemDto> RelatedProducts)
{
    public bool InStock => StockQuantity > 0;
}

/// <summary>Catalog search parameters, bound from the query string.</summary>
public record ProductQueryDto
{
    [StringLength(100)] public string? Search { get; init; }
    public int[]? BrandIds { get; init; }
    public int[]? CategoryIds { get; init; }
    [Range(0, 100000)] public decimal? MinPrice { get; init; }
    [Range(0, 100000)] public decimal? MaxPrice { get; init; }
    public bool InStockOnly { get; init; }
    /// <summary>newest | price-asc | price-desc | name</summary>
    public string? Sort { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 50)] public int PageSize { get; init; } = 12;
}

public record ProductImageUpsertDto(
    [Required, StringLength(500)] string Url,
    [StringLength(200)] string? AltText);

public record ProductSpecificationUpsertDto(
    [Required, StringLength(100)] string Name,
    [Required, StringLength(300)] string Value);

public record ProductUpsertDto
{
    [Required, StringLength(40, MinimumLength = 3), RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "SKU may contain letters, digits and dashes only.")]
    public string Sku { get; init; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; init; }

    [Range(0.01, 100000)]
    public decimal Price { get; init; }

    [Range(0, 100000)]
    public int StockQuantity { get; init; }

    public bool IsActive { get; init; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "Choose a brand.")]
    public int BrandId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")]
    public int CategoryId { get; init; }

    public List<ProductImageUpsertDto> Images { get; init; } = [];
    public List<ProductSpecificationUpsertDto> Specifications { get; init; } = [];
}

public record StockUpdateDto([Range(0, 100000)] int StockQuantity);
