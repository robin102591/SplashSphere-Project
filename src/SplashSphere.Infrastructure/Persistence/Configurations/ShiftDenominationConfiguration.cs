using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ShiftDenominationConfiguration : IEntityTypeConfiguration<ShiftDenomination>
{
    public void Configure(EntityTypeBuilder<ShiftDenomination> builder)
    {
        builder.ToTable("ShiftDenominations");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(d => d.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.CashierShiftId)
            .IsRequired()
            .HasMaxLength(36);

        // 4 decimal digits to handle ₱0.25 (0.2500). Stored as numeric(6,4).
        builder.Property(d => d.DenominationValue)
            .IsRequired()
            .HasPrecision(6, 4);

        builder.Property(d => d.Count)
            .IsRequired();

        builder.Property(d => d.Subtotal)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Relationship — cascade from shift ─────────────────────────────────
        builder.HasOne(d => d.CashierShift)
            .WithMany(s => s.Denominations)
            .HasForeignKey(d => d.CashierShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Unique constraint — one row per denomination per shift ────────────
        builder.HasIndex(d => new { d.CashierShiftId, d.DenominationValue }).IsUnique();
    }
}
