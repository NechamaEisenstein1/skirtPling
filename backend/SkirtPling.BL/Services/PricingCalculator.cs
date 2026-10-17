namespace SkirtPling.BL.Services;

/// <summary>The store's pricing rules, kept in one place so the cart and checkout always agree.</summary>
public static class PricingCalculator
{
    public const string Currency = "ILS";
    public const decimal FreeShippingThreshold = 300m;
    public const decimal StandardShippingCost = 29.90m;

    public static decimal Subtotal(IEnumerable<(decimal UnitPrice, int Quantity)> lines) =>
        lines.Sum(l => l.UnitPrice * l.Quantity);

    public static decimal ShippingCost(decimal subtotal) =>
        subtotal == 0 || subtotal >= FreeShippingThreshold ? 0m : StandardShippingCost;
}
