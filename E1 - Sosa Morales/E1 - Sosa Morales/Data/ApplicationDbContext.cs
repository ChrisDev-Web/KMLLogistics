using E1___Sosa_Morales.Models.AlertasStock;
using E1___Sosa_Morales.Models.Almacenes;
using E1___Sosa_Morales.Models.DetalleAlmacen;
using E1___Sosa_Morales.Models.MovimientosInventario;
using E1___Sosa_Morales.Models.TiposMovimiento;
using E1___Sosa_Morales.Models.Cargos;
using E1___Sosa_Morales.Models.Clientes;
using E1___Sosa_Morales.Models.Countries;
using E1___Sosa_Morales.Models.DetalleTransferencia;
using E1___Sosa_Morales.Models.Distritos;
using E1___Sosa_Morales.Models.Empleados;
using E1___Sosa_Morales.Models.EstadosTransferencia;
using E1___Sosa_Morales.Models.ListaTransferencias;
using E1___Sosa_Morales.Models.Perfil;
using E1___Sosa_Morales.Models.Provincias;
using E1___Sosa_Morales.Models.Proveedores;
using E1___Sosa_Morales.Models.Regiones;
using E1___Sosa_Morales.Models.Roles;
using E1___Sosa_Morales.Models.Shared;
using E1___Sosa_Morales.Models.TiposDocumento;
using E1___Sosa_Morales.Models.Users;
using E1___Sosa_Morales.Models.Usuarios;
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
        modelBuilder.Entity<RoleListItem>().HasNoKey();
        modelBuilder.Entity<RoleDetail>().HasNoKey();
        modelBuilder.Entity<RoleSpResult>().HasNoKey();
        modelBuilder.Entity<UsuarioListItem>().HasNoKey();
        modelBuilder.Entity<UsuarioDetail>().HasNoKey();
        modelBuilder.Entity<UsuarioSpResult>().HasNoKey();
        modelBuilder.Entity<UsuarioRoleOption>().HasNoKey();
        modelBuilder.Entity<UserProfileDetail>().HasNoKey();
        modelBuilder.Entity<ClientListItem>().HasNoKey();
        modelBuilder.Entity<ClientDetail>().HasNoKey();
        modelBuilder.Entity<ClientDocumentTypeOption>().HasNoKey();
        modelBuilder.Entity<ClientDistrictOption>().HasNoKey();
        modelBuilder.Entity<ClientSpResult>().HasNoKey();
        modelBuilder.Entity<SupplierListItem>().HasNoKey();
        modelBuilder.Entity<SupplierDetail>().HasNoKey();
        modelBuilder.Entity<SupplierDocumentTypeOption>().HasNoKey();
        modelBuilder.Entity<SupplierDistrictOption>().HasNoKey();
        modelBuilder.Entity<SupplierSpResult>().HasNoKey();
        modelBuilder.Entity<GeographyInfo>().HasNoKey();
        modelBuilder.Entity<JobPositionListItem>().HasNoKey();
        modelBuilder.Entity<JobPositionDetail>().HasNoKey();
        modelBuilder.Entity<JobPositionSpResult>().HasNoKey();
        modelBuilder.Entity<EmployeeListItem>().HasNoKey();
        modelBuilder.Entity<EmployeeDetailRecord>().HasNoKey();
        modelBuilder.Entity<EmployeeDocumentTypeOption>().HasNoKey();
        modelBuilder.Entity<EmployeeDistrictOption>().HasNoKey();
        modelBuilder.Entity<EmployeeJobPositionOption>().HasNoKey();
        modelBuilder.Entity<EmployeeUserOption>().HasNoKey();
        modelBuilder.Entity<EmployeeSpResult>().HasNoKey();
        modelBuilder.Entity<StatusTransferListItem>().HasNoKey();
        modelBuilder.Entity<StatusTransferDetail>().HasNoKey();
        modelBuilder.Entity<StatusTransferSpResult>().HasNoKey();
        modelBuilder.Entity<TransferListItem>().HasNoKey();
        modelBuilder.Entity<TransferDetailRecord>().HasNoKey();
        modelBuilder.Entity<TransferLineItem>().HasNoKey();
        modelBuilder.Entity<TransferOption>().HasNoKey();
        modelBuilder.Entity<TransferEmployeeOption>().HasNoKey();
        modelBuilder.Entity<TransferStatusOption>().HasNoKey();
        modelBuilder.Entity<TransferProductOption>().HasNoKey();
        modelBuilder.Entity<TransferSpResult>().HasNoKey();
        modelBuilder.Entity<TransferDetailListItem>().HasNoKey();
        modelBuilder.Entity<TransferDetailItem>().HasNoKey();
        modelBuilder.Entity<WarehouseListItem>().HasNoKey();
        modelBuilder.Entity<WarehouseDetail>().HasNoKey();
        modelBuilder.Entity<WarehouseDistrictOption>().HasNoKey();
        modelBuilder.Entity<WarehouseSpResult>().HasNoKey();
        modelBuilder.Entity<MovementTypeListItem>().HasNoKey();
        modelBuilder.Entity<MovementTypeDetail>().HasNoKey();
        modelBuilder.Entity<MovementTypeSpResult>().HasNoKey();
        modelBuilder.Entity<WarehouseDetailMetrics>().HasNoKey();
        modelBuilder.Entity<WarehouseDetailSummaryItem>().HasNoKey();
        modelBuilder.Entity<WarehouseDetailProductItem>().HasNoKey();
        modelBuilder.Entity<WarehouseDetailHeader>().HasNoKey();
        modelBuilder.Entity<WarehouseDetailRecord>().HasNoKey();
        modelBuilder.Entity<WarehouseDetailOption>().HasNoKey();
        modelBuilder.Entity<InventoryMovementListItem>().HasNoKey();
        modelBuilder.Entity<InventoryMovementDetail>().HasNoKey();
        modelBuilder.Entity<InventoryMovementFilterOption>().HasNoKey();
    }
}
