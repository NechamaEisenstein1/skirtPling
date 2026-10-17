using System.Text.RegularExpressions;

namespace SkirtPling.BL.Services;

public static partial class SlugGenerator
{
    /// <summary>"Linen &amp; Loom" becomes "linen-loom".</summary>
    public static string FromName(string name) =>
        NonAlphanumeric().Replace(name.Trim().ToLowerInvariant(), "-").Trim('-');

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
