using Korp.BillingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.BillingService.Data.Configurations;

public sealed class InvoiceConfiguration
    : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.Id)
            .ValueGeneratedNever();

        builder.Property(invoice => invoice.Number)
            .HasColumnName("number")
            .HasDefaultValueSql("nextval('invoice_number_seq')")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.HasIndex(invoice => invoice.Number)
            .IsUnique();

        builder.Property(invoice => invoice.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(invoice => invoice.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(invoice => invoice.ClosedAt)
            .HasColumnName("closed_at")
            .HasColumnType("timestamp with time zone");

        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}