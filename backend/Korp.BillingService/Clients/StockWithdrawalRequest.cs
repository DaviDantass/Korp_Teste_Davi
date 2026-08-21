namespace Korp.BillingService.Clients;

  public sealed record StockWithdrawalRequest(
      string IdempotencyKey,
      IReadOnlyList<StockWithdrawalItemRequest> Items);

  public sealed record StockWithdrawalItemRequest(
      Guid ProductId,
      int Quantity);