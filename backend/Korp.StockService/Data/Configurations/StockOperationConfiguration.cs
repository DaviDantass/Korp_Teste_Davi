using Korp.StockService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.StockService.Data.Configurations;

public sealed class StockOperationConfiguration : IEntityTypeConfiguration<StockOperation>
{
    public void Configure(EntityTypeBuilder<StockOperation> builder)
    {
        builder.ToTable("stock_operations");
        builder.HasKey(operation => operation.Id);
        builder.Property(operation => operation.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.HasIndex(operation => operation.IdempotencyKey).IsUnique();
        builder.Property(operation => operation.RequestHash)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(operation => operation.ResultJson).IsRequired();
        builder.Property(operation => operation.CreatedAt).IsRequired();
    }
}
