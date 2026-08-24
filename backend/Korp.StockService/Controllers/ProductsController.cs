using Korp.StockService.DTOs;
using Korp.StockService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Korp.StockService.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductService productService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var response = await productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedProductsResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        Ok(await productService.ListAsync(page, pageSize, search, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await productService.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        Ok(await productService.UpdateAsync(id, request, cancellationToken));
}
