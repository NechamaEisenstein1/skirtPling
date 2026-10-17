using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;

namespace SkirtPling.Tests.TestSupport;

/// <summary>
/// The app's context on SQLite, so tests need no SQL Server. SQLite has no decimal type and can't sort or
/// sum decimals, so money is stored as REAL here.
/// </summary>
public sealed class SqliteAppDbContext(DbContextOptions<SqliteAppDbContext> options) : AppDbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder builder) =>
        builder.Properties<decimal>().HaveConversion<double>();
}
