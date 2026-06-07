using E1___Sosa_Morales.Models.AlertasStock;
using E1___Sosa_Morales.Models.Countries;
using E1___Sosa_Morales.Models.Distritos;
using E1___Sosa_Morales.Models.Provincias;
using E1___Sosa_Morales.Models.Regiones;
using E1___Sosa_Morales.Models.Roles;
using E1___Sosa_Morales.Models.TiposDocumento;
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
        modelBuilder.Entity<DocumentTypeListItem>().HasNoKey();
        modelBuilder.Entity<DocumentTypeDetail>().HasNoKey();
        modelBuilder.Entity<DocumentTypeSpResult>().HasNoKey();
        modelBuilder.Entity<CountryListItem>().HasNoKey();
        modelBuilder.Entity<CountryDetail>().HasNoKey();
        modelBuilder.Entity<CountrySpResult>().HasNoKey();
        modelBuilder.Entity<RegionListItem>().HasNoKey();
        modelBuilder.Entity<RegionDetail>().HasNoKey();
        modelBuilder.Entity<RegionFkOption>().HasNoKey();
        modelBuilder.Entity<RegionSpResult>().HasNoKey();
        modelBuilder.Entity<ProvinceListItem>().HasNoKey();
        modelBuilder.Entity<ProvinceDetail>().HasNoKey();
        modelBuilder.Entity<ProvinceFkOption>().HasNoKey();
        modelBuilder.Entity<ProvinceSpResult>().HasNoKey();
        modelBuilder.Entity<DistrictListItem>().HasNoKey();
        modelBuilder.Entity<DistrictDetail>().HasNoKey();
        modelBuilder.Entity<DistrictFkOption>().HasNoKey();
        modelBuilder.Entity<DistrictSpResult>().HasNoKey();
    }
}
