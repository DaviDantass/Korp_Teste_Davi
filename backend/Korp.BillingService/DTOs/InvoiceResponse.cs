using Korp.BillingService.Models;

  namespace Korp.BillingService.DTOs;

  public sealed record InvoiceResponse(
      Guid Id,
      long Number,
      InvoiceStatus Status,
      DateTime CreatedAt,
      DateTime? ClosedAt,
      IReadOnlyList<InvoiceItemResponse> Items);