using Korp.StockService.DTOs;
using Korp.StockService.Exceptions;
using Korp.StockService.Models;
using Korp.StockService.Repositories;

namespace Korp.StockService.Services;

public sealed class ProductService(ProductRepository productRepository)
{
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await productRepository.GetByCodeAsync(normalizedCode, cancellationToken) is not null)
            throw new ProductAlreadyExistsException(normalizedCode);

        var product = new Product(request.Code, request.Description, request.InitialStock);
        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(product);
    }

    public async Task<PagedProductsResponse> ListAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await productRepository.ListAsync(page, pageSize, search, cancellationToken);
        return new PagedProductsResponse(
            result.Items.Select(ToResponse).ToList(),
            page,
            pageSize,
            result.TotalItems,
            (int)Math.Ceiling(result.TotalItems / (double)pageSize));
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);
        return ToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);
        product.ChangeDescription(request.Description);
        await productRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(product);
    }

    private static ProductResponse ToResponse(Product product) =>
        new(product.Id, product.Code, product.Description, product.Stock);
}
