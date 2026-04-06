using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class MembershipCardConfiguration : IEntityTypeConfiguration<MembershipCard>
{
    public void Configure(EntityTypeBuilder<MembershipCard> builder)
    {
        builder.ToTable("MembershipCards");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(m => m.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(m => m.CustomerId).IsRequired().HasMaxLength(36);
        builder.Property(m => m.CardNumber).IsRequired().HasMaxLength(20);
        builder.Property(m => m.CurrentTier).IsRequired().HasConversion<int>();
        builder.Property(m => m.PointsBalance).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.LifetimePointsEarned).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.LifetimePointsRedeemed).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(m => m.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(m => m.UpdatedAt).IsRequired();

        // One card per customer per tenant
        builder.HasIndex(m => new { m.TenantId, m.CustomerId }).IsUnique();
        // Card number globally unique for QR scanning
        builder.HasIndex(m => m.CardNumber).IsUnique();

        builder.HasOne(m => m.Tenant).WithMany()
            .HasForeignKey(m => m.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Customer).WithOne(c => c.MembershipCard)
            .HasForeignKey<MembershipCard>(m => m.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}
