using Korp.StockService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.StockService.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_products_stock_non_negative",
                "stock >= 0");
        });

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .ValueGeneratedNever();

        builder.Property(product => product.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(product => product.Code)
            .IsUnique()
            .HasDatabaseName("ux_products_code");

        builder.Property(product => product.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Stock)
            .HasColumnName("stock")
            .IsRequired();
    }
}