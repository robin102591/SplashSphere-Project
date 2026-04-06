using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        builder.ToTable("PointTransactions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(p => p.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(p => p.MembershipCardId).IsRequired().HasMaxLength(36);
        builder.Property(p => p.Type).IsRequired().HasConversion<int>();
        builder.Property(p => p.Points).IsRequired();
        builder.Property(p => p.BalanceAfter).IsRequired();
        builder.Property(p => p.Description).IsRequired().HasMaxLength(500);
        builder.Property(p => p.TransactionId).HasMaxLength(36);
        builder.Property(p => p.RewardId).HasMaxLength(36);
        builder.Property(p => p.CreatedAt).IsRequired().HasDefaultValueSql("now()");

        builder.HasIndex(p => p.MembershipCardId);
        builder.HasIndex(p => p.TransactionId);
        builder.HasIndex(p => new { p.MembershipCardId, p.CreatedAt });

        builder.HasOne(p => p.MembershipCard).WithMany(m => m.PointTransactions)
            .HasForeignKey(p => p.MembershipCardId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Transaction).WithMany()
            .HasForeignKey(p => p.TransactionId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(p => p.Reward).WithMany()
            .HasForeignKey(p => p.RewardId).OnDelete(DeleteBehavior.SetNull);
    }
}
