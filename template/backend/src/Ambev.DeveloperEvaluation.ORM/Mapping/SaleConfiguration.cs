using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.Property(x => x.SaleNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SaleDate).IsRequired();
        builder.Property(x => x.CustomerExternalId).HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.BranchExternalId).HasColumnType("uuid").IsRequired();
        builder.Property(x => x.BranchName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.IsCancelled).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.HasIndex(x => x.SaleNumber).IsUnique();
        builder.HasIndex(x => x.SaleDate);
        builder.HasIndex(x => x.CustomerExternalId);
        builder.HasIndex(x => x.BranchExternalId);
        builder.HasIndex(x => new { x.IsCancelled, x.SaleDate });
        builder.HasMany(x => x.Items)
       .WithOne()
       .HasForeignKey("SaleId")
       .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Sale.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
