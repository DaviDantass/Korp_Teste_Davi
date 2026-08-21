namespace Korp.StockService.Models;

public sealed class Product
{
    private Product() { }

    public Product(string code, string description, int initialStock)
    {
        Id = Guid.NewGuid();
        Code = NormalizeCode(code);
        Description = ValidateDescription(description);
        Stock = ValidateStock(initialStock);
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Stock { get; private set; }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade de entrada deve ser maior que zero.");

        checked { Stock += quantity; }
    }

    public void WithdrawStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade da baixa deve ser maior que zero.");

        if (quantity > Stock)
            throw new InvalidOperationException("Saldo insuficiente para realizar a baixa.");

        Stock -= quantity;
    }

    public void ChangeDescription(string description) => Description = ValidateDescription(description);

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("O código é obrigatório.", nameof(code));

        return code.Trim().ToUpperInvariant();
    }

    private static string ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição é obrigatória.", nameof(description));

        return description.Trim();
    }

    private static int ValidateStock(int stock)
    {
        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "O saldo inicial não pode ser negativo.");

        return stock;
    }
}
