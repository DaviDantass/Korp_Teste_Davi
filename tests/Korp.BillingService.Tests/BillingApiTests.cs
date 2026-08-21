using System.Net;
using System.Net.Http.Json;
using Korp.BillingService.Clients;
using Korp.BillingService.DTOs;
using Korp.BillingService.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Korp.BillingService.Tests;

public sealed class BillingApiTests
{
    [Fact]
    public async Task CloseInvoice_WhenStockSucceeds_ShouldCloseInvoice()
    {
        using var factory = new BillingApiFactory(HttpStatusCode.OK);
        using var client = factory.CreateClient();
        var invoice = await CreateInvoiceAsync(client);

        var response = await client.PostAsync(
            $"/api/invoices/{invoice.Id}/close",
            content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var closed = await response.Content.ReadFromJsonAsync<InvoiceResponse>();

        Assert.NotNull(closed);
        Assert.Equal(InvoiceStatus.Closed, closed.Status);
        Assert.NotNull(closed.ClosedAt);
    }

    [Fact]
    public async Task CloseInvoice_WhenStockFails_ShouldKeepInvoiceOpen()
    {
        using var factory = new BillingApiFactory(
            HttpStatusCode.ServiceUnavailable);
        using var client = factory.CreateClient();
        var invoice = await CreateInvoiceAsync(client);

        var closeResponse = await client.PostAsync(
            $"/api/invoices/{invoice.Id}/close",
            content: null);

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            closeResponse.StatusCode);

        var getResponse = await client.GetAsync(
            $"/api/invoices/{invoice.Id}");
        var current = await getResponse.Content
            .ReadFromJsonAsync<InvoiceResponse>();

        Assert.NotNull(current);
        Assert.Equal(InvoiceStatus.Open, current.Status);
        Assert.Null(current.ClosedAt);
    }

    private static async Task<InvoiceResponse> CreateInvoiceAsync(
        HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/invoices",
            new CreateInvoiceRequest(
                [new CreateInvoiceItemRequest(Guid.NewGuid(), 1)]));

        response.EnsureSuccessStatusCode();
        return (await response.Content
            .ReadFromJsonAsync<InvoiceResponse>())!;
    }
}

public sealed class BillingApiFactory(HttpStatusCode stockStatusCode)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<StockServiceClient>();
            services.AddSingleton(new StockServiceClient(
                new HttpClient(new StubHandler(stockStatusCode))
                {
                    BaseAddress = new Uri("http://stock-service")
                }));
        });
    }

    private sealed class StubHandler(HttpStatusCode statusCode)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode));
    }
}
