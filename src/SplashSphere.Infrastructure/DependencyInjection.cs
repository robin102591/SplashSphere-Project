using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Interfaces;
using SplashSphere.Infrastructure.Auth;
using SplashSphere.Infrastructure.Authentication;
using SplashSphere.Infrastructure.Persistence;
using SplashSphere.Infrastructure.Persistence.Interceptors;
using SplashSphere.Infrastructure.Persistence.Repositories;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core ───────────────────────────────────────────────────────────
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Repositories & Unit of Work ───────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IServicePricingRepository, ServicePricingRepository>();
        services.AddScoped<IServiceCommissionRepository, ServiceCommissionRepository>();

        // ── Domain event publisher & background jobs ──────────────────────────
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        // ── Tenant context ────────────────────────────────────────────────────
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // ── Clerk JWT authentication ──────────────────────────────────────────
        services.AddClerkJwtAuthentication(configuration);

        // ── Clerk backend API (organization management) ───────────────────────
        services.AddScoped<IClerkOrganizationService, ClerkOrganizationService>();

        // ── Hangfire ──────────────────────────────────────────────────────────
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));
        services.AddHangfireServer();

        // ── SignalR ───────────────────────────────────────────────────────────
        services.AddSignalR();

        return services;
    }
}
