using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using SkirtPling.BL.DTOs;

namespace SkirtPling.Tests.TestSupport;

/// <summary>
/// Runs the real API in memory on a temporary SQLite file, seeded with the demo catalog and the dev admin
/// from appsettings.Development.json.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@skirtpling.local";
    public const string AdminPassword = "Admin123!dev";

    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"skirtpling-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={databasePath}");
        builder.UseSetting("Database:InitializeOnStartup", "true");
        builder.UseSetting("Seed:SeedCatalog", "true");
        builder.UseSetting("Seed:AdminEmail", AdminEmail);
        builder.UseSetting("Seed:AdminPassword", AdminPassword);
    }

    public async Task<HttpClient> CreateClientForAsync(string email, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, password));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    public async Task<HttpClient> CreateNewCustomerClientAsync()
    {
        var email = $"customer-{Guid.NewGuid():N}@example.com";
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto(email, "Password123", "Test Customer"));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(databasePath))
            File.Delete(databasePath);
    }
}
