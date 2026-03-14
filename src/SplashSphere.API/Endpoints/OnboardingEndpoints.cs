using MediatR;
using SplashSphere.Application.Features.Onboarding.Commands.CreateOnboarding;

namespace SplashSphere.API.Endpoints;

public static class OnboardingEndpoints
{
    public static IEndpointRouteBuilder MapOnboardingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/onboarding")
            .RequireAuthorization()
            .WithTags("Onboarding");

        // POST /api/v1/onboarding
        // Creates the Clerk Organization, Tenant, first Branch, and links the User.
        // Requires JWT auth but NOT an existing tenant (allowed by TenantResolutionMiddleware).
        group.MapPost("/", async (
            CreateOnboardingRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CreateOnboardingCommand(
                request.BusinessName,
                request.BusinessEmail,
                request.ContactNumber,
                request.Address,
                request.BranchName,
                request.BranchCode,
                request.BranchAddress,
                request.BranchContactNumber);

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { tenantId = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateOnboarding")
        .WithSummary("Complete tenant onboarding: creates Clerk org, tenant, and first branch.");

        return app;
    }
}

public sealed record CreateOnboardingRequest(
    string BusinessName,
    string BusinessEmail,
    string ContactNumber,
    string Address,
    string BranchName,
    string BranchCode,
    string BranchAddress,
    string BranchContactNumber);
