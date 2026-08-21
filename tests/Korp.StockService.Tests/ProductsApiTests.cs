using System.Net;
using System.Net.Http.Json;
using Korp.StockService.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Korp.StockService.Tests;

public sealed class ProductsApiTests : IClassFixture<ProductsApiFactory>
{
    private readonly HttpClient client;

    public ProductsApiTests(ProductsApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreatedAndPersistProduct()
    {
        var request = NewCreateRequest();

        var response = await client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(product);
        Assert.Equal(request.Code.ToUpperInvariant(), product.Code);
        Assert.Equal(request.InitialStock, product.Stock);

        var getResponse = await client.GetAsync($"/api/products/{product.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithSameCode_ShouldReturnConflict()
    {
        var request = NewCreateRequest();

        var firstResponse = await client.PostAsJsonAsync("/api/products", request);
        var secondResponse = await client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal("application/problem+json", secondResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetProduct_WithUnknownId_ShouldReturnNotFound()
    {
        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StockOut_WithEnoughStock_ShouldUpdateBalance()
    {
        var created = await CreateProductAsync(initialStock: 10);

        var response = await client.PostAsJsonAsync(
            $"/api/products/{created.Id}/stock-out",
            new StockMovementRequest(4));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(updated);
        Assert.Equal(6, updated.Stock);
    }

    [Fact]
    public async Task StockOut_WithInsufficientStock_ShouldReturnConflict()
    {
        var created = await CreateProductAsync(initialStock: 2);

        var response = await client.PostAsJsonAsync(
            $"/api/products/{created.Id}/stock-out",
            new StockMovementRequest(3));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task StockIn_WithInvalidQuantity_ShouldReturnBadRequest()
    {
        var created = await CreateProductAsync(initialStock: 2);

        var response = await client.PostAsJsonAsync(
            $"/api/products/{created.Id}/stock-in",
            new StockMovementRequest(0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<ProductResponse> CreateProductAsync(int initialStock)
    {
        var response = await client.PostAsJsonAsync(
            "/api/products",
            NewCreateRequest(initialStock));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private static CreateProductRequest NewCreateRequest(int initialStock = 10)
    {
        return new CreateProductRequest(
            $"TEST-{Guid.NewGuid():N}",
            "Produto de teste",
            initialStock);
    }
}

public sealed class ProductsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
