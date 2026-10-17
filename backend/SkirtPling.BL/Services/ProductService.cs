using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.BL.Mapping;
using SkirtPling.DAL.Entities;
using SkirtPling.DAL.Repositories;

namespace SkirtPling.BL.Services;

public class ProductService(
    IProductRepository products,
    IBrandRepository brands,
    ICategoryRepository categories,
    TimeProvider clock) : IProductService
{
    private const int RelatedProductCount = 4;

    public async Task<PagedResultDto<ProductListItemDto>> SearchAsync(ProductQueryDto query, bool includeInactive, CancellationToken ct)
    {
        var filter = new ProductFilter
        {
            Search = query.Search,
            BrandIds = query.BrandIds ?? [],
            CategoryIds = query.CategoryIds ?? [],
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            InStockOnly = query.InStockOnly,
            IncludeInactive = includeInactive,
            Sort = ParseSort(query.Sort),
            Page = query.Page,
            PageSize = query.PageSize
        };

        var result = await products.SearchAsync(filter, ct);
        return new PagedResultDto<ProductListItemDto>(
            result.Items.Select(p => p.ToListItemDto()).ToList(), query.Page, query.PageSize, result.TotalCount);
    }

    public async Task<ProductDetailsDto> GetDetailsAsync(int id, bool includeInactive, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        if (product is null || (!product.IsActive && !includeInactive))
            throw new NotFoundException("Product not found.");

        var related = await products.GetRelatedAsync(product, RelatedProductCount, ct);
        return product.ToDetailsDto(related);
    }

    public async Task<ProductDetailsDto> CreateAsync(ProductUpsertDto dto, CancellationToken ct)
    {
        await EnsureValidAsync(dto, productId: null, ct);

        var product = new Product { CreatedAt = clock.GetUtcNow().UtcDateTime };
        Apply(product, dto);
        products.Add(product);
        await products.SaveChangesAsync(ct);

        return await GetDetailsAsync(product.Id, includeInactive: true, ct);
    }

    public async Task<ProductDetailsDto> UpdateAsync(int id, ProductUpsertDto dto, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct) ?? throw new NotFoundException("Product not found.");
        await EnsureValidAsync(dto, id, ct);

        Apply(product, dto);
        await products.SaveChangesAsync(ct);

        return await GetDetailsAsync(id, includeInactive: true, ct);
    }

    public async Task UpdateStockAsync(int id, int stockQuantity, CancellationToken ct)
    {
        if (stockQuantity < 0)
            throw new BusinessRuleException("Stock cannot be negative.");

        var product = await products.GetByIdAsync(id, ct) ?? throw new NotFoundException("Product not found.");
        product.StockQuantity = stockQuantity;
        await products.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct) ?? throw new NotFoundException("Product not found.");
        if (await products.HasOrdersAsync(id, ct))
            throw new ConflictException("This product appears in past orders. Deactivate it instead of deleting it.");

        products.Remove(product);
        await products.SaveChangesAsync(ct);
    }

    private async Task EnsureValidAsync(ProductUpsertDto dto, int? productId, CancellationToken ct)
    {
        if (await brands.GetByIdAsync(dto.BrandId, ct) is null)
            throw new BusinessRuleException("The selected brand does not exist.");
        if (await categories.GetByIdAsync(dto.CategoryId, ct) is null)
            throw new BusinessRuleException("The selected category does not exist.");
        if (await products.SkuExistsAsync(dto.Sku.Trim().ToUpperInvariant(), productId, ct))
            throw new ConflictException($"SKU '{dto.Sku}' is already used by another product.");
    }

    private static void Apply(Product product, ProductUpsertDto dto)
    {
        product.Sku = dto.Sku.Trim().ToUpperInvariant();
        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.IsActive = dto.IsActive;
        product.BrandId = dto.BrandId;
        product.CategoryId = dto.CategoryId;

        // Images and specifications are owned by the product, so the submitted lists replace the old ones.
        product.Images.Clear();
        product.Images.AddRange(dto.Images.Select((image, index) => new ProductImage
        {
            Url = image.Url.Trim(),
            AltText = string.IsNullOrWhiteSpace(image.AltText) ? product.Name : image.AltText.Trim(),
            SortOrder = index
        }));

        product.Specifications.Clear();
        product.Specifications.AddRange(dto.Specifications.Select(s => new ProductSpecification
        {
            Name = s.Name.Trim(),
            Value = s.Value.Trim()
        }));
    }

    private static ProductSort ParseSort(string? sort) => sort switch
    {
        "price-asc" => ProductSort.PriceAsc,
        "price-desc" => ProductSort.PriceDesc,
        "name" => ProductSort.Name,
        "stock" => ProductSort.StockAsc,
        _ => ProductSort.Newest
    };
}
