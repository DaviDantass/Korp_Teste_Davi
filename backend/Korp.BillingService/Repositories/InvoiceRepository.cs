using Korp.BillingService.Data;
using Korp.BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace Korp.BillingService.Repositories;

public sealed class InvoiceRepository(AppDbContext dbContext)
{
    public async Task AddAsync(
          Invoice invoice,
          CancellationToken cancellationToken = default)
    {
        await dbContext.Invoices.AddAsync(invoice, cancellationToken);
    }
    public Task<Invoice?> GetByIdAsync(
          Guid id,
          CancellationToken cancellationToken = default)
    {
        return dbContext.Invoices
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(
                invoice => invoice.Id == id,
                cancellationToken);
    }
    public async Task<IReadOnlyList<Invoice>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .OrderByDescending(invoice => invoice.Number)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}