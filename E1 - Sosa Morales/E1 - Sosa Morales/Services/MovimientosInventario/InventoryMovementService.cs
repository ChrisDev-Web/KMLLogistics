using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.MovimientosInventario;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.MovimientosInventario;

public class InventoryMovementService : IInventoryMovementService
{
    private readonly ApplicationDbContext _context;

    public InventoryMovementService(ApplicationDbContext context) => _context = context;

    public async Task<InventoryMovementPagedResult> ListAsync(string? search, int? idWarehouse, int? idProduct, int? idMovementType, string? movementDirection, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<InventoryMovementListItem>(
            "EXEC dbo.sp_inventory_movement_list @search, @id_warehouse, @id_product, @id_movement_type, @movement_direction, @page, @page_size",
            Param("@search", search),
            Param("@id_warehouse", idWarehouse),
            Param("@id_product", idProduct),
            Param("@id_movement_type", idMovementType),
            Param("@movement_direction", movementDirection),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new InventoryMovementPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdInventoryMovement,
                productName = r.ProductName,
                warehouseName = r.WarehouseName,
                movementTypeName = r.MovementTypeName,
                movementDirection = r.MovementDirection,
                quantity = r.Quantity,
                reference = r.Reference ?? "",
                fecMovement = r.FecMovement.ToString("dd/MM/yyyy HH:mm"),
                employeeName = r.EmployeeName
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<InventoryMovementDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<InventoryMovementDetail>(
            "EXEC dbo.sp_inventory_movement_get_by_id @id_inventory_movement",
            new SqlParameter("@id_inventory_movement", id)).ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<InventoryMovementFilterOption>> GetWarehouseOptionsAsync()
        => await _context.Database.SqlQueryRaw<InventoryMovementFilterOption>(
            "SELECT id_warehouse AS id, name FROM Warehouses WHERE deleted_at IS NULL AND status = 1 ORDER BY name").ToListAsync();

    public async Task<List<InventoryMovementFilterOption>> GetProductOptionsAsync()
        => await _context.Database.SqlQueryRaw<InventoryMovementFilterOption>(
            "SELECT id_product AS id, name FROM Products WHERE deleted_at IS NULL AND status = 1 ORDER BY name").ToListAsync();

    public async Task<List<InventoryMovementFilterOption>> GetMovementTypeOptionsAsync()
        => await _context.Database.SqlQueryRaw<InventoryMovementFilterOption>(
            "SELECT id_movement_type AS id, name FROM MovementTypes WHERE deleted_at IS NULL AND status = 1 ORDER BY name").ToListAsync();

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
