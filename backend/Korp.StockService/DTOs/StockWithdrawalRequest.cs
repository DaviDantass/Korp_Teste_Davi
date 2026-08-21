using System.ComponentModel.DataAnnotations;

namespace Korp.StockService.DTOs;

public sealed record StockWithdrawalRequest(
    [param: Required, StringLength(100, MinimumLength = 1)]
    string IdempotencyKey,

    [param: Required, MinLength(1)]
    IReadOnlyList<StockWithdrawalItemRequest> Items);
