namespace Korp.BillingService.DTOs;

  public sealed record InvoiceItemResponse(
      Guid ProductId,
      int Quantity);