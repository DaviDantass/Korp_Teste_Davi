using Korp.StockService.Models;

namespace Korp.StockService.Tests.Models;

public class ProductTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateProductWithNormalizedCode()
    {
        var product = new Product("  abc-123 ", " Teclado ", 10);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("ABC-123", product.Code);
        Assert.Equal("Teclado", product.Description);
        Assert.Equal(10, product.Stock);
    }

    [Fact]
    public void AddStock_WithValidQuantity_ShouldIncreaseStock()
    {
        var product = new Product("ABC-123", "Teclado", 10);

        product.AddStock(5);

        Assert.Equal(15, product.Stock);
    }

    [Fact]
    public void WithdrawStock_WithValidQuantity_ShouldDecreaseStock()
    {
        var product = new Product("ABC-123", "Teclado", 10);

        product.WithdrawStock(4);

        Assert.Equal(6, product.Stock);
    }

    [Fact]
    public void Constructor_WithNegativeInitialStock_ShouldThrowArgumentOutOfRangeException()
    {
        var action = () => new Product("ABC-123", "Teclado", -1);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void WithdrawStock_WhenQuantityExceedsStock_ShouldThrowInvalidOperationException()
    {
        var product = new Product("ABC-123", "Teclado", 10);

        var action = () => product.WithdrawStock(11);

        Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(10, product.Stock);
    }
}
