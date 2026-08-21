using Korp.BillingService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var billingDatabaseConnection = builder.Configuration
    .GetConnectionString("BillingDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'BillingDatabase' was not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(billingDatabaseConnection));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
