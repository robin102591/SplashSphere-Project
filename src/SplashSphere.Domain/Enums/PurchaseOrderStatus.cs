namespace SplashSphere.Domain.Enums;

/// <summary>
/// Tracks the lifecycle of a purchase order from creation to fulfilment.
/// </summary>
public enum PurchaseOrderStatus
{
    /// <summary>PO created but not yet sent to the supplier.</summary>
    Draft = 1,

    /// <summary>PO has been sent/communicated to the supplier.</summary>
    Sent = 2,

    /// <summary>Some line items have been received; others are still outstanding.</summary>
    PartiallyReceived = 3,

    /// <summary>All line items have been received in full.</summary>
    Received = 4,

    /// <summary>PO was cancelled before full receipt.</summary>
    Cancelled = 5,
}
