using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Seed;

/// <summary>Creates roles, an optional development admin and the demo catalog. Safe to run on every start.</summary>
public class DatabaseSeeder(
    AppDbContext db,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<SeedOptions> options,
    ILogger<DatabaseSeeder> logger)
{
    private readonly SeedOptions settings = options.Value;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await SeedAdminAsync();

        if (settings.SeedCatalog && !await db.Products.AnyAsync(ct))
        {
            db.Products.AddRange(CatalogSeedData.CreateProducts());
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded demo catalog");
        }
    }

    private async Task SeedAdminAsync()
    {
        if (string.IsNullOrWhiteSpace(settings.AdminEmail) || string.IsNullOrWhiteSpace(settings.AdminPassword))
            return;
        if (await userManager.FindByEmailAsync(settings.AdminEmail) is not null)
            return;

        var admin = new AppUser
        {
            UserName = settings.AdminEmail,
            Email = settings.AdminEmail,
            FullName = "Store Admin",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(admin, settings.AdminPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("Could not seed admin user: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }
        await userManager.AddToRolesAsync(admin, Roles.All);
        logger.LogInformation("Seeded development admin {Email}", settings.AdminEmail);
    }
}
