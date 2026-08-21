using System.ComponentModel.DataAnnotations;

namespace Korp.StockService.DTOs;

public sealed record CreateProductRequest(
    [property: Required, StringLength(50, MinimumLength = 1)] string Code,
    [property: Required, StringLength(200, MinimumLength = 1)] string Description,
    [property: Range(0, int.MaxValue)] int InitialStock);
