using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.ToTable("CashierShifts");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.BranchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(s => s.CashierId)
            .IsRequired()
            .HasMaxLength(36);

        // Manila-local business date stored as SQL date (no time component)
        builder.Property(s => s.ShiftDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(s => s.OpenedAt)
            .IsRequired();

        builder.Property(s => s.ClosedAt);

        // Stored as int — Open=1, Closed=2, Voided=3
        // No DB default — constructor always sets this explicitly.
        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        // ── Cash fund & computed totals (all decimal 10,2) ────────────────────
        builder.Property(s => s.OpeningCashFund)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(s => s.TotalCashPayments)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.TotalNonCashPayments)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.TotalCashIn)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.TotalCashOut)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.ExpectedCashInDrawer)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.ActualCashInDrawer)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.Variance)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        // ── Transaction summary ───────────────────────────────────────────────
        builder.Property(s => s.TotalTransactionCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.TotalRevenue)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.TotalCommissions)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(s => s.TotalDiscounts)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        // ── Review ────────────────────────────────────────────────────────────
        // Stored as int — Pending=1, Approved=2, Flagged=3
        // No DB default — constructor always sets this explicitly.
        builder.Property(s => s.ReviewStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.ReviewedById)
            .HasMaxLength(36);

        builder.Property(s => s.ReviewedAt);

        builder.Property(s => s.ReviewNotes)
            .HasMaxLength(1000);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Branch)
            .WithMany(b => b.CashierShifts)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // The cashier (User) can be deleted without losing shift history (SetNull)
        builder.HasOne(s => s.Cashier)
            .WithMany(u => u.CashierShifts)
            .HasForeignKey(s => s.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        // The reviewer (manager) — nullable FK, SetNull on user deletion
        builder.HasOne(s => s.ReviewedBy)
            .WithMany(u => u.ReviewedShifts)
            .HasForeignKey(s => s.ReviewedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Child collections — configured in their own files (cascade from shift)
        builder.HasMany(s => s.CashMovements)
            .WithOne(m => m.CashierShift)
            .HasForeignKey(m => m.CashierShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Denominations)
            .WithOne(d => d.CashierShift)
            .HasForeignKey(d => d.CashierShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.PaymentSummaries)
            .WithOne(p => p.CashierShift)
            .HasForeignKey(p => p.CashierShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(s => s.TenantId);

        // Queue board / shift list per branch + date
        builder.HasIndex(s => new { s.TenantId, s.BranchId, s.ShiftDate });

        // "Is there an open shift for this cashier at this branch?"
        builder.HasIndex(s => new { s.TenantId, s.CashierId, s.Status });

        // Variance review queue: all Pending/Flagged shifts per tenant
        builder.HasIndex(s => new { s.TenantId, s.ReviewStatus });
    }
}
