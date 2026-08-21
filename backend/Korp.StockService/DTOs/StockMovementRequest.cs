using System.ComponentModel.DataAnnotations;

namespace Korp.StockService.DTOs;

public sealed record StockMovementRequest(
    [param: Range(1, int.MaxValue)] int Quantity);
