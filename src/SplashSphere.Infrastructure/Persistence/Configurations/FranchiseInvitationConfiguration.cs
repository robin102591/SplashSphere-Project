using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class FranchiseInvitationConfiguration : IEntityTypeConfiguration<FranchiseInvitation>
{
    public void Configure(EntityTypeBuilder<FranchiseInvitation> builder)
    {
        builder.ToTable("FranchiseInvitations");

        builder.HasKey(fi => fi.Id);
        builder.Property(fi => fi.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(fi => fi.FranchisorTenantId).IsRequired().HasMaxLength(256);
        builder.Property(fi => fi.Email).IsRequired().HasMaxLength(256);
        builder.Property(fi => fi.BusinessName).IsRequired().HasMaxLength(256);
        builder.Property(fi => fi.OwnerName).HasMaxLength(256);
        builder.Property(fi => fi.FranchiseCode).HasMaxLength(50);
        builder.Property(fi => fi.TerritoryName).HasMaxLength(256);
        builder.Property(fi => fi.Token).IsRequired().HasMaxLength(128);
        builder.Property(fi => fi.ExpiresAt).IsRequired();
        builder.Property(fi => fi.IsUsed).IsRequired().HasDefaultValue(false);
        builder.Property(fi => fi.AcceptedByTenantId).HasMaxLength(256);

        builder.Property(fi => fi.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(fi => fi.UpdatedAt).IsRequired();

        // Unique token for invitation lookup
        builder.HasIndex(fi => fi.Token).IsUnique();
        builder.HasIndex(fi => fi.FranchisorTenantId);

        builder.HasOne(fi => fi.FranchisorTenant).WithMany()
            .HasForeignKey(fi => fi.FranchisorTenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
