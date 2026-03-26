using MediatR;
using SplashSphere.Application.Features.Search.Queries.GlobalSearch;

namespace SplashSphere.API.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/search")
            .WithTags("Search")
            .RequireAuthorization();

        group.MapGet("/", GlobalSearch).WithName("GlobalSearch");

        return app;
    }

    private static async Task<IResult> GlobalSearch(
        [AsParameters] GlobalSearchQuery query,
        ISender sender,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Q) || query.Q.Trim().Length < 2)
            return TypedResults.Ok(new
            {
                customers = Array.Empty<object>(),
                employees = Array.Empty<object>(),
                transactions = Array.Empty<object>(),
                vehicles = Array.Empty<object>(),
                services = Array.Empty<object>(),
                merchandise = Array.Empty<object>(),
            });

        return TypedResults.Ok(await sender.Send(query, ct));
    }
}
