using System.Net;
using System.Net.Http.Json;
using SkirtPling.BL.DTOs;
using SkirtPling.DAL.External;
using SkirtPling.Tests.TestSupport;

namespace SkirtPling.Tests.Integration;

public class CheckoutFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly ShippingAddressDto Address = new("Dina Levi", "0501234567", "Herzl 1", "Jerusalem", "9100000", "Israel");

    [Fact]
    public async Task GuestCart_MergesOnSignIn_AndCheckoutCreatesOrder()
    {
        var guestId = Guid.NewGuid().ToString();
        var guest = factory.CreateClient();
        guest.DefaultRequestHeaders.Add("X-Guest-Cart-Id", guestId);
        var product = (await guest.GetFromJsonAsync<PagedResultDto<ProductListItemDto>>("/api/products?inStockOnly=true&pageSize=1"))!.Items[0];
        (await guest.PostAsJsonAsync("/api/cart/items", new AddCartItemDto(product.Id, 2))).EnsureSuccessStatusCode();

        var customer = await factory.CreateNewCustomerClientAsync();
        customer.DefaultRequestHeaders.Add("X-Guest-Cart-Id", guestId);
        var merged = await (await customer.PostAsync("/api/cart/merge", null)).Content.ReadFromJsonAsync<CartDto>();
        Assert.Equal(2, merged!.ItemCount);

        var response = await customer.PostAsJsonAsync("/api/orders", new CheckoutDto(Address, MockPaymentGateway.ApprovedToken));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal("Paid", order!.Status);
        Assert.Equal(product.Price * 2, order.Subtotal);

        var history = await customer.GetFromJsonAsync<PagedResultDto<OrderSummaryDto>>("/api/orders");
        Assert.Single(history!.Items);
        var after = await customer.GetFromJsonAsync<ProductDetailsDto>($"/api/products/{product.Id}");
        Assert.Equal(product.StockQuantity - 2, after!.StockQuantity);
    }

    [Fact]
    public async Task Customer_CannotReadAnotherCustomersOrder()
    {
        var owner = await factory.CreateNewCustomerClientAsync();
        (await owner.PostAsJsonAsync("/api/cart/items", new AddCartItemDto(1, 1))).EnsureSuccessStatusCode();
        var order = await (await owner.PostAsJsonAsync("/api/orders", new CheckoutDto(Address, MockPaymentGateway.ApprovedToken)))
            .Content.ReadFromJsonAsync<OrderDto>();

        var intruder = await factory.CreateNewCustomerClientAsync();
        var response = await intruder.GetAsync($"/api/orders/{order!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WithDeclinedPayment_ReturnsBadRequest()
    {
        var customer = await factory.CreateNewCustomerClientAsync();
        (await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemDto(1, 1))).EnsureSuccessStatusCode();

        var response = await customer.PostAsJsonAsync("/api/orders", new CheckoutDto(Address, MockPaymentGateway.DeclinedToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("declined", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Checkout_WithMissingAddress_FailsValidation()
    {
        var customer = await factory.CreateNewCustomerClientAsync();
        var invalid = new { shippingAddress = new { fullName = "", phone = "x" }, paymentToken = "" };

        var response = await customer.PostAsJsonAsync("/api/orders", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("errors", await response.Content.ReadAsStringAsync());
    }
}
