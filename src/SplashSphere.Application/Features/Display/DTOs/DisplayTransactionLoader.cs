using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Display.DTOs;

/// <summary>
/// Materialises a <see cref="DisplayTransactionResultDto"/> (or its completion
/// wrapper) from the transaction aggregate. Shared by the SignalR broadcaster
/// (Infrastructure) and the reconnect-sync query handler (Application) so
/// both paths produce byte-identical payloads.
/// <para>
/// The loader returns the routing key (<c>display:{branchId}:{stationId}</c>)
/// alongside the DTO so the broadcaster doesn't need to re-query for it.
/// Returns <c>(null, null)</c> when the transaction has no station, has been
/// deleted, or doesn't belong to the current tenant — callers treat that as
/// a no-op.
/// </para>
/// </summary>
public static class DisplayTransactionLoader
{
    public static async Task<(string? Group, DisplayTransactionResultDto? Payload)> LoadTransactionAsync(
        IApplicationDbContext db,
        string transactionId,
        CancellationToken cancellationToken)
    {
        var route = await db.Transactions
            .AsNoTracking()
            .Where(t => t.Id == transactionId && t.PosStationId != null)
            .Select(t => new { t.BranchId, t.PosStationId })
            .FirstOrDefaultAsync(cancellationToken);

        if (route is null || route.PosStationId is null) return (null, null);

        var dto = await BuildTransactionAsync(db, transactionId, cancellationToken);
        if (dto is null) return (null, null);

        return ($"display:{route.BranchId}:{route.PosStationId}", dto);
    }

    public static async Task<(string? Group, DisplayCompletionResultDto? Payload)> LoadCompletionAsync(
        IApplicationDbContext db,
        string transactionId,
        CancellationToken cancellationToken)
    {
        var (group, txDto) = await LoadTransactionAsync(db, transactionId, cancellationToken);
        if (group is null || txDto is null) return (null, null);

        var detail = await db.Transactions
            .AsNoTracking()
            .Where(t => t.Id == transactionId)
            .Select(t => new
            {
                t.BranchId,
                t.PointsEarned,
                CustomerPoints = t.Customer != null && t.Customer.MembershipCard != null
                    ? (int?)t.Customer.MembershipCard.PointsBalance
                    : null,
                Payments = t.Payments
                    .Select(p => new { p.PaymentMethod, p.Amount })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail is null) return (null, null);

        var receiptSetting = await db.ReceiptSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.BranchId == detail.BranchId, cancellationToken);

        receiptSetting ??= await db.ReceiptSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.BranchId == null, cancellationToken);

        var totalPaid = detail.Payments.Sum(p => p.Amount);
        var primaryMethod = detail.Payments.Count > 0
            ? FormatMethod(detail.Payments[0].PaymentMethod)
            : "Cash";
        var change = Math.Max(0m, totalPaid - txDto.Total);

        var payload = new DisplayCompletionResultDto(
            txDto,
            primaryMethod,
            totalPaid,
            change,
            detail.PointsEarned > 0 ? detail.PointsEarned : null,
            detail.CustomerPoints,
            receiptSetting?.ThankYouMessage ?? "Thank you for your patronage!",
            receiptSetting?.PromoText);

        return (group, payload);
    }

    /// <summary>
    /// Used by the reconnect-sync endpoint: returns the active (Pending or
    /// InProgress) transaction for a station, or null if the station has no
    /// open transaction (display stays on Idle).
    /// </summary>
    public static async Task<DisplayTransactionResultDto?> LoadByStationAsync(
        IApplicationDbContext db,
        string branchId,
        string stationId,
        CancellationToken cancellationToken)
    {
        var transactionId = await db.Transactions
            .AsNoTracking()
            .Where(t => t.BranchId == branchId
                     && t.PosStationId == stationId
                     && (t.Status == TransactionStatus.Pending
                      || t.Status == TransactionStatus.InProgress))
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (transactionId is null) return null;

        return await BuildTransactionAsync(db, transactionId, cancellationToken);
    }

    private static async Task<DisplayTransactionResultDto?> BuildTransactionAsync(
        IApplicationDbContext db,
        string transactionId,
        CancellationToken cancellationToken)
    {
        var header = await db.Transactions
            .AsNoTracking()
            .Where(t => t.Id == transactionId)
            .Select(t => new
            {
                t.TotalAmount,
                t.DiscountAmount,
                t.TaxAmount,
                t.FinalAmount,
                Customer = t.Customer != null
                    ? new
                    {
                        Name = t.Customer.FirstName + " " + t.Customer.LastName,
                        LoyaltyTier = t.Customer.MembershipCard != null
                            ? t.Customer.MembershipCard.CurrentTier.ToString()
                            : null,
                    }
                    : null,
                Vehicle = new
                {
                    Plate = t.Car.PlateNumber,
                    MakeModel = (t.Car.Make != null ? t.Car.Make.Name + " " : "")
                        + (t.Car.Model != null ? t.Car.Model.Name : ""),
                    TypeSize = t.Car.VehicleType.Name + " / " + t.Car.Size.Name,
                },
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null) return null;

        var serviceLines = await db.TransactionServices
            .AsNoTracking()
            .Where(ts => ts.TransactionId == transactionId)
            .Select(ts => new DisplayLineItemResultDto(
                ts.Id,
                ts.Service.Name,
                "service",
                1,
                ts.UnitPrice,
                ts.UnitPrice))
            .ToListAsync(cancellationToken);

        var packageLines = await db.TransactionPackages
            .AsNoTracking()
            .Where(tp => tp.TransactionId == transactionId)
            .Select(tp => new DisplayLineItemResultDto(
                tp.Id,
                tp.Package.Name,
                "package",
                1,
                tp.UnitPrice,
                tp.UnitPrice))
            .ToListAsync(cancellationToken);

        var merchLines = await db.TransactionMerchandise
            .AsNoTracking()
            .Where(tm => tm.TransactionId == transactionId)
            .Select(tm => new DisplayLineItemResultDto(
                tm.Id,
                tm.Merchandise.Name,
                "merchandise",
                tm.Quantity,
                tm.UnitPrice,
                tm.UnitPrice * tm.Quantity))
            .ToListAsync(cancellationToken);

        var items = serviceLines.Concat(packageLines).Concat(merchLines).ToList();
        var vehicleMakeModelTrimmed = header.Vehicle.MakeModel.Trim();

        return new DisplayTransactionResultDto(
            transactionId,
            header.Vehicle.Plate,
            string.IsNullOrWhiteSpace(vehicleMakeModelTrimmed) ? null : vehicleMakeModelTrimmed,
            header.Vehicle.TypeSize,
            header.Customer?.Name,
            header.Customer?.LoyaltyTier,
            items,
            header.TotalAmount,
            header.DiscountAmount,
            header.DiscountAmount > 0 ? "Discount" : null,
            header.TaxAmount,
            header.FinalAmount);
    }

    private static string FormatMethod(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash         => "Cash",
        PaymentMethod.GCash        => "GCash",
        PaymentMethod.CreditCard   => "Credit Card",
        PaymentMethod.DebitCard    => "Debit Card",
        PaymentMethod.BankTransfer => "Bank Transfer",
        _                          => method.ToString(),
    };
}
