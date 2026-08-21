using System.ComponentModel.DataAnnotations;

namespace Korp.StockService.DTOs;

public sealed record StockWithdrawalItemRequest(
    [param: Required] Guid ProductId,
    [param: Range(1, int.MaxValue)] int Quantity);
