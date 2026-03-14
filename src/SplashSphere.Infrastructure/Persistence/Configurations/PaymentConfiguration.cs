using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

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

        builder.Property(p => p.TransactionId)
            .IsRequired()
            .HasMaxLength(26); // ULID

        // Stored as int — Cash=1, GCash=2, CreditCard=3, DebitCard=4, BankTransfer=5
        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        // GCash ref, card approval code, bank transfer ref — null for cash
        builder.Property(p => p.ReferenceNumber)
            .HasMaxLength(256);

        // UTC timestamp set in the entity constructor
        builder.Property(p => p.PaidAt)
            .IsRequired();

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: payments are deleted when the parent transaction is deleted
        builder.HasOne(p => p.Transaction)
            .WithMany(t => t.Payments)
            .HasForeignKey(p => p.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(p => p.TenantId);

        // List all payments for a transaction (multi-payment reconciliation)
        builder.HasIndex(p => p.TransactionId);
    }
}
