using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PayrollEntryConfiguration : IEntityTypeConfiguration<PayrollEntry>
{
    public void Configure(EntityTypeBuilder<PayrollEntry> builder)
    {
        builder.ToTable("PayrollEntries");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(pe => pe.Id);
        builder.Property(pe => pe.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(pe => pe.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pe => pe.PayrollPeriodId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(pe => pe.EmployeeId)
            .IsRequired()
            .HasMaxLength(36);

        // Snapshot of EmployeeType at close time — stored as int
        builder.Property(pe => pe.EmployeeTypeSnapshot)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pe => pe.DaysWorked)
            .IsRequired();

        // Null for Commission-type employees
        builder.Property(pe => pe.DailyRateSnapshot)
            .HasPrecision(10, 2);

        // DailyRateSnapshot × DaysWorked for Daily; 0 for Commission
        builder.Property(pe => pe.BaseSalary)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(pe => pe.TotalCommissions)
            .IsRequired()
            .HasPrecision(10, 2);

        // Admin-adjustable while period is Closed; default 0
        builder.Property(pe => pe.Bonuses)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        // Admin-adjustable while period is Closed; default 0
        builder.Property(pe => pe.Deductions)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(pe => pe.Notes);

        // ── Computed / ignored ────────────────────────────────────────────────
        // NetPay = BaseSalary + TotalCommissions + Bonuses - Deductions
        // Pure in-memory computation — not stored as a column
        builder.Ignore(pe => pe.NetPay);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(pe => pe.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(pe => pe.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: entries are owned by the period
        builder.HasOne(pe => pe.PayrollPeriod)
            .WithMany(pp => pp.Entries)
            .HasForeignKey(pe => pe.PayrollPeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete an employee who has payroll history
        builder.HasOne(pe => pe.Employee)
            .WithMany(e => e.PayrollEntries)
            .HasForeignKey(pe => pe.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pe => pe.Tenant)
            .WithMany()
            .HasForeignKey(pe => pe.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Unique constraint: one entry per employee per period ──────────────
        builder.HasIndex(pe => new { pe.PayrollPeriodId, pe.EmployeeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(pe => pe.TenantId);
        builder.HasIndex(pe => pe.EmployeeId);

        // Commission history page: all entries for one employee across periods
        builder.HasIndex(pe => new { pe.EmployeeId, pe.PayrollPeriodId });
    }
}
