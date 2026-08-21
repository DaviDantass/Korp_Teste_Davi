namespace Korp.StockService.Exceptions;

public sealed class InsufficientStockException(Guid productId, int quantity)
    : Exception(
        $"Saldo insuficiente para baixar {quantity} unidade(s) do produto '{productId}'.");
