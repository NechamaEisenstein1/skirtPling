using System.Net;
using System.Net.Http.Json;
using SkirtPling.BL.DTOs;
using SkirtPling.Tests.TestSupport;

namespace SkirtPling.Tests.Integration;

public class AuthorizationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("/api/admin/dashboard")]
    [InlineData("/api/admin/orders")]
    [InlineData("/api/admin/users")]
    [InlineData("/api/admin/products")]
    public async Task AdminEndpoints_RejectAnonymousUsers(string url)
    {
        var response = await factory.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/admin/dashboard")]
    [InlineData("/api/admin/users")]
    public async Task AdminEndpoints_RejectCustomers(string url)
    {
        var customer = await factory.CreateNewCustomerClientAsync();

        var response = await customer.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoints_AllowAdmins()
    {
        var admin = await factory.CreateClientForAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);

        var stats = await admin.GetFromJsonAsync<DashboardStatsDto>("/api/admin/dashboard");

        Assert.Equal(20, stats!.TotalProducts);
    }

    [Fact]
    public async Task Customer_CannotCreateProducts()
    {
        var customer = await factory.CreateNewCustomerClientAsync();

        var response = await customer.PostAsJsonAsync("/api/admin/products", new ProductUpsertDto { Sku = "HACK-1", Name = "Hack", Price = 1, BrandId = 1, CategoryId = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Orders_RequireSignIn()
    {
        var response = await factory.CreateClient().GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsGenericError()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginDto(ApiFactory.AdminEmail, "wrong-password"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Invalid email or password", await response.Content.ReadAsStringAsync());
    }
}
