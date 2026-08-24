using Korp.StockService.Data;
using Korp.StockService.Models;
using Microsoft.EntityFrameworkCore;

namespace Korp.StockService.Repositories;

public sealed class ProductRepository(AppDbContext dbContext)
{
    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
    }

    public Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public Task<Product?> GetByIdAsNoTrackingAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public Task<Product?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return dbContext.Products
            .FirstOrDefaultAsync(
                product => product.Code == normalizedCode,
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalItems)> ListAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(product =>
                product.Code.Contains(term) || product.Description.Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .OrderBy(product => product.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<bool> TryWithdrawStockAsync(
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "A quantidade da baixa deve ser maior que zero.");
        }

        var affectedRows = await dbContext.Products
            .Where(product => product.Id == productId && product.Stock >= quantity)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    product => product.Stock,
                    product => product.Stock - quantity),
                cancellationToken);

        return affectedRows == 1;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
