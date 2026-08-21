using System.ComponentModel.DataAnnotations;

namespace Korp.StockService.DTOs;

public sealed record UpdateProductRequest(
    [param: Required, StringLength(200, MinimumLength = 1)] string Description);
