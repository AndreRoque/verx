using CashFlow.Consolidation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Data;

public class ConsolidationDbContext(DbContextOptions<ConsolidationDbContext> options) : DbContext(options)
{
    public DbSet<DailyConsolidation> DailyConsolidations => Set<DailyConsolidation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyConsolidation>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasIndex(d => d.Date).IsUnique();
            e.Property(d => d.TotalCredits).HasPrecision(18, 2);
            e.Property(d => d.TotalDebits).HasPrecision(18, 2);
        });
    }
}
