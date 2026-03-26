using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PayrollAdjustmentConfiguration : IEntityTypeConfiguration<PayrollAdjustment>
{
    public void Configure(EntityTypeBuilder<PayrollAdjustment> builder)
    {
        builder.ToTable("PayrollAdjustments");

        // ── Primary key ─────────────────────────────────────────────────────────
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ───────────────────────────────────────────────────
        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.PayrollEntryId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        builder.Property(a => a.TemplateId)
            .HasMaxLength(36);

        // ── Audit timestamps ────────────────────────────────────────────────────
        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        // ── Relationships ───────────────────────────────────────────────────────
        builder.HasOne(a => a.Entry)
            .WithMany(e => e.Adjustments)
            .HasForeignKey(a => a.PayrollEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Template)
            .WithMany(t => t.Adjustments)
            .HasForeignKey(a => a.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ─────────────────────────────────────────────────────────────
        builder.HasIndex(a => a.PayrollEntryId);
        builder.HasIndex(a => a.TenantId);
    }
}
