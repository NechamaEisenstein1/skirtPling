using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.External;
using SkirtPling.DAL.Repositories;
using SkirtPling.DAL.Seed;

namespace SkirtPling.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        // SQL Server is the real database. SQLite exists for automated tests and quick demos without SQL Server.
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(connectionString);
            else
                options.UseSqlServer(connectionString);
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();

        services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
        services.Configure<LegacyWarehouseOptions>(configuration.GetSection(LegacyWarehouseOptions.SectionName));
        services.AddScoped<ILegacyWarehouseClient, LegacyWarehouseClient>();

        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.AddScoped<DatabaseSeeder>();
        return services;
    }
}
