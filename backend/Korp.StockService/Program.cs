using Korp.StockService.Data;
using Korp.StockService.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var stockDatabaseConnection = builder.Configuration
    .GetConnectionString("StockDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'StockDatabase' was not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(stockDatabaseConnection));
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
