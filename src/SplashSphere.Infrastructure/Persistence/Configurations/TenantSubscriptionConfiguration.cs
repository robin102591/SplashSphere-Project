using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(s => s.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.PlanTier)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(PlanTier.Trial);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(SubscriptionStatus.Trial);

        builder.Property(s => s.TrialStartDate).IsRequired();
        builder.Property(s => s.TrialEndDate).IsRequired();

        builder.Property(s => s.FeatureOverrides)
            .HasColumnType("jsonb");

        builder.Property(s => s.SmsUsedThisMonth)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.SmsCountResetDate)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // One-to-one with Tenant
        builder.HasIndex(s => s.TenantId).IsUnique();

        builder.HasOne(s => s.Tenant)
            .WithOne(t => t.Subscription)
            .HasForeignKey<TenantSubscription>(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Computed property — EF ignores it
        builder.Ignore(s => s.TrialExpired);
    }
}
