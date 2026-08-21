using Korp.BillingService.Models;

namespace Korp.BillingService.Tests;

public sealed class InvoiceTests
{
    [Fact]
    public void Constructor_WithItems_ShouldCreateOpenInvoice()
    {
        var item = new InvoiceItem(Guid.NewGuid(), 2);
        var invoice = new Invoice([item]);

        Assert.Equal(InvoiceStatus.Open, invoice.Status);
        Assert.Single(invoice.Items);
        Assert.Equal(invoice.Id, item.InvoiceId);
        Assert.Null(invoice.ClosedAt);
    }

    [Fact]
    public void Constructor_WithoutItems_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Invoice([]));
    }

    [Fact]
    public void Constructor_WithRepeatedProduct_ShouldThrow()
    {
        var productId = Guid.NewGuid();
        var items = new[]
        {
            new InvoiceItem(productId, 1),
            new InvoiceItem(productId, 2)
        };

        Assert.Throws<ArgumentException>(() => new Invoice(items));
    }

    [Fact]
    public void InvoiceItem_WithNonPositiveQuantity_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new InvoiceItem(Guid.NewGuid(), 0));
    }

    [Fact]
    public void Close_ShouldChangeStatusAndPreventClosingAgain()
    {
        var invoice = new Invoice(
            [new InvoiceItem(Guid.NewGuid(), 1)]);

        invoice.Close();

        Assert.Equal(InvoiceStatus.Closed, invoice.Status);
        Assert.NotNull(invoice.ClosedAt);
        Assert.Throws<InvalidOperationException>(() => invoice.Close());
    }
}
