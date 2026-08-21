using Korp.BillingService.Data;
using Microsoft.EntityFrameworkCore;
using Korp.BillingService.Repositories;
using Korp.BillingService.Services;
using Korp.BillingService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<InvoiceRepository>();
builder.Services.AddScoped<InvoiceService>();

var billingDatabaseConnection = builder.Configuration
    .GetConnectionString("BillingDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'BillingDatabase' was not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(billingDatabaseConnection));

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
