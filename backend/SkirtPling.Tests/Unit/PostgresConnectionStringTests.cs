using Npgsql;
using SkirtPling.DAL.Context;

namespace SkirtPling.Tests.Unit;

public class PostgresConnectionStringTests
{
    [Fact]
    public void Normalize_ConvertsNeonStyleUrl()
    {
        var result = new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(
            "postgresql://shop_owner:p%40ss@ep-cool-name.eu-central-1.aws.neon.tech/skirtpling?sslmode=require&channel_binding=require"));

        Assert.Equal("ep-cool-name.eu-central-1.aws.neon.tech", result.Host);
        Assert.Equal(5432, result.Port);
        Assert.Equal("skirtpling", result.Database);
        Assert.Equal("shop_owner", result.Username);
        Assert.Equal("p@ss", result.Password);
        Assert.Equal(SslMode.Require, result.SslMode);
    }

    [Fact]
    public void Normalize_KeepsKeyValueStrings()
    {
        const string connectionString = "Host=localhost;Database=shop;Username=postgres";

        Assert.Equal(connectionString, PostgresConnectionString.Normalize(connectionString));
    }
}
