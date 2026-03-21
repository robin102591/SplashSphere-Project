using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ShiftPaymentSummaryConfiguration : IEntityTypeConfiguration<ShiftPaymentSummary>
{
    public void Configure(EntityTypeBuilder<ShiftPaymentSummary> builder)
    {
        builder.ToTable("ShiftPaymentSummaries");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(p => p.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.CashierShiftId)
            .IsRequired()
            .HasMaxLength(36);

        // Stored as int — Cash=1, GCash=2, CreditCard=3, DebitCard=4, BankTransfer=5
        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.TransactionCount)
            .IsRequired();

        builder.Property(p => p.TotalAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Relationship — cascade from shift ─────────────────────────────────
        builder.HasOne(p => p.CashierShift)
            .WithMany(s => s.PaymentSummaries)
            .HasForeignKey(p => p.CashierShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Unique constraint — one row per payment method per shift ──────────
        builder.HasIndex(p => new { p.CashierShiftId, p.Method }).IsUnique();
    }
}
