using Microsoft.EntityFrameworkCore;

namespace Korp.BillingService.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
}
