using Korp.StockService.Data;
using Korp.StockService.Repositories;
using Korp.StockService.Services;
using Korp.StockService.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var stockDatabaseConnection = builder.Configuration
    .GetConnectionString("StockDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'StockDatabase' was not configured.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(stockDatabaseConnection));
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<StockOperationRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<StockOperationsService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
