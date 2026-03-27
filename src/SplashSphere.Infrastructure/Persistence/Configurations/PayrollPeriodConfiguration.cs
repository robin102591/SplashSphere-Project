using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.ToTable("PayrollPeriods");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(pp => pp.Id);
        builder.Property(pp => pp.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(pp => pp.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pp => pp.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pp => pp.Year)
            .IsRequired();

        builder.Property(pp => pp.CutOffWeek)
            .IsRequired();

        // DateOnly → PostgreSQL "date"
        // Stores the first day of the payroll period (Asia/Manila calendar)
        builder.Property(pp => pp.StartDate)
            .IsRequired()
            .HasColumnType("date");

        // Stores the last day of the payroll period (Asia/Manila calendar)
        builder.Property(pp => pp.EndDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(pp => pp.ScheduledReleaseDate)
            .HasColumnType("date");

        builder.Property(pp => pp.ReleasedAt);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(pp => pp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(pp => pp.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(pp => pp.Tenant)
            .WithMany()
            .HasForeignKey(pp => pp.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Unique constraint: one period per start date per tenant ────────────
        // Prevents the daily payroll Hangfire job from double-creating
        builder.HasIndex(pp => new { pp.TenantId, pp.StartDate })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(pp => pp.TenantId);

        // Status queries: list Open periods, find Closed periods to process
        builder.HasIndex(pp => new { pp.TenantId, pp.Status });
    }
}
