namespace Korp.BillingService.Exceptions;

  public sealed class InvoiceNotFoundException(Guid invoiceId)
      : Exception($"Nota fiscal '{invoiceId}' não encontrada.");