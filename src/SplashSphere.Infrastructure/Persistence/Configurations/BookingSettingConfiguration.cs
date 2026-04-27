using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class BookingSettingConfiguration : IEntityTypeConfiguration<BookingSetting>
{
    public void Configure(EntityTypeBuilder<BookingSetting> builder)
    {
        builder.ToTable("BookingSettings");

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

        // TimeOnly maps to PostgreSQL `time` by default
        builder.Property(s => s.OpenTime)
            .IsRequired();

        builder.Property(s => s.CloseTime)
            .IsRequired();

        builder.Property(s => s.SlotIntervalMinutes)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(s => s.MaxBookingsPerSlot)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(s => s.AdvanceBookingDays)
            .IsRequired()
            .HasDefaultValue(7);

        builder.Property(s => s.MinLeadTimeMinutes)
            .IsRequired()
            .HasDefaultValue(120);

        builder.Property(s => s.NoShowGraceMinutes)
            .IsRequired()
            .HasDefaultValue(15);

        builder.Property(s => s.IsBookingEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.ShowInPublicDirectory)
            .IsRequired()
            .HasDefaultValue(true);

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
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        // One settings row per branch
        builder.HasIndex(s => new { s.TenantId, s.BranchId })
            .IsUnique()
            .HasDatabaseName("UX_BookingSetting_Tenant_Branch");
    }
}
