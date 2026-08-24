namespace Korp.StockService.DTOs;

public sealed record PagedProductsResponse(
    IReadOnlyList<ProductResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
