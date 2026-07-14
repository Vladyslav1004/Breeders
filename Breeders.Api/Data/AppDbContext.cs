using Breeders.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Breeders.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
    }

    public DbSet<Litter> Litters { get; set; }

    public DbSet<BreederBenefit> BreederBenefits { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BreederBenefit>().HasKey(benefit => benefit.BreederId);
    }
}