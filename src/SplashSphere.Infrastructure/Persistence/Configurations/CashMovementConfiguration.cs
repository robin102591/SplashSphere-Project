using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.ToTable("CashMovements");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(m => m.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(m => m.CashierShiftId)
            .IsRequired()
            .HasMaxLength(36);

        // Stored as int — CashIn=1, CashOut=2
        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(m => m.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Reference)
            .HasMaxLength(256);

        builder.Property(m => m.MovementTime)
            .IsRequired();

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        // ── Relationship — cascade delete handled in CashierShiftConfiguration ─
        // (Registered on both sides for EF Core correctness)
        builder.HasOne(m => m.CashierShift)
            .WithMany(s => s.CashMovements)
            .HasForeignKey(m => m.CashierShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        // List all movements for a shift (ordered by MovementTime in query)
        builder.HasIndex(m => m.CashierShiftId);
    }
}
