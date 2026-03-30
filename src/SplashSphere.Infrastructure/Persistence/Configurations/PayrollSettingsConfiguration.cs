using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PayrollSettingsConfiguration : IEntityTypeConfiguration<PayrollSettings>
{
    public void Configure(EntityTypeBuilder<PayrollSettings> builder)
    {
        builder.ToTable("PayrollSettings");

        builder.HasKey(ps => ps.Id);
        builder.Property(ps => ps.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(ps => ps.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        // PayrollFrequency stored as int (1=Weekly, 2=SemiMonthly)
        builder.Property(ps => ps.Frequency)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(PayrollFrequency.Weekly);

        // DayOfWeek stored as int (0=Sunday, 1=Monday, ..., 6=Saturday)
        builder.Property(ps => ps.CutOffStartDay)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(DayOfWeek.Monday);

        builder.Property(ps => ps.PayReleaseDayOffset)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(ps => ps.AutoCalcGovernmentDeductions)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ps => ps.BranchId)
            .HasMaxLength(36);

        builder.Property(ps => ps.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(ps => ps.UpdatedAt)
            .IsRequired();

        // Unique: one row per (tenant, branch). PostgreSQL treats NULLs as distinct
        // by default, so we use NULLS NOT DISTINCT to allow only one default row.
        builder.HasIndex(ps => new { ps.TenantId, ps.BranchId })
            .IsUnique()
            .HasDatabaseName("IX_PayrollSettings_TenantId_BranchId")
            .AreNullsDistinct(false);

        // FK to Tenant
        builder.HasOne(ps => ps.Tenant)
            .WithMany()
            .HasForeignKey(ps => ps.TenantId)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to Branch (optional)
        builder.HasOne(ps => ps.Branch)
            .WithMany()
            .HasForeignKey(ps => ps.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
