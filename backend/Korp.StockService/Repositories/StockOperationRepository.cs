using Korp.StockService.Data;
using Korp.StockService.Models;
using Microsoft.EntityFrameworkCore;

namespace Korp.StockService.Repositories;

public sealed class StockOperationRepository(AppDbContext dbContext)
{
    public Task AcquireKeyLockAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtext({key}))",
            cancellationToken);

    public Task<StockOperation?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.StockOperations.AsNoTracking()
            .FirstOrDefaultAsync(operation => operation.IdempotencyKey == key, cancellationToken);

    public void Add(StockOperation operation) => dbContext.StockOperations.Add(operation);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
