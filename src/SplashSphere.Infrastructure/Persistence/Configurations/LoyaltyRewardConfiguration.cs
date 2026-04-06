using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class LoyaltyRewardConfiguration : IEntityTypeConfiguration<LoyaltyReward>
{
    public void Configure(EntityTypeBuilder<LoyaltyReward> builder)
    {
        builder.ToTable("LoyaltyRewards");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(r => r.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.RewardType).IsRequired().HasConversion<int>();
        builder.Property(r => r.PointsCost).IsRequired();
        builder.Property(r => r.ServiceId).HasMaxLength(36);
        builder.Property(r => r.PackageId).HasMaxLength(36);
        builder.Property(r => r.DiscountAmount).HasPrecision(10, 2);
        builder.Property(r => r.DiscountPercent).HasPrecision(5, 2);
        builder.Property(r => r.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(r => r.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.TenantId);

        builder.HasOne(r => r.Tenant).WithMany()
            .HasForeignKey(r => r.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Service).WithMany()
            .HasForeignKey(r => r.ServiceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.Package).WithMany()
            .HasForeignKey(r => r.PackageId).OnDelete(DeleteBehavior.SetNull);
    }
}
