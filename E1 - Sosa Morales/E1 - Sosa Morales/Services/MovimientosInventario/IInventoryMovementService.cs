using E1___Sosa_Morales.Models.MovimientosInventario;

namespace E1___Sosa_Morales.Services.MovimientosInventario;

public interface IInventoryMovementService
{
    Task<InventoryMovementPagedResult> ListAsync(string? search, int? idWarehouse, int? idProduct, int? idMovementType, string? movementDirection, int page, int pageSize);
    Task<InventoryMovementDetail?> GetByIdAsync(int id);
    Task<List<InventoryMovementFilterOption>> GetWarehouseOptionsAsync();
    Task<List<InventoryMovementFilterOption>> GetProductOptionsAsync();
    Task<List<InventoryMovementFilterOption>> GetMovementTypeOptionsAsync();
}
