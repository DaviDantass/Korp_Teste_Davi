using Korp.StockService.DTOs;
using Korp.StockService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Korp.StockService.Controllers;

[ApiController]
[Route("api/stock")]
public sealed class StockController(StockOperationsService stockService) : ControllerBase
{
    [HttpPost("{id:guid}/stock-in")]
    public async Task<ActionResult<ProductResponse>> AddStock(Guid id, StockMovementRequest request, CancellationToken cancellationToken) =>
        Ok(await stockService.AddStockAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/stock-out")]
    public async Task<ActionResult<ProductResponse>> WithdrawStock(Guid id, StockMovementRequest request, CancellationToken cancellationToken) =>
        Ok(await stockService.WithdrawStockAsync(id, request, cancellationToken));

    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> WithdrawMany(
        StockWithdrawalRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await stockService.WithdrawManyAsync(
            request,
            cancellationToken));
    }
}
