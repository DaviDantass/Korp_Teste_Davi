using Korp.BillingService.Data;
using Microsoft.EntityFrameworkCore;
using Korp.BillingService.Repositories;
using Korp.BillingService.Services;
using Korp.BillingService.Middleware;
using Korp.BillingService.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<InvoiceRepository>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddHttpClient<StockServiceClient>(client =>
{
    var stockServiceUrl = builder.Configuration["StockService:BaseUrl"]
        ?? throw new InvalidOperationException(
            "A URL do StockService não foi configurada.");

    client.BaseAddress = new Uri(stockServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var billingDatabaseConnection = builder.Configuration
    .GetConnectionString("BillingDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'BillingDatabase' was not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(billingDatabaseConnection));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

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
