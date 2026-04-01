using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Expenses.Commands.RecordExpense;

public sealed class RecordExpenseCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<RecordExpenseCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        RecordExpenseCommand request,
        CancellationToken cancellationToken)
    {
        var expense = new Expense(
            tenantContext.TenantId,
            request.BranchId,
            tenantContext.UserId,
            request.CategoryId,
            request.Amount,
            request.Description,
            request.ExpenseDate)
        {
            Vendor = request.Vendor,
            ReceiptReference = request.ReceiptReference,
            Frequency = request.Frequency,
            IsRecurring = request.IsRecurring,
        };

        db.Expenses.Add(expense);
        return Result.Success(expense.Id);
    }
}
