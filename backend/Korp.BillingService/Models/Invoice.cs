namespace Korp.BillingService.Models;

public sealed class Invoice
{
    private Invoice()
    {
    }

    public Invoice(IEnumerable<InvoiceItem> items)
    {
        var invoiceItems = items?.ToList()
            ?? throw new ArgumentNullException(nameof(items));

        if (invoiceItems.Count == 0)
            throw new ArgumentException(
                "A nota deve possuir pelo menos um item.",
                nameof(items));

        if (invoiceItems
            .GroupBy(item => item.ProductId)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "A nota não pode possuir o mesmo produto mais de uma vez.",
                nameof(items));
        }

        Id = Guid.NewGuid();
        Status = InvoiceStatus.Open;
        CreatedAt = DateTime.UtcNow;

        foreach (var item in invoiceItems)
            item.AssignToInvoice(Id);

        Items = invoiceItems;
    }

    public Guid Id { get; private set; }
    public long Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items { get; private set; } =
        new List<InvoiceItem>();

    public void Close()
    {
        if (Status == InvoiceStatus.Closed)
            throw new InvalidOperationException("A nota já está fechada.");

        Status = InvoiceStatus.Closed;
        ClosedAt = DateTime.UtcNow;
    }
}
