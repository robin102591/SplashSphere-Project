namespace SplashSphere.Domain.Enums;

/// <summary>
/// Accepted payment methods at the POS. A single transaction may have
/// multiple payment records that together sum to the final amount.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Physical Philippine Peso bills and coins.</summary>
    Cash = 1,

    /// <summary>
    /// GCash mobile wallet (Mynt / Globe Fintech).
    /// Also used to represent Maya (PayMaya) QR/wallet payments.
    /// </summary>
    GCash = 2,

    /// <summary>Visa, Mastercard, or local credit card via POS terminal.</summary>
    CreditCard = 3,

    /// <summary>Visa Debit, Mastercard Debit, or local debit card via POS terminal.</summary>
    DebitCard = 4,

    /// <summary>Direct bank transfer (InstaPay / PESONet).</summary>
    BankTransfer = 5,
}
