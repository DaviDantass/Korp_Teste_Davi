using System.Net;
using Korp.BillingService.Clients;
using Korp.BillingService.Exceptions;

namespace Korp.BillingService.Tests.Clients;

public sealed class StockServiceClientTests
{
    [Fact]
    public async Task WithdrawAsync_WhenStockReturnsSuccess_ShouldComplete()
    {
        using var httpClient = CreateClient(HttpStatusCode.OK);
        var client = new StockServiceClient(httpClient);

        await client.WithdrawAsync(CreateRequest());
    }

    [Fact]
    public async Task WithdrawAsync_WhenStockReturnsConflict_ShouldThrowConflict()
    {
        using var httpClient = CreateClient(HttpStatusCode.Conflict);
        var client = new StockServiceClient(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.WithdrawAsync(CreateRequest()));
    }

    [Fact]
    public async Task WithdrawAsync_WhenStockIsUnavailable_ShouldThrowUnavailable()
    {
        using var httpClient = new HttpClient(new FailingHandler())
        {
            BaseAddress = new Uri("http://stock-service")
        };
        var client = new StockServiceClient(httpClient);

        await Assert.ThrowsAsync<StockServiceUnavailableException>(() =>
            client.WithdrawAsync(CreateRequest()));
    }

    private static HttpClient CreateClient(HttpStatusCode statusCode) =>
        new(new StatusHandler(statusCode))
        {
            BaseAddress = new Uri("http://stock-service")
        };

    private static StockWithdrawalRequest CreateRequest() =>
        new(
            "test-key",
            [new StockWithdrawalItemRequest(Guid.NewGuid(), 2)]);

    private sealed class StatusHandler(HttpStatusCode statusCode)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode));
    }

    private sealed class FailingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("Stock indisponível.");
    }
}
