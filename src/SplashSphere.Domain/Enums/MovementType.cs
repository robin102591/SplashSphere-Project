namespace SplashSphere.Domain.Enums;

/// <summary>
/// Classifies inventory stock movements by direction and purpose.
/// "In" types increase stock; "Out" types decrease stock.
/// </summary>
public enum MovementType
{
    /// <summary>Stock received from a purchase order.</summary>
    PurchaseIn = 1,

    /// <summary>Stock consumed during service delivery (supply items).</summary>
    UsageOut = 2,

    /// <summary>Stock sold to a customer (merchandise).</summary>
    SaleOut = 3,

    /// <summary>Manual upward stock correction.</summary>
    AdjustmentIn = 4,

    /// <summary>Manual downward stock correction.</summary>
    AdjustmentOut = 5,

    /// <summary>Stock received from another branch.</summary>
    TransferIn = 6,

    /// <summary>Stock sent to another branch.</summary>
    TransferOut = 7,

    /// <summary>Stock returned by customer or to supplier.</summary>
    ReturnIn = 8,

    /// <summary>Stock written off due to damage, expiry, or spillage.</summary>
    WasteOut = 9,
}
