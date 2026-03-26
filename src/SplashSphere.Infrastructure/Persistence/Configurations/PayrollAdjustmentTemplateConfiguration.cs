using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PayrollAdjustmentTemplateConfiguration : IEntityTypeConfiguration<PayrollAdjustmentTemplate>
{
    public void Configure(EntityTypeBuilder<PayrollAdjustmentTemplate> builder)
    {
        builder.ToTable("PayrollAdjustmentTemplates");

        // ── Primary key ─────────────────────────────────────────────────────────
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ───────────────────────────────────────────────────
        builder.Property(t => t.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.DefaultAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // ── Audit timestamps ────────────────────────────────────────────────────
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        // ── Relationships ───────────────────────────────────────────────────────
        builder.HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ─────────────────────────────────────────────────────────────
        builder.HasIndex(t => t.TenantId);

        // No duplicate names within a tenant
        builder.HasIndex(t => new { t.TenantId, t.Name })
            .IsUnique();
    }
}
