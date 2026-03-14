using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Transactions.Commands.CreateTransaction;

namespace SplashSphere.API.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/transactions")
            .RequireAuthorization()
            .WithTags("Transactions");

        // POST /api/v1/transactions
        group.MapPost("/", CreateTransaction)
            .WithName("CreateTransaction")
            .WithSummary("Create a new POS transaction.");

        return app;
    }

    private static async Task<Results<Created<CreateTransactionResponse>, ValidationProblem, NotFound, BadRequest<ProblemDetails>>> CreateTransaction(
        [FromBody] CreateTransactionRequest body,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateTransactionCommand(
            body.BranchId,
            body.CarId,
            body.CustomerId,
            body.Services,
            body.Packages,
            body.Merchandise,
            body.DiscountAmount,
            body.TaxAmount,
            body.Notes,
            body.QueueEntryId);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code == "NotFound"
                ? TypedResults.NotFound()
                : TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Created($"/api/v1/transactions/{result.Value}", new CreateTransactionResponse(result.Value!));
    }

    private sealed record CreateTransactionRequest(
        string BranchId,
        string CarId,
        string? CustomerId,
        IReadOnlyList<TransactionServiceRequest> Services,
        IReadOnlyList<TransactionPackageRequest> Packages,
        IReadOnlyList<TransactionMerchandiseRequest> Merchandise,
        decimal DiscountAmount,
        decimal TaxAmount,
        string? Notes,
        string? QueueEntryId);

    private sealed record CreateTransactionResponse(string TransactionId);
}
