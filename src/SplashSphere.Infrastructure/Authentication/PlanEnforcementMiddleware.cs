using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.Infrastructure.Authentication;

/// <summary>
/// Middleware that enforces plan-based feature gates.
/// Runs after <see cref="TenantResolutionMiddleware"/> so <see cref="TenantContext"/>
/// is populated. Checks the <see cref="RequiresFeatureAttribute"/> on the matched endpoint.
/// Returns 403 ProblemDetails if the tenant's plan does not include the required feature.
/// </summary>
public sealed class PlanEnforcementMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IPlanEnforcementService planService,
        TenantContext tenantContext)
    {
        var endpoint = context.GetEndpoint();
        var featureAttr = endpoint?.Metadata.GetMetadata<RequiresFeatureAttribute>();

        if (featureAttr is not null && !string.IsNullOrEmpty(tenantContext.TenantId))
        {
            var allowed = await planService.HasFeatureAsync(
                tenantContext.TenantId, featureAttr.FeatureKey, context.RequestAborted);

            if (!allowed)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Feature not available",
                    Detail = $"The '{featureAttr.FeatureKey}' feature is not included in your current plan. Please upgrade to access this feature.",
                    Status = 403,
                    Extensions = { ["featureKey"] = featureAttr.FeatureKey }
                }, context.RequestAborted);
                return;
            }
        }

        await next(context);
    }
}
