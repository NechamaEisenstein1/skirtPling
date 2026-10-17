using SkirtPling.BL.DTOs;
using SkirtPling.DAL.Entities;

namespace SkirtPling.BL.Mapping;

public static class CatalogMapping
{
    public static BrandDto ToDto(this Brand brand) => new(brand.Id, brand.Name, brand.Slug, brand.Description);

    public static CategoryDto ToDto(this Category category) => new(category.Id, category.Name, category.Slug);

    public static ProductListItemDto ToListItemDto(this Product product) => new(
        product.Id, product.Sku, product.Name, product.Price, product.StockQuantity, product.IsActive,
        product.Brand.Name, product.Category.Name,
        product.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url);

    public static ProductDetailsDto ToDetailsDto(this Product product, IEnumerable<Product> related) => new(
        product.Id, product.Sku, product.Name, product.Description, product.Price, product.StockQuantity, product.IsActive,
        product.Brand.ToDto(), product.Category.ToDto(),
        product.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto(i.Id, i.Url, i.AltText)).ToList(),
        product.Specifications.Select(s => new ProductSpecificationDto(s.Name, s.Value)).ToList(),
        related.Select(p => p.ToListItemDto()).ToList());
}
