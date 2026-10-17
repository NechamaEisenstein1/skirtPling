using SkirtPling.BL.Services;

namespace SkirtPling.Tests.Unit;

public class PricingCalculatorTests
{
    [Fact]
    public void Subtotal_MultipliesPriceByQuantity()
    {
        var subtotal = PricingCalculator.Subtotal([(100m, 2), (49.90m, 1)]);

        Assert.Equal(249.90m, subtotal);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(299.99, 29.90)]
    [InlineData(300, 0)]
    [InlineData(1000, 0)]
    public void ShippingCost_IsFreeFromThreshold(decimal subtotal, decimal expected)
    {
        Assert.Equal(expected, PricingCalculator.ShippingCost(subtotal));
    }
}
