using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Seed;

namespace SkirtPling.Api.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Brings the database schema up to date and seeds roles, the dev admin and the demo catalog.
    /// SQL Server uses EF migrations; SQLite (tests/demo) creates the schema directly.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Database.IsSqlServer())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }
}
