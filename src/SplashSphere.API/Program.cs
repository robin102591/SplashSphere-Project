using System.Reflection;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using SplashSphere.API.Endpoints;
using SplashSphere.API.Endpoints.Connect;
using SplashSphere.API.Infrastructure;
using SplashSphere.Application;
using SplashSphere.Infrastructure;
using SplashSphere.Infrastructure.Authentication;
using SplashSphere.Infrastructure.Hubs;
using SplashSphere.Infrastructure.Jobs;
using SplashSphere.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Railway injects PORT — Kestrel must listen on it. In local dev PORT is unset,
// so we fall back to launchSettings.json (http://localhost:5221 / https://localhost:7170).
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── Serilog ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

// ── Application + Infrastructure ─────────────────────────────────────────────
// Pass Infrastructure marker so MediatR scans both assemblies for notification handlers
builder.Services.AddApplication(typeof(SplashSphere.Infrastructure.DependencyInjection));
builder.Services.AddInfrastructure(builder.Configuration);

// ── Dev-only: override auth with pass-through handler ─────────────────────────
// Auto-authenticates every request as the seed tenant so no Clerk token is needed.
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, _ => { });
}

// ── OpenAPI + Swagger ────────────────────────────────────────────────────────
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SplashSphere API",
        Version = "v1",
        Description = "Multi-tenant car wash management platform API for the Philippine market.",
        Contact = new OpenApiContact
        {
            Name = "LezanobTech",
            Email = "dev@splashsphere.ph"
        }
    });

    // JWT Bearer authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Clerk JWT token. Format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Include XML documentation comments
    var xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

// ── ProblemDetails + exception handling ──────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("SplashSphere", policy =>
    {
        var origins = builder.Configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? [];

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // Required for SignalR
    });
});

// In Program.cs — register health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: ["db", "ready"]);

builder.Services.AddAuthorization();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// Health check endpoint (Railway uses this)
app.MapHealthChecks("/health");

// ── OpenAPI, Scalar, Swagger UI ──────────────────────────────────────────────
app.MapOpenApi();
app.MapScalarApiReference();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SplashSphere API v1");
    c.RoutePrefix = "docs";
});

// ── Dev-only: auto-migrate + seed ────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(app.Services);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors("SplashSphere");
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();

// Resolves internal UserId from ClerkUserId; enforces onboarding gate for
// tenantless users (see TenantResolutionMiddleware for allowed-path list).
app.UseMiddleware<TenantResolutionMiddleware>();

// Enforces plan-based feature gates via [RequiresFeature(...)] endpoint attributes.
app.UseMiddleware<PlanEnforcementMiddleware>();

app.UseAuthorization();

// ── Hangfire dashboard (dev only) ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
    app.UseHangfireDashboard("/hangfire");

// ── Recurring jobs (Manila timezone) ─────────────────────────────────────────
app.UseRecurringJobs();

// ── SignalR hub ───────────────────────────────────────────────────────────────
app.MapHub<SplashSphereHub>("/hubs/notifications");

// ── API endpoints ─────────────────────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapOnboardingEndpoints();
app.MapWebhookEndpoints();

app.MapBranchEndpoints();
app.MapPosStationEndpoints();
app.MapDisplayEndpoints();
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
app.MapPayrollEndpoints();
app.MapCashAdvanceEndpoints();
app.MapPricingModifierEndpoints();
app.MapDashboardEndpoints();
app.MapReportEndpoints();
app.MapShiftEndpoints();
app.MapShiftSettingsEndpoints();
app.MapPayrollSettingsEndpoints();
app.MapSearchEndpoints();
app.MapNotificationEndpoints();
app.MapAttendanceEndpoints();
app.MapBillingEndpoints();
app.MapExpenseEndpoints();
app.MapAuditLogEndpoints();
app.MapLoyaltyEndpoints();
app.MapFranchiseEndpoints();
app.MapImportEndpoints();
app.MapSupplyEndpoints();
app.MapStockMovementEndpoints();
app.MapServiceUsageEndpoints();
app.MapPurchaseOrderEndpoints();
app.MapSupplierEndpoints();
app.MapEquipmentEndpoints();
app.MapBookingSettingEndpoints();
app.MapBookingAdminEndpoints();
app.MapSettingsEndpoints();

// ── Customer Connect ──────────────────────────────────────────────────────────
app.MapConnectAuthEndpoints();
app.MapConnectProfileEndpoints();
app.MapConnectCatalogueEndpoints();
app.MapConnectDiscoveryEndpoints();
app.MapConnectServicesEndpoints();
app.MapConnectBookingEndpoints();
app.MapConnectLoyaltyEndpoints();
app.MapConnectReferralEndpoints();
app.MapConnectQueueEndpoints();
app.MapConnectHistoryEndpoints();

app.Run();

// Required for WebApplicationFactory<Program> in integration tests.
// Top-level statements generate an implicit Program class — this makes it accessible.
public partial class Program;
