namespace Korp.StockService.Models;

public sealed class StockOperation
{
    private StockOperation() { }

    public StockOperation(string idempotencyKey, string resultJson)
    {
        Id = Guid.NewGuid();
        IdempotencyKey = idempotencyKey;
        ResultJson = resultJson;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string ResultJson { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
