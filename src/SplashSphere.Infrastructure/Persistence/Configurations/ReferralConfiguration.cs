using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.ToTable("Referrals");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(r => r.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.ReferrerCustomerId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(r => r.ReferredCustomerId)
            .HasMaxLength(36);

        builder.Property(r => r.ReferralCode)
            .IsRequired()
            .HasMaxLength(32);

        // Stored as int — Pending=1, Completed=2, Expired=3
        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.ReferrerPointsEarned)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.ReferredPointsEarned)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.CompletedAt);
        builder.Property(r => r.ExpiredAt);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Referrer)
            .WithMany()
            .HasForeignKey(r => r.ReferrerCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Referred)
            .WithMany()
            .HasForeignKey(r => r.ReferredCustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Indexes ───────────────────────────────────────────────────────────
        // Referral code unique within a tenant (different tenants can reuse a code)
        builder.HasIndex(r => new { r.TenantId, r.ReferralCode })
            .IsUnique()
            .HasDatabaseName("UX_Referral_Tenant_Code");

        builder.HasIndex(r => new { r.TenantId, r.ReferrerCustomerId });
        // Used by ExpireReferrals Hangfire job
        builder.HasIndex(r => new { r.Status, r.CreatedAt });
    }
}
