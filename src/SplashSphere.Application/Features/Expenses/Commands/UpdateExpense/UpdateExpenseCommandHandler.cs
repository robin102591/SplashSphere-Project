using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateExpenseCommand, Result>
{
    public async Task<Result> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await db.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (expense is null)
            return Result.Failure(Error.NotFound("Expense", request.Id));

        expense.CategoryId = request.CategoryId;
        expense.Amount = request.Amount;
        expense.Description = request.Description;
        expense.ExpenseDate = request.ExpenseDate;
        expense.Vendor = request.Vendor;
        expense.ReceiptReference = request.ReceiptReference;
        expense.Frequency = request.Frequency;
        expense.IsRecurring = request.IsRecurring;

        return Result.Success();
    }
}
