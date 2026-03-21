using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ShiftSettingsConfiguration : IEntityTypeConfiguration<ShiftSettings>
{
    public void Configure(EntityTypeBuilder<ShiftSettings> builder)
    {
        builder.ToTable("ShiftSettings");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.DefaultOpeningFund)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(2000m);

        builder.Property(s => s.AutoApproveThreshold)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(50m);

        builder.Property(s => s.FlagThreshold)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(200m);

        builder.Property(s => s.RequireShiftForTransactions)
            .IsRequired()
            .HasDefaultValue(true);

        // TimeOnly stored as PostgreSQL time
        builder.Property(s => s.EndOfDayReminderTime)
            .IsRequired()
            .HasColumnType("time");

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // One record per tenant
        builder.HasIndex(s => s.TenantId).IsUnique();

        // FK to Tenant — no bi-directional navigation on Tenant side
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
