using Korp.BillingService.Models;
  using Microsoft.EntityFrameworkCore;

  namespace Korp.BillingService.Data;

  public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
      : DbContext(options)
  {
      public DbSet<Invoice> Invoices => Set<Invoice>();

      public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
          modelBuilder.HasSequence<long>("invoice_number_seq")
              .StartsAt(1)
              .IncrementsBy(1);

          modelBuilder.ApplyConfigurationsFromAssembly(
              typeof(AppDbContext).Assembly);
      }
  }