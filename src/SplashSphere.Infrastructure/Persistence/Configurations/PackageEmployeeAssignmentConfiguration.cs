using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PackageEmployeeAssignmentConfiguration : IEntityTypeConfiguration<PackageEmployeeAssignment>
{
    public void Configure(EntityTypeBuilder<PackageEmployeeAssignment> builder)
    {
        builder.ToTable("PackageEmployeeAssignments");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(pea => pea.Id);
        builder.Property(pea => pea.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(pea => pea.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pea => pea.TransactionPackageId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(pea => pea.EmployeeId)
            .IsRequired()
            .HasMaxLength(36);

        // Per-employee share: Math.Round(totalCommission / count, 2, MidpointRounding.AwayFromZero)
        builder.Property(pea => pea.CommissionAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(pea => pea.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(pea => pea.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: assignments deleted when parent TransactionPackage is deleted
        builder.HasOne(pea => pea.TransactionPackage)
            .WithMany(tp => tp.EmployeeAssignments)
            .HasForeignKey(pea => pea.TransactionPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete an employee with commission assignment history
        builder.HasOne(pea => pea.Employee)
            .WithMany(e => e.PackageAssignments)
            .HasForeignKey(pea => pea.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique constraint: one assignment per employee per package line item ─
        builder.HasIndex(pea => new { pea.TransactionPackageId, pea.EmployeeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(pea => pea.TenantId);
        builder.HasIndex(pea => pea.EmployeeId);
    }
}
