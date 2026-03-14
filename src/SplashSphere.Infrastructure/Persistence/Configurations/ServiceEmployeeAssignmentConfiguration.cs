using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ServiceEmployeeAssignmentConfiguration : IEntityTypeConfiguration<ServiceEmployeeAssignment>
{
    public void Configure(EntityTypeBuilder<ServiceEmployeeAssignment> builder)
    {
        builder.ToTable("ServiceEmployeeAssignments");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(sea => sea.Id);
        builder.Property(sea => sea.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(sea => sea.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sea => sea.TransactionServiceId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(sea => sea.EmployeeId)
            .IsRequired()
            .HasMaxLength(36);

        // Per-employee share: Math.Round(totalCommission / count, 2, MidpointRounding.AwayFromZero)
        builder.Property(sea => sea.CommissionAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(sea => sea.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sea => sea.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: assignments deleted when parent TransactionService is deleted
        builder.HasOne(sea => sea.TransactionService)
            .WithMany(ts => ts.EmployeeAssignments)
            .HasForeignKey(sea => sea.TransactionServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete an employee with commission assignment history
        builder.HasOne(sea => sea.Employee)
            .WithMany(e => e.ServiceAssignments)
            .HasForeignKey(sea => sea.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique constraint: one assignment per employee per service line item ─
        builder.HasIndex(sea => new { sea.TransactionServiceId, sea.EmployeeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(sea => sea.TenantId);
        builder.HasIndex(sea => sea.EmployeeId);
    }
}
