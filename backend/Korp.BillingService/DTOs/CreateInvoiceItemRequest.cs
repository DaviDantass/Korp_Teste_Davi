using System.ComponentModel.DataAnnotations;

  namespace Korp.BillingService.DTOs;

  public sealed record CreateInvoiceItemRequest(
      Guid ProductId,

      [param: Range(1, int.MaxValue)]
      int Quantity);