using Korp.StockService.Models;

namespace Korp.StockService.Repositories;

public interface IProductRepository
{
    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<bool> TryWithdrawStockAsync(
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
