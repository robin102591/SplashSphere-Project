using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Interfaces;
using SplashSphere.Infrastructure.Auth;
using SplashSphere.Infrastructure.Auth.Connect;
using SplashSphere.Infrastructure.Authentication;
using SplashSphere.Infrastructure.Persistence;
using SplashSphere.Infrastructure.Persistence.Interceptors;
using SplashSphere.Infrastructure.Persistence.Repositories;
using SplashSphere.Infrastructure.Jobs;
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
        services.AddScoped<AuditLogInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<AuditLogInterceptor>());
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
        // Default Bearer scheme = Clerk (admin/POS). ConnectJwt is registered
        // as a second scheme for the Customer Connect app — see ConnectJwtSetup.
        services
            .AddClerkJwtAuthentication(configuration)
            .AddConnectJwtAuthentication(configuration);

        // ── Clerk backend API (organization management) ───────────────────────
        services.AddScoped<IClerkOrganizationService, ClerkOrganizationService>();

        // ── Notification service (persist + broadcast) ──────────────────────
        services.AddScoped<INotificationService, NotificationService>();

        // ── Plan enforcement ────────────────────────────────────────────────
        services.AddScoped<IPlanEnforcementService, PlanEnforcementService>();

        // ── Data migration (CSV/Excel import) ──────────────────────────────
        services.AddScoped<IDataMigrationService, DataMigrationService>();

        // ── Payment gateway ──────────────────────────────────────────────────
        // Uses PayMongo when secret key is configured, otherwise falls back to mock.
        if (!string.IsNullOrEmpty(configuration["PayMongo:SecretKey"]))
        {
            services.AddHttpClient("PayMongo");
            services.AddScoped<IPaymentGateway, ExternalServices.PayMongoPaymentGateway>();
        }
        else
        {
            services.AddScoped<IPaymentGateway, ExternalServices.MockPaymentGateway>();
        }

        // ── Email service ───────────────────────────────────────────────────
        // Uses Resend when API key is configured, otherwise logs to console.
        if (!string.IsNullOrEmpty(configuration["Resend:ApiKey"]))
        {
            services.AddHttpClient("Resend");
            services.AddScoped<IEmailService, ExternalServices.ResendEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, ExternalServices.MockEmailService>();
        }

        // ── SMS service ─────────────────────────────────────────────────────
        // Uses Semaphore (PH gateway) when API key is configured, otherwise logs to console.
        if (!string.IsNullOrEmpty(configuration["Semaphore:ApiKey"]))
        {
            services.AddHttpClient("Semaphore");
            services.AddScoped<ISmsService, ExternalServices.SemaphoreSmsService>();
        }
        else
        {
            services.AddScoped<ISmsService, ExternalServices.MockSmsService>();
        }

        // ── Connect (customer app) OTP ──────────────────────────────────────
        // Distributed cache — in-memory for dev, swap to AddStackExchangeRedisCache in prod.
        services.AddDistributedMemoryCache();
        services.AddScoped<IOtpSender, OtpSender>();
        services.AddScoped<IOtpStore, DistributedCacheOtpStore>();

        // ── Connect (customer app) JWT ──────────────────────────────────────
        services.AddScoped<IConnectTokenService, ConnectTokenService>();

        // ── Background job services ───────────────────────────────────────────
        services.AddTransient<PayrollJobService>();
        services.AddTransient<InventoryJobService>();
        services.AddTransient<TransactionJobService>();
        services.AddTransient<QueueJobService>();
        services.AddTransient<BillingJobService>();
        services.AddTransient<ExpenseJobService>();
        services.AddTransient<FranchiseJobService>();

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
