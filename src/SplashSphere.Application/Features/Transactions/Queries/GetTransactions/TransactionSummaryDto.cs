using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Queries.GetTransactions;

public sealed record TransactionSummaryDto(
    string Id,
    string TransactionNumber,
    string BranchId,
    string BranchName,
    string CarId,
    string PlateNumber,
    string VehicleTypeName,
    string SizeName,
    string? CustomerId,
    string? CustomerName,
    TransactionStatus Status,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal FinalAmount,
    string CashierName,
    string? QueueEntryId,
    DateTime CreatedAt);
