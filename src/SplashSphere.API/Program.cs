using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using SplashSphere.API.Endpoints;
using SplashSphere.API.Infrastructure;
using SplashSphere.Application;
using SplashSphere.Infrastructure;
using SplashSphere.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// In Development, override auth with a pass-through handler so every request
// is auto-authenticated as the seed tenant — no Clerk token needed.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, _ => { });
}

// OpenAPI
builder.Services.AddOpenApi();

// ProblemDetails + exception handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Auto-migrate and seed
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(app.Services);
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (dev only)
if (app.Environment.IsDevelopment())
    app.UseHangfireDashboard("/hangfire");

// ── Endpoints ────────────────────────────────────────────────────────────────
app.MapBranchEndpoints();
app.MapVehicleTypeEndpoints();
app.MapSizeEndpoints();
app.MapMakeEndpoints();
app.MapModelEndpoints();
app.MapServiceCategoryEndpoints();
app.MapMerchandiseCategoryEndpoints();
app.MapServiceEndpoints();
app.MapPackageEndpoints();
app.MapCustomerEndpoints();
app.MapCarEndpoints();
app.MapEmployeeEndpoints();
app.MapMerchandiseEndpoints();
app.MapQueueEndpoints();
app.MapTransactionEndpoints();

app.Run();
