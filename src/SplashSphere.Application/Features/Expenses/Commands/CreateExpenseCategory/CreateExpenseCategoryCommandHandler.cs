using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Expenses.Commands.CreateExpenseCategory;

public sealed class CreateExpenseCategoryCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CreateExpenseCategoryCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateExpenseCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = new ExpenseCategory(tenantContext.TenantId, request.Name, request.Icon);
        db.ExpenseCategories.Add(category);
        return Result.Success(category.Id);
    }
}
