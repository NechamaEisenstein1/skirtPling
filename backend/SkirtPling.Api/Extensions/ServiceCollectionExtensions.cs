using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SkirtPling.Api.Auth;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "Frontend";

    public static IServiceCollection AddIdentityAndJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
        if (jwt.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be configured with at least 32 characters (use user-secrets or an environment variable).");

        services.Configure<JwtOptions>(jwtSection);
        services.AddSingleton<JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim names as issued ("sub", "email") instead of mapping them to long SOAP-style URIs.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType = "name",
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
            policy.WithOrigins(origins)
                .WithHeaders("Authorization", "Content-Type", CartHeaders.GuestCartId)
                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")));
        return services;
    }
}
