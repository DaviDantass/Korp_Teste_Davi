using System.ComponentModel.DataAnnotations;

namespace Korp.StockService.DTOs;

public sealed record CreateProductRequest(
    [param: Required, StringLength(50, MinimumLength = 1)] string Code,
    [param: Required, StringLength(200, MinimumLength = 1)] string Description,
    [param: Range(0, int.MaxValue)] int InitialStock);
