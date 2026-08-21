using Korp.BillingService.DTOs;
using Microsoft.AspNetCore.Mvc;
using Korp.BillingService.Services;

namespace Korp.BillingService.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController(
    InvoiceService invoiceService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(
        typeof(InvoiceResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<InvoiceResponse>> Create(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await invoiceService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<InvoiceResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await invoiceService.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(InvoiceResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await invoiceService.GetByIdAsync(
            id,
            cancellationToken));
    }
}
