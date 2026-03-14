using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Commands.AdjustStock;

/// <summary>
/// Applies a signed adjustment to <see cref="Domain.Entities.Merchandise.StockQuantity"/>.
/// <para>
/// <b>Positive adjustment</b> (e.g. +50): stock received, restock, correction upward.
/// <b>Negative adjustment</b> (e.g. -3): write-off, damaged goods, manual correction.
/// </para>
/// The handler rejects any adjustment that would result in a negative quantity.
/// Normal POS sales do NOT go through this command — inventory is decremented inside
/// <c>CreateTransactionCommandHandler</c> as part of the transaction creation algorithm.
/// </summary>
public sealed record AdjustStockCommand(
    string MerchandiseId,
    int Adjustment,
    string? Reason) : ICommand;
