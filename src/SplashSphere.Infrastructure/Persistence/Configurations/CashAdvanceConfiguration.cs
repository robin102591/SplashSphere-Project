using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class CashAdvanceConfiguration : IEntityTypeConfiguration<CashAdvance>
{
    public void Configure(EntityTypeBuilder<CashAdvance> builder)
    {
        builder.ToTable("CashAdvances");

        // ── Primary key ─────────────────────────────────────────────────────────
        builder.HasKey(ca => ca.Id);
        builder.Property(ca => ca.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ───────────────────────────────────────────────────
        builder.Property(ca => ca.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ca => ca.EmployeeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(ca => ca.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(ca => ca.RemainingBalance)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(ca => ca.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ca => ca.Reason)
            .HasMaxLength(500);

        builder.Property(ca => ca.ApprovedById)
            .HasMaxLength(36);

        builder.Property(ca => ca.DeductionPerPeriod)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Audit timestamps ────────────────────────────────────────────────────
        builder.Property(ca => ca.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(ca => ca.UpdatedAt)
            .IsRequired();

        // ── Relationships ───────────────────────────────────────────────────────
        builder.HasOne(ca => ca.Employee)
            .WithMany(e => e.CashAdvances)
            .HasForeignKey(ca => ca.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.ApprovedBy)
            .WithMany()
            .HasForeignKey(ca => ca.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ca => ca.Tenant)
            .WithMany()
            .HasForeignKey(ca => ca.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ─────────────────────────────────────────────────────────────
        builder.HasIndex(ca => new { ca.TenantId, ca.EmployeeId });
        builder.HasIndex(ca => new { ca.TenantId, ca.Status });
    }
}
