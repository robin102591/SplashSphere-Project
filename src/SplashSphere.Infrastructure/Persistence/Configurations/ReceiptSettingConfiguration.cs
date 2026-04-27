using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ReceiptSettingConfiguration : IEntityTypeConfiguration<ReceiptSetting>
{
    public void Configure(EntityTypeBuilder<ReceiptSetting> builder)
    {
        builder.ToTable("ReceiptSettings");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Tenant / Branch ───────────────────────────────────────────────────
        builder.Property(r => r.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.BranchId)
            .HasMaxLength(36);

        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // ── Header text ───────────────────────────────────────────────────────
        builder.Property(r => r.CustomHeaderText).HasMaxLength(256);

        // ── Footer text ───────────────────────────────────────────────────────
        builder.Property(r => r.ThankYouMessage)
            .IsRequired()
            .HasMaxLength(256)
            .HasDefaultValue("Thank you for your patronage!");

        builder.Property(r => r.PromoText).HasMaxLength(512);
        builder.Property(r => r.CustomFooterText).HasMaxLength(512);

        // ── Enums (stored as int) ─────────────────────────────────────────────
        builder.Property(r => r.LogoSize).HasConversion<int>().IsRequired();
        builder.Property(r => r.LogoPosition).HasConversion<int>().IsRequired();
        builder.Property(r => r.ReceiptWidth).HasConversion<int>().IsRequired();
        builder.Property(r => r.FontSize).HasConversion<int>().IsRequired();

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(r => r.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // One row per (tenant, branch); BranchId IS NULL is the tenant default.
        // Postgres treats NULL != NULL by default in unique indexes, so we use
        // a partial filtered index for the null branch slot.
        builder.HasIndex(r => new { r.TenantId, r.BranchId })
            .IsUnique()
            .HasFilter("\"BranchId\" IS NOT NULL");

        builder.HasIndex(r => r.TenantId)
            .IsUnique()
            .HasFilter("\"BranchId\" IS NULL");
    }
}
