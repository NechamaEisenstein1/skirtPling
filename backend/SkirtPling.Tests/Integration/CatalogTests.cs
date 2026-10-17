using System.Net;
using System.Net.Http.Json;
using SkirtPling.BL.DTOs;
using SkirtPling.Tests.TestSupport;

namespace SkirtPling.Tests.Integration;

public class CatalogTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Search_FiltersSortsAndPages()
    {
        var page = await factory.CreateClient()
            .GetFromJsonAsync<PagedResultDto<ProductListItemDto>>("/api/products?categoryIds=5&sort=price-asc&pageSize=2");

        Assert.Equal(2, page!.Items.Count);
        Assert.True(page.TotalCount >= 4);
        Assert.All(page.Items, p => Assert.Equal("Denim", p.CategoryName));
        Assert.True(page.Items[0].Price <= page.Items[1].Price);
    }

    [Fact]
    public async Task Details_IncludeImagesSpecsAndRelatedProducts()
    {
        var product = await factory.CreateClient().GetFromJsonAsync<ProductDetailsDto>("/api/products/1");

        Assert.NotEmpty(product!.Images);
        Assert.NotEmpty(product.Specifications);
        Assert.NotEmpty(product.RelatedProducts);
    }

    [Fact]
    public async Task Admin_CanCreateUpdateAndDeleteProduct()
    {
        var admin = await factory.CreateClientForAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        var dto = new ProductUpsertDto
        {
            Sku = "sp-test-100", Name = "Test Skirt", Price = 99.90m, StockQuantity = 3, BrandId = 1, CategoryId = 1,
            Images = [new ProductImageUpsertDto("/images/products/sp-pl-001-1.svg", null)],
            Specifications = [new ProductSpecificationUpsertDto("Color", "Red")]
        };

        var created = await (await admin.PostAsJsonAsync("/api/admin/products", dto)).Content.ReadFromJsonAsync<ProductDetailsDto>();
        Assert.Equal("SP-TEST-100", created!.Sku);

        var duplicate = await admin.PostAsJsonAsync("/api/admin/products", dto);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var updated = await (await admin.PutAsJsonAsync($"/api/admin/products/{created.Id}", dto with { Name = "Renamed Skirt", IsActive = false }))
            .Content.ReadFromJsonAsync<ProductDetailsDto>();
        Assert.Equal("Renamed Skirt", updated!.Name);

        var publicView = await factory.CreateClient().GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, publicView.StatusCode); // inactive products are hidden from shoppers

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/admin/products/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Admin_CannotDeleteBrandThatHasProducts()
    {
        var admin = await factory.CreateClientForAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);

        var response = await admin.DeleteAsync("/api/admin/brands/1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_IsHealthy()
    {
        var response = await factory.CreateClient().GetStringAsync("/health");

        Assert.Equal("Healthy", response);
    }
}
