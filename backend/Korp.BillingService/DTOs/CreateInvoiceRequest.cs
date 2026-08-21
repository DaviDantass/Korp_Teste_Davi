using System.ComponentModel.DataAnnotations;

  namespace Korp.BillingService.DTOs;

  public sealed record CreateInvoiceRequest(
      [param: Required, MinLength(1)]
      IReadOnlyList<CreateInvoiceItemRequest> Items);