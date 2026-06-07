using E1___Sosa_Morales.Models.AlertasStock;
using E1___Sosa_Morales.Models.Roles;
using E1___Sosa_Morales.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasNoKey();
        modelBuilder.Entity<Role>().HasNoKey();
        modelBuilder.Entity<SpResult>().HasNoKey();
        modelBuilder.Entity<StockAlertItem>().HasNoKey();
        modelBuilder.Entity<StockAlertCountResult>().HasNoKey();
        modelBuilder.Entity<StockAlertFilterOption>().HasNoKey();
    }
}
