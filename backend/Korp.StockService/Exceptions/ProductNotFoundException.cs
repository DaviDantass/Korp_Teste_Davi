namespace Korp.StockService.Exceptions;

public sealed class ProductNotFoundException(Guid id)
    : Exception($"Produto '{id}' não foi encontrado.");
