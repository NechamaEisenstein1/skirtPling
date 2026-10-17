using Microsoft.AspNetCore.HttpOverrides;
using SkirtPling.Api.Extensions;
using SkirtPling.Api.Middleware;
using SkirtPling.BL;
using SkirtPling.DAL;
using SkirtPling.DAL.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddBusinessLogic();
builder.Services.AddIdentityAndJwt(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddOpenApi();

// Behind a hosting proxy (e.g. Render) TLS ends at the proxy; trust its X-Forwarded-* headers
// so HTTPS redirection and request URLs see the original scheme.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
// In the Docker image the built Angular app is copied into wwwroot and served from the same origin.
// UseRouting comes after the static files so a file request never falls through to the SPA fallback.
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(ServiceCollectionExtensions.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
// Angular routes such as /shop or /admin/orders are client-side; unknown non-API paths return the SPA.
app.MapFallbackToFile("{*path:regex(^(?!api/).*$)}", "index.html");

if (app.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
    await app.InitializeDatabaseAsync();

app.Run();

// Exposes the entry point to the integration tests (WebApplicationFactory<Program>).
public partial class Program;
