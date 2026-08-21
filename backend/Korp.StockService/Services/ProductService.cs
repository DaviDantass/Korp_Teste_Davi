using Korp.StockService.DTOs;
using Korp.StockService.Exceptions;
using Korp.StockService.Models;
using Korp.StockService.Repositories;

namespace Korp.StockService.Services;

public sealed class ProductService(ProductRepository productRepository)
{
    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var existingProduct = await productRepository.GetByCodeAsync(
            normalizedCode,
            cancellationToken);

        if (existingProduct is not null)
        {
            throw new ProductAlreadyExistsException(normalizedCode);
        }

        var product = new Product(
            request.Code,
            request.Description,
            request.InitialStock);

        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await productRepository.ListAsync(cancellationToken);

        return products.Select(ToResponse).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        return ToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        product.ChangeDescription(request.Description);
        await productRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductResponse> AddStockAsync(
        Guid productId,
        StockMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(
          productId,
          cancellationToken)
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
        var product = await productRepository.GetByIdAsync(
            productId,
            cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        var withdrawn = await productRepository.TryWithdrawStockAsync(
            productId,
            request.Quantity,
            cancellationToken);

        if (!withdrawn)
        {
            throw new InsufficientStockException(
                productId,
                request.Quantity);
        }

        var updatedProduct = await productRepository.GetByIdAsNoTrackingAsync(
            productId,
            cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        return ToResponse(updatedProduct);
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Code,
            product.Description,
            product.Stock);
    }
}
