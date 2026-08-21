namespace Korp.BillingService.Exceptions;

public sealed class StockServiceUnavailableException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);
