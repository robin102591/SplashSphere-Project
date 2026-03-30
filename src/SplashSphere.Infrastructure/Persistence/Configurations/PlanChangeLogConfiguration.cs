using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PlanChangeLogConfiguration : IEntityTypeConfiguration<PlanChangeLog>
{
    public void Configure(EntityTypeBuilder<PlanChangeLog> builder)
    {
        builder.ToTable("PlanChangeLogs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(l => l.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.FromPlan)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.ToPlan)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.ChangedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(l => l.UpdatedAt)
            .IsRequired();

        // Index for querying tenant's plan history
        builder.HasIndex(l => new { l.TenantId, l.CreatedAt });

        // FK to Tenant
        builder.HasOne(l => l.Tenant)
            .WithMany()
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
