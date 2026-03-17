using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SplashSphere.Application.Common.Behaviors;

namespace SplashSphere.Application;

public static class DependencyInjection
{
    /// <param name="infrastructureAssemblyMarker">
    /// A type from the Infrastructure assembly so MediatR can scan it for
    /// notification handlers (SignalR hub handlers live there).
    /// Pass <c>typeof(SplashSphere.Infrastructure.DependencyInjection)</c>.
    /// </param>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        Type? infrastructureAssemblyMarker = null)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            // Also scan Infrastructure so SignalR notification handlers are registered
            if (infrastructureAssemblyMarker is not null)
                cfg.RegisterServicesFromAssembly(infrastructureAssemblyMarker.Assembly);

            // Pipeline order (outermost → innermost):
            //   LoggingBehavior → ValidationBehavior → UnitOfWorkBehavior → Handler
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        return services;
    }
}
