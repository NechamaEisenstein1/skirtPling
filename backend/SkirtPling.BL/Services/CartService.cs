using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.DAL.Entities;
using SkirtPling.DAL.Repositories;

namespace SkirtPling.BL.Services;

public class CartService(ICartRepository carts, IProductRepository products, TimeProvider clock) : ICartService
{
    public const int MaxQuantityPerLine = 20;

    public async Task<CartDto> GetAsync(CartOwner owner, CancellationToken ct) =>
        ToDto(await FindCartAsync(owner, ct));

    public async Task<CartDto> AddItemAsync(CartOwner owner, AddCartItemDto dto, CancellationToken ct)
    {
        var product = await GetPurchasableProductAsync(dto.ProductId, ct);
        var cart = await FindCartAsync(owner, ct) ?? CreateCart(owner);

        var line = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        var newQuantity = (line?.Quantity ?? 0) + dto.Quantity;
        EnsureQuantityAllowed(product, newQuantity);

        if (line is null)
            cart.Items.Add(new CartItem { ProductId = product.Id, Product = product, Quantity = newQuantity });
        else
            line.Quantity = newQuantity;

        return await SaveAsync(cart, ct);
    }

    public async Task<CartDto> UpdateItemAsync(CartOwner owner, int productId, int quantity, CancellationToken ct)
    {
        var cart = await FindCartAsync(owner, ct) ?? throw new NotFoundException("Cart is empty.");
        var line = cart.Items.FirstOrDefault(i => i.ProductId == productId) ?? throw new NotFoundException("Item is not in the cart.");

        EnsureQuantityAllowed(line.Product, quantity);
        line.Quantity = quantity;
        return await SaveAsync(cart, ct);
    }

    public async Task<CartDto> RemoveItemAsync(CartOwner owner, int productId, CancellationToken ct)
    {
        var cart = await FindCartAsync(owner, ct) ?? throw new NotFoundException("Cart is empty.");
        var line = cart.Items.FirstOrDefault(i => i.ProductId == productId) ?? throw new NotFoundException("Item is not in the cart.");

        cart.Items.Remove(line);
        return await SaveAsync(cart, ct);
    }

    public async Task<CartDto> MergeGuestCartAsync(string userId, Guid guestId, CancellationToken ct)
    {
        var guestCart = await carts.GetByGuestIdAsync(guestId, ct);
        var userCart = await carts.GetByUserIdAsync(userId, ct);
        if (guestCart is null)
            return ToDto(userCart);

        userCart ??= CreateCart(new CartOwner(userId, null));
        foreach (var guestLine in guestCart.Items)
        {
            var line = userCart.Items.FirstOrDefault(i => i.ProductId == guestLine.ProductId);
            // Never merge past what is in stock or the per-line limit; the shopper can adjust in the cart.
            var limit = Math.Min(MaxQuantityPerLine, guestLine.Product.StockQuantity);
            if (line is null)
                userCart.Items.Add(new CartItem { ProductId = guestLine.ProductId, Product = guestLine.Product, Quantity = Math.Min(guestLine.Quantity, limit) });
            else
                line.Quantity = Math.Max(line.Quantity, Math.Min(line.Quantity + guestLine.Quantity, limit));
        }
        userCart.Items.RemoveAll(i => i.Quantity <= 0);

        carts.Remove(guestCart);
        return await SaveAsync(userCart, ct);
    }

    private async Task<Cart?> FindCartAsync(CartOwner owner, CancellationToken ct)
    {
        if (owner.UserId is not null)
            return await carts.GetByUserIdAsync(owner.UserId, ct);
        if (owner.GuestId is not null)
            return await carts.GetByGuestIdAsync(owner.GuestId.Value, ct);
        throw new BusinessRuleException("A cart id is required for guests.");
    }

    private Cart CreateCart(CartOwner owner)
    {
        var cart = new Cart { UserId = owner.UserId, GuestId = owner.UserId is null ? owner.GuestId : null };
        carts.Add(cart);
        return cart;
    }

    private async Task<Product> GetPurchasableProductAsync(int productId, CancellationToken ct)
    {
        var product = (await products.GetByIdsAsync([productId], ct)).SingleOrDefault();
        if (product is null || !product.IsActive)
            throw new NotFoundException("Product not found.");
        return product;
    }

    private static void EnsureQuantityAllowed(Product product, int quantity)
    {
        if (quantity < 1 || quantity > MaxQuantityPerLine)
            throw new BusinessRuleException($"Quantity must be between 1 and {MaxQuantityPerLine}.");
        if (quantity > product.StockQuantity)
            throw new ConflictException(product.StockQuantity == 0
                ? $"'{product.Name}' is out of stock."
                : $"Only {product.StockQuantity} of '{product.Name}' left in stock.");
    }

    private async Task<CartDto> SaveAsync(Cart cart, CancellationToken ct)
    {
        cart.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await carts.SaveChangesAsync(ct);
        return ToDto(cart);
    }

    /// <summary>Prices always come from the current product data, never from what the client sent.</summary>
    internal static CartDto ToDto(Cart? cart)
    {
        var items = (cart?.Items ?? [])
            .Select(i => new CartItemDto(
                i.ProductId, i.Product.Sku, i.Product.Name,
                i.Product.Images.OrderBy(img => img.SortOrder).FirstOrDefault()?.Url,
                i.Product.Price, i.Quantity, i.Product.StockQuantity))
            .ToList();

        var subtotal = PricingCalculator.Subtotal(items.Select(i => (i.UnitPrice, i.Quantity)));
        var shipping = PricingCalculator.ShippingCost(subtotal);
        return new CartDto(items, subtotal, shipping, subtotal + shipping);
    }
}
