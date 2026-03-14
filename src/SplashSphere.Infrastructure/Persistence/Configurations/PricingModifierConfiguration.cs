using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PricingModifierConfiguration : IEntityTypeConfiguration<PricingModifier>
{
    public void Configure(EntityTypeBuilder<PricingModifier> builder)
    {
        builder.ToTable("PricingModifiers");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(pm => pm.Id);
        builder.Property(pm => pm.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(pm => pm.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pm => pm.BranchId)
            .HasMaxLength(36);

        builder.Property(pm => pm.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pm => pm.Type)
            .IsRequired()
            .HasConversion<int>();

        // Value is a multiplier (e.g. 1.2000) or absolute PHP amount.
        // Extra decimal places handle precise multipliers like 1.1250.
        builder.Property(pm => pm.Value)
            .IsRequired()
            .HasPrecision(10, 4);

        // ── Activation condition fields ───────────────────────────────────────

        // TimeOnly → PostgreSQL "time without time zone"
        builder.Property(pm => pm.StartTime)
            .HasColumnType("time");

        builder.Property(pm => pm.EndTime)
            .HasColumnType("time");

        // DayOfWeek (System enum) → stored as integer (0 = Sunday … 6 = Saturday)
        builder.Property(pm => pm.ActiveDayOfWeek)
            .HasConversion<int?>();

        // DateOnly → PostgreSQL "date"
        builder.Property(pm => pm.HolidayDate)
            .HasColumnType("date");

        builder.Property(pm => pm.HolidayName)
            .HasMaxLength(128);

        builder.Property(pm => pm.StartDate)
            .HasColumnType("date");

        builder.Property(pm => pm.EndDate)
            .HasColumnType("date");

        builder.Property(pm => pm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(pm => pm.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(pm => pm.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(pm => pm.Tenant)
            .WithMany()
            .HasForeignKey(pm => pm.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // SetNull: deleting a branch clears BranchId, making the modifier tenant-wide
        builder.HasOne(pm => pm.Branch)
            .WithMany()
            .HasForeignKey(pm => pm.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(pm => pm.TenantId);
        builder.HasIndex(pm => new { pm.TenantId, pm.Type });
        builder.HasIndex(pm => pm.BranchId);
    }
}
