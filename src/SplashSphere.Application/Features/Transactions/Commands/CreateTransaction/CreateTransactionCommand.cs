using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.CreateTransaction;

/// <summary>
/// Creates a new POS transaction. Returns the new transaction's ULID string ID on success.
/// </summary>
/// <param name="BranchId">Branch where the service takes place.</param>
/// <param name="CarId">
///     The car being serviced. When null, the handler looks up a car by <paramref name="PlateNumber"/>
///     and creates one if none exists — requires <paramref name="VehicleTypeId"/> and
///     <paramref name="SizeId"/> to be supplied.
/// </param>
/// <param name="PlateNumber">Plate number used for car lookup / auto-creation when <paramref name="CarId"/> is null.</param>
/// <param name="VehicleTypeId">Required for auto-creation when <paramref name="CarId"/> is null.</param>
/// <param name="SizeId">Required for auto-creation when <paramref name="CarId"/> is null.</param>
/// <param name="CustomerId">Optional registered customer (walk-ins may omit).</param>
/// <param name="Services">Individual service line items with their assigned employees.</param>
/// <param name="Packages">Package line items with their assigned employees.</param>
/// <param name="Merchandise">Merchandise items sold alongside the wash.</param>
/// <param name="DiscountAmount">Total discount applied. Must be ≥ 0.</param>
/// <param name="TaxAmount">VAT / tax component. Must be ≥ 0.</param>
/// <param name="Notes">Optional cashier notes.</param>
/// <param name="QueueEntryId">
///     When set, the handler links this transaction to the queue entry and transitions
///     the entry from CALLED → IN_SERVICE.
/// </param>
public sealed record CreateTransactionCommand(
    string BranchId,
    string? CarId,
    string? PlateNumber,
    string? VehicleTypeId,
    string? SizeId,
    string? CustomerId,
    IReadOnlyList<TransactionServiceRequest> Services,
    IReadOnlyList<TransactionPackageRequest> Packages,
    IReadOnlyList<TransactionMerchandiseRequest> Merchandise,
    decimal DiscountAmount,
    decimal TaxAmount,
    string? Notes,
    string? QueueEntryId) : ICommand<string>;

/// <summary>One service requested in the transaction, with the employees who performed it.</summary>
public sealed record TransactionServiceRequest(
    string ServiceId,
    IReadOnlyList<string> EmployeeIds);

/// <summary>One package requested in the transaction, with the employees who performed it.</summary>
public sealed record TransactionPackageRequest(
    string PackageId,
    IReadOnlyList<string> EmployeeIds);

/// <summary>Merchandise item sold in the same transaction.</summary>
public sealed record TransactionMerchandiseRequest(
    string MerchandiseId,
    int Quantity);
