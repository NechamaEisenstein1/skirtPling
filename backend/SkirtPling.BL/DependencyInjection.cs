using Microsoft.Extensions.DependencyInjection;
using SkirtPling.BL.Interfaces;
using SkirtPling.BL.Services;

namespace SkirtPling.BL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ILegacyInventoryService, LegacyInventoryService>();
        return services;
    }
}
