namespace Korp.BillingService.DTOs;

public sealed record PagedInvoicesResponse(
    IReadOnlyList<InvoiceResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
