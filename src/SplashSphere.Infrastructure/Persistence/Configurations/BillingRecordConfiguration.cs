using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class BillingRecordConfiguration : IEntityTypeConfiguration<BillingRecord>
{
    public void Configure(EntityTypeBuilder<BillingRecord> builder)
    {
        builder.ToTable("BillingRecords");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(b => b.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(b => b.SubscriptionId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(b => b.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("PHP");

        builder.Property(b => b.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(BillingStatus.Pending);

        builder.Property(b => b.PaymentGatewayId).HasMaxLength(256);
        builder.Property(b => b.PaymentMethod).HasMaxLength(50);
        builder.Property(b => b.InvoiceNumber).HasMaxLength(50);
        builder.Property(b => b.Notes).HasMaxLength(500);

        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(b => new { b.TenantId, b.BillingDate });
        builder.HasIndex(b => b.InvoiceNumber)
            .IsUnique()
            .HasFilter("\"InvoiceNumber\" IS NOT NULL");

        // FK to Tenant
        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to Subscription
        builder.HasOne(b => b.Subscription)
            .WithMany()
            .HasForeignKey(b => b.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
