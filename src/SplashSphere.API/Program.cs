using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using SplashSphere.API.Endpoints;
using SplashSphere.API.Infrastructure;
using SplashSphere.Application;
using SplashSphere.Infrastructure;
using SplashSphere.Infrastructure.Authentication;
using SplashSphere.Infrastructure.Hubs;
using SplashSphere.Infrastructure.Jobs;
using SplashSphere.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

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

// ── OpenAPI ───────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── ProblemDetails + exception handling ──────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ── CORS ──────────────────────────────────────────────────────────────────────
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

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Dev-only: OpenAPI, Scalar, migrations, seed ───────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(app.Services);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors();
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

app.Run();
