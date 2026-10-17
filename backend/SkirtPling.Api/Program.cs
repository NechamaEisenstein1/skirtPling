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

var app = builder.Build();

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
app.UseCors(ServiceCollectionExtensions.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (app.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
    await app.InitializeDatabaseAsync();

app.Run();

// Exposes the entry point to the integration tests (WebApplicationFactory<Program>).
public partial class Program;
