using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Services;
using SkirtPling.DAL.Repositories;
using SkirtPling.Tests.TestSupport;

namespace SkirtPling.Tests.Unit;

public class OrderServiceTests : IDisposable
{
    private static readonly ShippingAddressDto Address = new("Dina Levi", "0501234567", "Herzl 1", "Jerusalem", "9100000", "Israel");

    private readonly TestDatabase db = new();
    private readonly CartService cartService;
    private readonly CartOwner customer;

    public OrderServiceTests()
    {
        cartService = new CartService(new CartRepository(db.Context), new ProductRepository(db.Context), TimeProvider.System);
        customer = new CartOwner(db.Customer.Id, null);
    }

    private OrderService CreateService(FakePaymentGateway gateway) =>
        new(new OrderRepository(db.Context), new CartRepository(db.Context), gateway, TimeProvider.System);

    [Fact]
    public async Task PlaceOrder_ApprovedPayment_CreatesPaidOrderAndReducesStock()
    {
        await cartService.AddItemAsync(customer, new AddCartItemDto(db.Skirt.Id, 3), CancellationToken.None);
        var gateway = new FakePaymentGateway(approve: true);

        var order = await CreateService(gateway).PlaceOrderAsync(db.Customer.Id, "dina@example.com", new CheckoutDto(Address, "tok"), CancellationToken.None);

        Assert.Equal("Paid", order.Status);
        Assert.Equal(300m, order.Subtotal);
        Assert.Equal(0m, order.ShippingCost);
        Assert.Equal(300m, gateway.Requests.Single().Amount);
        Assert.Equal(7, db.StockOf(db.Skirt));
        Assert.Empty((await cartService.GetAsync(customer, CancellationToken.None)).Items);
    }

    [Fact]
    public async Task PlaceOrder_DeclinedPayment_RestoresStockAndKeepsCart()
    {
        await cartService.AddItemAsync(customer, new AddCartItemDto(db.Skirt.Id, 2), CancellationToken.None);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService(new FakePaymentGateway(approve: false))
                .PlaceOrderAsync(db.Customer.Id, "dina@example.com", new CheckoutDto(Address, "tok"), CancellationToken.None));

        Assert.Equal(10, db.StockOf(db.Skirt));
        Assert.Single((await cartService.GetAsync(customer, CancellationToken.None)).Items);
    }

    [Fact]
    public async Task PlaceOrder_EmptyCart_Throws()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService(new FakePaymentGateway(approve: true))
                .PlaceOrderAsync(db.Customer.Id, "dina@example.com", new CheckoutDto(Address, "tok"), CancellationToken.None));
    }

    [Fact]
    public async Task PlaceOrder_StockSoldMeanwhile_ThrowsAndChargesNothing()
    {
        await cartService.AddItemAsync(customer, new AddCartItemDto(db.LastOne.Id, 1), CancellationToken.None);
        db.LastOne.StockQuantity = 0; // someone else bought the last unit
        await db.Context.SaveChangesAsync();
        var gateway = new FakePaymentGateway(approve: true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService(gateway).PlaceOrderAsync(db.Customer.Id, "dina@example.com", new CheckoutDto(Address, "tok"), CancellationToken.None));

        Assert.Empty(gateway.Requests);
    }

    [Fact]
    public async Task GetForUser_OtherCustomersOrder_IsNotFound()
    {
        await cartService.AddItemAsync(customer, new AddCartItemDto(db.Skirt.Id, 1), CancellationToken.None);
        var service = CreateService(new FakePaymentGateway(approve: true));
        var order = await service.PlaceOrderAsync(db.Customer.Id, "dina@example.com", new CheckoutDto(Address, "tok"), CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetForUserAsync("someone-else", order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatus_CancelPaidOrder_ReturnsStock()
    {
        await cartService.AddItemAsync(customer, new AddCartItemDto(db.Skirt.Id, 4), CancellationToken.None);
        var service = CreateService(new FakePaymentGateway(approve: true));
        var order = await service.PlaceOrderAsync(db.Customer.Id, "dina@example.com", new CheckoutDto(Address, "tok"), CancellationToken.None);

        var cancelled = await service.UpdateStatusAsync(order.Id, "Cancelled", CancellationToken.None);

        Assert.Equal("Cancelled", cancelled.Status);
        Assert.Equal(10, db.StockOf(db.Skirt));
    }

    [Fact]
    public async Task UpdateStatus_InvalidTransition_Throws()
    {
        await cartService.AddItemAsync(customer, new AddCartItemDto(db.Skirt.Id, 1), CancellationToken.None);
        var service = CreateService(new FakePaymentGateway(approve: true));
        var order = await service.PlaceOrderAsync(db.Customer.Id, "dina@example.com", new CheckoutDto(Address, "tok"), CancellationToken.None);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateStatusAsync(order.Id, "Delivered", CancellationToken.None));
    }

    public void Dispose() => db.Dispose();
}
