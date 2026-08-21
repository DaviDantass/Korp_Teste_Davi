using System.Net;
using System.Net.Http.Json;
using Korp.StockService.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Korp.StockService.Tests;

public sealed class StockOperationsApiTests : IClassFixture<ProductsApiFactory>
{
    private readonly HttpClient client;

    public StockOperationsApiTests(ProductsApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task WithdrawMany_ShouldDebitAllProducts()
    {
        var first = await CreateProductAsync(10);
        var second = await CreateProductAsync(8);

        var response = await WithdrawAsync(
            $"batch-{Guid.NewGuid():N}",
            new StockWithdrawalItemRequest(first.Id, 2),
            new StockWithdrawalItemRequest(second.Id, 3));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();

        Assert.NotNull(products);
        Assert.Equal(2, products.Count);
        Assert.Contains(products, product => product.Id == first.Id && product.Stock == 8);
        Assert.Contains(products, product => product.Id == second.Id && product.Stock == 5);
    }

    [Fact]
    public async Task WithdrawMany_WhenOneItemFails_ShouldRollbackAllItems()
    {
        var first = await CreateProductAsync(10);
        var second = await CreateProductAsync(2);

        var response = await WithdrawAsync(
            $"rollback-{Guid.NewGuid():N}",
            new StockWithdrawalItemRequest(first.Id, 3),
            new StockWithdrawalItemRequest(second.Id, 3));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var firstAfterFailure = await GetProductAsync(first.Id);
        Assert.Equal(10, firstAfterFailure.Stock);
    }

    [Fact]
    public async Task WithdrawMany_WithDuplicateProduct_ShouldReturnBadRequest()
    {
        var product = await CreateProductAsync(10);

        var response = await WithdrawAsync(
            $"duplicate-{Guid.NewGuid():N}",
            new StockWithdrawalItemRequest(product.Id, 2),
            new StockWithdrawalItemRequest(product.Id, 3));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WithdrawMany_WithSameKey_ShouldReturnPreviousResultWithoutDebitingAgain()
    {
        var product = await CreateProductAsync(10);
        var key = $"idempotent-{Guid.NewGuid():N}";

        var firstResponse = await WithdrawAsync(key, new StockWithdrawalItemRequest(product.Id, 4));
        var secondResponse = await WithdrawAsync(key, new StockWithdrawalItemRequest(product.Id, 4));

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var firstResult = await firstResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();

        Assert.Equal(firstResult, secondResult);
        Assert.Equal(6, (await GetProductAsync(product.Id)).Stock);
    }

    [Fact]
    public async Task WithdrawMany_WithSameKeyAndDifferentRequest_ShouldReturnConflict()
    {
        var product = await CreateProductAsync(10);
        var key = $"different-request-{Guid.NewGuid():N}";

        var firstResponse = await WithdrawAsync(
            key,
            new StockWithdrawalItemRequest(product.Id, 2));

        var secondResponse = await WithdrawAsync(
            key,
            new StockWithdrawalItemRequest(product.Id, 3));

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal(8, (await GetProductAsync(product.Id)).Stock);
    }

    [Fact]
    public async Task ConcurrentWithdrawals_ShouldNotProduceNegativeStock()
    {
        var product = await CreateProductAsync(10);

        var tasks = new[]
        {
            WithdrawAsync($"concurrent-a-{Guid.NewGuid():N}", new StockWithdrawalItemRequest(product.Id, 7)),
            WithdrawAsync($"concurrent-b-{Guid.NewGuid():N}", new StockWithdrawalItemRequest(product.Id, 7))
        };

        var responses = await Task.WhenAll(tasks);

        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.Equal(3, (await GetProductAsync(product.Id)).Stock);
    }

    [Fact]
    public async Task ConcurrentRequestsWithSameKey_ShouldReturnSameResult()
    {
        var product = await CreateProductAsync(10);
        var key = $"same-key-{Guid.NewGuid():N}";

        var responses = await Task.WhenAll(
            WithdrawAsync(key, new StockWithdrawalItemRequest(product.Id, 4)),
            WithdrawAsync(key, new StockWithdrawalItemRequest(product.Id, 4)));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        Assert.Equal(6, (await GetProductAsync(product.Id)).Stock);
    }

    private async Task<ProductResponse> CreateProductAsync(int initialStock)
    {
        var response = await client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest(
                $"BATCH-{Guid.NewGuid():N}",
                "Produto de teste em lote",
                initialStock));

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private async Task<HttpResponseMessage> WithdrawAsync(
        string key,
        params StockWithdrawalItemRequest[] items)
    {
        return await client.PostAsJsonAsync(
            "/api/stock/withdraw",
            new StockWithdrawalRequest(key, items));
    }

    private async Task<ProductResponse> GetProductAsync(Guid id)
    {
        var response = await client.GetAsync($"/api/products/{id}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }
}
