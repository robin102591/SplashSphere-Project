using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Expenses;
using SplashSphere.Application.Features.Expenses.Commands.CreateExpenseCategory;
using SplashSphere.Application.Features.Expenses.Commands.DeleteExpense;
using SplashSphere.Application.Features.Expenses.Commands.RecordExpense;
using SplashSphere.Application.Features.Expenses.Commands.UpdateExpense;
using SplashSphere.Application.Features.Expenses.Queries.GetExpenseCategories;
using SplashSphere.Application.Features.Expenses.Queries.GetExpenses;
using SplashSphere.Application.Features.Expenses.Queries.GetProfitLossReport;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/expenses")
            .RequireAuthorization()
            .WithTags("Expenses")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.ExpenseTracking));

        group.MapPost("/", RecordExpense).WithSummary("Record an expense");
        group.MapGet("/", GetExpenses).WithSummary("List expenses");
        group.MapPut("/{id}", UpdateExpense).WithSummary("Update an expense");
        group.MapDelete("/{id}", DeleteExpense).WithSummary("Delete an expense");

        // Categories
        var catGroup = app.MapGroup("/api/v1/expense-categories")
            .RequireAuthorization()
            .WithTags("Expenses")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.ExpenseTracking));

        catGroup.MapGet("/", GetCategories).WithSummary("List expense categories");
        catGroup.MapPost("/", CreateCategory).WithSummary("Create an expense category");

        // P&L Report
        app.MapGet("/api/v1/reports/profit-loss", GetProfitLossReport)
            .RequireAuthorization()
            .WithTags("Reports")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.ProfitLossReports))
            .WithSummary("Get profit and loss report");

        return app;
    }

    private static async Task<IResult> RecordExpense(
        [FromBody] RecordExpenseRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordExpenseCommand(
            body.BranchId, body.CategoryId, body.Amount, body.Description,
            body.ExpenseDate, body.Vendor, body.ReceiptReference,
            body.Frequency, body.IsRecurring), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/expenses/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> GetExpenses(
        [AsParameters] ExpenseListParams p, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new GetExpensesQuery(p.BranchId, p.CategoryId, p.From, p.To, p.Page, p.PageSize), ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> UpdateExpense(
        string id, [FromBody] UpdateExpenseRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateExpenseCommand(
            id, body.CategoryId, body.Amount, body.Description, body.ExpenseDate,
            body.Vendor, body.ReceiptReference, body.Frequency, body.IsRecurring), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteExpense(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteExpenseCommand(id), ct);
        return result.IsFailure ? TypedResults.NotFound() : TypedResults.NoContent();
    }

    private static async Task<IResult> GetCategories(ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetExpenseCategoriesQuery(), ct));

    private static async Task<IResult> CreateCategory(
        [FromBody] CreateCategoryRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateExpenseCategoryCommand(body.Name, body.Icon), ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/expense-categories/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> GetProfitLossReport(
        DateOnly from, DateOnly to, string? branchId, ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetProfitLossReportQuery(from, to, branchId), ct));

    // Request records
    private sealed record RecordExpenseRequest(
        string BranchId, string CategoryId, decimal Amount, string Description,
        DateTime ExpenseDate, string? Vendor = null, string? ReceiptReference = null,
        ExpenseFrequency Frequency = ExpenseFrequency.OneTime, bool IsRecurring = false);

    private sealed record UpdateExpenseRequest(
        string CategoryId, decimal Amount, string Description, DateTime ExpenseDate,
        string? Vendor, string? ReceiptReference,
        ExpenseFrequency Frequency, bool IsRecurring);

    private sealed record CreateCategoryRequest(string Name, string? Icon = null);

    private sealed record ExpenseListParams(
        string? BranchId = null, string? CategoryId = null,
        DateOnly? From = null, DateOnly? To = null,
        int Page = 1, int PageSize = 50);
}
