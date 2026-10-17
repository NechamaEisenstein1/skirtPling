namespace SkirtPling.DAL.Seed;

/// <summary>Development-only seeding settings. Leave AdminPassword empty in production configuration.</summary>
public class SeedOptions
{
    public const string SectionName = "Seed";

    public bool SeedCatalog { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
