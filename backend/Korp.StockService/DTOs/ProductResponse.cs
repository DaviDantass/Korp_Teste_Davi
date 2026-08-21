namespace Korp.StockService.DTOs;

public sealed record ProductResponse(
    Guid Id,
    string Code,
    string Description,
    int Stock);
