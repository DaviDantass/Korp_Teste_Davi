namespace Korp.BillingService.Models;

public sealed class InvoiceItem
{
    private InvoiceItem()
    {
    }

    public InvoiceItem(Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException(
                "O produto é obrigatório.",
                nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "A quantidade deve ser maior que zero.");

        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }

    public Guid InvoiceId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    internal void AssignToInvoice(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
            throw new ArgumentException(
                "A nota fiscal é obrigatória.",
                nameof(invoiceId));

        if (InvoiceId != Guid.Empty && InvoiceId != invoiceId)
            throw new InvalidOperationException(
                "O item já pertence a outra nota fiscal.");

        InvoiceId = invoiceId;
    }
}
