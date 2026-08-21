namespace Korp.StockService.Models;

public sealed class StockOperation
{
    private StockOperation()
    {
    }

    public StockOperation(
        string idempotencyKey,
        string requestHash,
        string resultJson)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException(
                "A chave de idempotência é obrigatória.",
                nameof(idempotencyKey));

        if (string.IsNullOrWhiteSpace(requestHash))
            throw new ArgumentException(
                "O hash da requisição é obrigatório.",
                nameof(requestHash));

        if (string.IsNullOrWhiteSpace(resultJson))
            throw new ArgumentException(
                "O resultado da operação é obrigatório.",
                nameof(resultJson));

        Id = Guid.NewGuid();
        IdempotencyKey = idempotencyKey.Trim();
        RequestHash = requestHash;
        ResultJson = resultJson;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public string ResultJson { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
