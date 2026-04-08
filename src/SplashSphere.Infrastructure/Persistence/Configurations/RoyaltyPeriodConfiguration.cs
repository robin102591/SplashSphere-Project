using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class RoyaltyPeriodConfiguration : IEntityTypeConfiguration<RoyaltyPeriod>
{
    public void Configure(EntityTypeBuilder<RoyaltyPeriod> builder)
    {
        builder.ToTable("RoyaltyPeriods");

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(rp => rp.FranchisorTenantId).IsRequired().HasMaxLength(256);
        builder.Property(rp => rp.FranchiseeTenantId).IsRequired().HasMaxLength(256);
        builder.Property(rp => rp.AgreementId).IsRequired().HasMaxLength(36);

        builder.Property(rp => rp.PeriodStart).IsRequired();
        builder.Property(rp => rp.PeriodEnd).IsRequired();
        builder.Property(rp => rp.GrossRevenue).IsRequired().HasPrecision(14, 2);
        builder.Property(rp => rp.RoyaltyRate).IsRequired().HasPrecision(10, 4);
        builder.Property(rp => rp.RoyaltyAmount).IsRequired().HasPrecision(14, 2);
        builder.Property(rp => rp.MarketingFeeRate).IsRequired().HasPrecision(10, 4);
        builder.Property(rp => rp.MarketingFeeAmount).IsRequired().HasPrecision(14, 2);
        builder.Property(rp => rp.TechnologyFeeRate).IsRequired().HasPrecision(10, 4);
        builder.Property(rp => rp.TechnologyFeeAmount).IsRequired().HasPrecision(14, 2);
        builder.Property(rp => rp.TotalDue).IsRequired().HasPrecision(14, 2);

        builder.Property(rp => rp.Status).IsRequired().HasConversion<int>();
        builder.Property(rp => rp.PaymentReference).HasMaxLength(256);

        builder.Property(rp => rp.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(rp => rp.UpdatedAt).IsRequired();

        // Unique: one royalty period per franchisee per date range
        builder.HasIndex(rp => new { rp.FranchiseeTenantId, rp.PeriodStart, rp.PeriodEnd }).IsUnique();

        // FK to Agreement
        builder.HasOne(rp => rp.Agreement).WithMany(a => a.RoyaltyPeriods)
            .HasForeignKey(rp => rp.AgreementId).OnDelete(DeleteBehavior.Restrict);

        // FKs to Tenant
        builder.HasOne(rp => rp.FranchisorTenant).WithMany()
            .HasForeignKey(rp => rp.FranchisorTenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(rp => rp.FranchiseeTenant).WithMany()
            .HasForeignKey(rp => rp.FranchiseeTenantId).OnDelete(DeleteBehavior.Restrict);
    }
}
