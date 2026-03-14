using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SplashSphere.Application.Common.Behaviors;

namespace SplashSphere.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

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
