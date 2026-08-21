namespace Korp.StockService.Exceptions;

public sealed class ProductAlreadyExistsException(string code)
    : Exception($"Já existe um produto com o código '{code}'.");
