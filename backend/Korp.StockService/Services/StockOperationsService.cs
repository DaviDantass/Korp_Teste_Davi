using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Korp.StockService.Data;
using Korp.StockService.DTOs;
using Korp.StockService.Exceptions;
using Korp.StockService.Models;
using Korp.StockService.Repositories;

namespace Korp.StockService.Services;

public sealed class StockOperationsService(
    ProductRepository productRepository,
    StockOperationRepository operationRepository,
    AppDbContext dbContext)
{
    public async Task<ProductResponse> AddStockAsync(
        Guid productId,
        StockMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        product.AddStock(request.Quantity);
        await productRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(product);
    }

    public async Task<ProductResponse> WithdrawStockAsync(
        Guid productId,
        StockMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = await productRepository.GetByIdAsNoTrackingAsync(productId, cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        if (!await productRepository.TryWithdrawStockAsync(productId, request.Quantity, cancellationToken))
            throw new InsufficientStockException(productId, request.Quantity);

        var updated = await productRepository.GetByIdAsNoTrackingAsync(productId, cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        return ToResponse(updated);
    }

    public async Task<IReadOnlyList<ProductResponse>> WithdrawManyAsync(
        StockWithdrawalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Items.GroupBy(item => item.ProductId).Any(group => group.Count() > 1))
            throw new ArgumentException(
                "O mesmo produto não pode aparecer mais de uma vez na baixa.");

        var idempotencyKey = request.IdempotencyKey.Trim();
        var requestHash = ComputeRequestHash(request.Items);

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await operationRepository.AcquireKeyLockAsync(
            idempotencyKey,
            cancellationToken);

        var existing = await operationRepository.GetByKeyAsync(
            idempotencyKey,
            cancellationToken);

        if (existing is not null)
        {
            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "A chave de idempotência já foi usada para outra requisição.");

            await transaction.CommitAsync(cancellationToken);
            return Deserialize(existing.ResultJson);
        }

        var results = new List<ProductResponse>();

        foreach (var item in request.Items)
        {
            _ = await productRepository.GetByIdAsNoTrackingAsync(
                    item.ProductId,
                    cancellationToken)
                ?? throw new ProductNotFoundException(item.ProductId);

            if (!await productRepository.TryWithdrawStockAsync(
                    item.ProductId,
                    item.Quantity,
                    cancellationToken))
            {
                throw new InsufficientStockException(item.ProductId, item.Quantity);
            }

            var updated = await productRepository.GetByIdAsNoTrackingAsync(
                    item.ProductId,
                    cancellationToken)
                ?? throw new ProductNotFoundException(item.ProductId);

            results.Add(ToResponse(updated));
        }

        operationRepository.Add(new StockOperation(
            idempotencyKey,
            requestHash,
            JsonSerializer.Serialize(results)));

        await operationRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return results;
    }

    private static string ComputeRequestHash(
        IReadOnlyList<StockWithdrawalItemRequest> items)
    {
        var payload = JsonSerializer.Serialize(items);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    private static IReadOnlyList<ProductResponse> Deserialize(string json) =>
        JsonSerializer.Deserialize<List<ProductResponse>>(json)
        ?? throw new InvalidOperationException("Resultado de operação inválido.");

    private static ProductResponse ToResponse(Product product) =>
        new(product.Id, product.Code, product.Description, product.Stock);
}
