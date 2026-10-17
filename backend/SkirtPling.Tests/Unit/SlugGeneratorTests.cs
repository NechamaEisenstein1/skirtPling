using SkirtPling.BL.Services;

namespace SkirtPling.Tests.Unit;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Linen & Loom", "linen-loom")]
    [InlineData("  A-Line  ", "a-line")]
    [InlineData("Velvet Row 2026", "velvet-row-2026")]
    public void FromName_ProducesUrlFriendlySlug(string name, string expected)
    {
        Assert.Equal(expected, SlugGenerator.FromName(name));
    }
}
