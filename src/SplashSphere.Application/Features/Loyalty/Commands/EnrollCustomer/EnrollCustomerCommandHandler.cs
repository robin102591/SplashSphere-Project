using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Commands.EnrollCustomer;

public sealed class EnrollCustomerCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<EnrollCustomerCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        EnrollCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customerExists = await context.Customers
            .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (!customerExists)
            return Result.Failure<string>(Error.NotFound("Customer", request.CustomerId));

        var alreadyEnrolled = await context.MembershipCards
            .AnyAsync(m => m.CustomerId == request.CustomerId, cancellationToken);

        if (alreadyEnrolled)
            return Result.Failure<string>(Error.Validation("Customer is already enrolled in the loyalty program."));

        // Generate a sequential card number: SS-{5-digit sequence}
        var maxCard = await context.MembershipCards
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => m.CardNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextSeq = 1;
        if (maxCard is not null && maxCard.StartsWith("SS-") && int.TryParse(maxCard[3..], out var current))
            nextSeq = current + 1;

        var cardNumber = $"SS-{nextSeq:D5}";

        var card = new MembershipCard(tenantContext.TenantId, request.CustomerId, cardNumber);
        context.MembershipCards.Add(card);

        return Result.Success(card.Id);
    }
}
