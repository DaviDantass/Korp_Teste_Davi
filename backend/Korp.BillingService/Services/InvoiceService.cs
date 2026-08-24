using Korp.BillingService.Clients;
using Korp.BillingService.DTOs;
using Korp.BillingService.Exceptions;
using Korp.BillingService.Models;
using Korp.BillingService.Repositories;

namespace Korp.BillingService.Services;

public sealed class InvoiceService(
    InvoiceRepository invoiceRepository,
    StockServiceClient stockServiceClient)
{
    public async Task<InvoiceResponse> CreateAsync(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var items = request.Items
            .Select(item => new InvoiceItem(item.ProductId, item.Quantity))
            .ToList();

        var invoice = new Invoice(items);

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(invoice);
    }

    public async Task<PagedInvoicesResponse> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await invoiceRepository.ListAsync(page, pageSize, cancellationToken);
        return new PagedInvoicesResponse(
            result.Items.Select(ToResponse).ToList(),
            page,
            pageSize,
            result.TotalItems,
            (int)Math.Ceiling(result.TotalItems / (double)pageSize));
    }

    public async Task<InvoiceResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvoiceNotFoundException(id);

        return ToResponse(invoice);
    }

    public async Task<InvoiceResponse> CloseAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvoiceNotFoundException(id);

        if (invoice.Status == InvoiceStatus.Closed)
            throw new InvalidOperationException("A nota já está fechada.");

        var withdrawalRequest = new StockWithdrawalRequest(
            $"invoice-close:{invoice.Id}",
            invoice.Items
                .Select(item => new StockWithdrawalItemRequest(
                    item.ProductId,
                    item.Quantity))
                .ToList());

        await stockServiceClient.WithdrawAsync(
            withdrawalRequest,
            cancellationToken);

        invoice.Close();
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(invoice);
    }

    private static InvoiceResponse ToResponse(Invoice invoice) =>
        new(
            invoice.Id,
            invoice.Number,
            invoice.Status,
            invoice.CreatedAt,
            invoice.ClosedAt,
            invoice.Items
                .Select(item => new InvoiceItemResponse(
                    item.ProductId,
                    item.Quantity))
                .ToList());
}
