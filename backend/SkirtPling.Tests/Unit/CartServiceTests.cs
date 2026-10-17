using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Services;
using SkirtPling.DAL.Repositories;
using SkirtPling.Tests.TestSupport;

namespace SkirtPling.Tests.Unit;

public class CartServiceTests : IDisposable
{
    private readonly TestDatabase db = new();
    private readonly CartService service;
    private readonly CartOwner guest = new(null, Guid.NewGuid());

    public CartServiceTests()
    {
        service = new CartService(new CartRepository(db.Context), new ProductRepository(db.Context), TimeProvider.System);
    }

    [Fact]
    public async Task AddItem_CalculatesTotalsFromServerPrices()
    {
        var cart = await service.AddItemAsync(guest, new AddCartItemDto(db.Skirt.Id, 2), CancellationToken.None);

        Assert.Equal(2, cart.ItemCount);
        Assert.Equal(200m, cart.Subtotal);
        Assert.Equal(PricingCalculator.StandardShippingCost, cart.ShippingCost);
        Assert.Equal(200m + PricingCalculator.StandardShippingCost, cart.Total);
    }

    [Fact]
    public async Task AddItem_SameProductTwice_IncreasesQuantity()
    {
        await service.AddItemAsync(guest, new AddCartItemDto(db.Skirt.Id, 1), CancellationToken.None);
        var cart = await service.AddItemAsync(guest, new AddCartItemDto(db.Skirt.Id, 2), CancellationToken.None);

        Assert.Single(cart.Items);
        Assert.Equal(3, cart.Items[0].Quantity);
    }

    [Fact]
    public async Task AddItem_MoreThanStock_Throws()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            service.AddItemAsync(guest, new AddCartItemDto(db.LastOne.Id, 2), CancellationToken.None));
    }

    [Fact]
    public async Task AddItem_InactiveProduct_IsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddItemAsync(guest, new AddCartItemDto(db.Hidden.Id, 1), CancellationToken.None));
    }

    [Fact]
    public async Task MergeGuestCart_CombinesLinesWithinStock()
    {
        var user = new CartOwner(db.Customer.Id, null);
        await service.AddItemAsync(user, new AddCartItemDto(db.Skirt.Id, 8), CancellationToken.None);
        await service.AddItemAsync(guest, new AddCartItemDto(db.Skirt.Id, 5), CancellationToken.None);
        await service.AddItemAsync(guest, new AddCartItemDto(db.LastOne.Id, 1), CancellationToken.None);

        var merged = await service.MergeGuestCartAsync(db.Customer.Id, guest.GuestId!.Value, CancellationToken.None);

        Assert.Equal(10, merged.Items.Single(i => i.ProductId == db.Skirt.Id).Quantity); // capped at stock
        Assert.Equal(1, merged.Items.Single(i => i.ProductId == db.LastOne.Id).Quantity);
        Assert.Empty((await service.GetAsync(guest, CancellationToken.None)).Items);
    }

    public void Dispose() => db.Dispose();
}
