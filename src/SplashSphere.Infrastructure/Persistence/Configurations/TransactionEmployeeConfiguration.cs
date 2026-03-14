using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class TransactionEmployeeConfiguration : IEntityTypeConfiguration<TransactionEmployee>
{
    public void Configure(EntityTypeBuilder<TransactionEmployee> builder)
    {
        builder.ToTable("TransactionEmployees");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(te => te.Id);
        builder.Property(te => te.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(te => te.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(te => te.TransactionId)
            .IsRequired()
            .HasMaxLength(26); // ULID

        builder.Property(te => te.EmployeeId)
            .IsRequired()
            .HasMaxLength(36);

        // Aggregated commission sum across all service + package assignments
        builder.Property(te => te.TotalCommission)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(te => te.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(te => te.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: summary records deleted when parent transaction is deleted
        builder.HasOne(te => te.Transaction)
            .WithMany(t => t.Employees)
            .HasForeignKey(te => te.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete an employee with transaction commission history
        builder.HasOne(te => te.Employee)
            .WithMany(e => e.TransactionSummaries)
            .HasForeignKey(te => te.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique constraint: one summary row per employee per transaction ────
        builder.HasIndex(te => new { te.TransactionId, te.EmployeeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(te => te.TenantId);

        // Payroll close: sum commissions for one employee across a date range
        builder.HasIndex(te => new { te.EmployeeId, te.TransactionId });
    }
}
