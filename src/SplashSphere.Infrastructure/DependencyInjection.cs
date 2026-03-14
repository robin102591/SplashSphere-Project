using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Interfaces;
using SplashSphere.Infrastructure.Auth;
using SplashSphere.Infrastructure.Persistence;
using SplashSphere.Infrastructure.Persistence.Interceptors;
using SplashSphere.Infrastructure.Persistence.Repositories;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repositories & Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IServicePricingRepository, ServicePricingRepository>();
        services.AddScoped<IServiceCommissionRepository, ServiceCommissionRepository>();

        // Domain event publisher & background jobs
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        // Tenant Context
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // Auth
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Clerk:Authority"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Clerk:Authority"],
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    NameClaimType = "sub",
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<TenantContext>();
                        var claims = context.Principal!.Claims;
                        tenantContext.ClerkUserId = claims.First(c => c.Type == "sub").Value ?? "user_2lhQ5tvadQkBEO0zsjzv42bcgyB";
                        tenantContext.TenantId = claims.FirstOrDefault(c => c.Type == "org_id")?.Value ?? "org_sparklwash_dev";
                        tenantContext.Role = claims.FirstOrDefault(c => c.Type == "org_role")?.Value ?? "Admin";
                        return Task.CompletedTask;
                    }
                };
            });

        // Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));
        services.AddHangfireServer();

        // SignalR
        services.AddSignalR();

        return services;
    }
}
