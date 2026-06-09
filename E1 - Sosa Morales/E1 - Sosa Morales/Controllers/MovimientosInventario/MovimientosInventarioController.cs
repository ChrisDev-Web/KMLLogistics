using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.MovimientosInventario;
using E1___Sosa_Morales.Services.MovimientosInventario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.MovimientosInventario;

[Authorize]
public class MovimientosInventarioController : Controller
{
    private readonly IInventoryMovementService _service;

    public MovimientosInventarioController(IInventoryMovementService service) => _service = service;

    public IActionResult Index()
        => View(new MovimientosInventarioViewModel { Module = ModuleRegistry.BuildModuleView("Inventario", "MovimientosInventario", "inventario") });

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idWarehouse, int? idProduct, int? idMovementType, string? movementDirection, int page = 1, int pageSize = 10)
        => Json(await _service.ListAsync(search, idWarehouse, idProduct, idMovementType, movementDirection, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdInventoryMovement,
                productName = item.ProductName,
                warehouseName = item.WarehouseName,
                movementTypeName = item.MovementTypeName,
                movementDirection = item.MovementDirection,
                quantity = item.Quantity,
                reference = item.Reference ?? "",
                fecMovement = item.FecMovement.ToString("dd/MM/yyyy HH:mm"),
                employeeName = item.EmployeeName,
                employeeUsername = item.EmployeeUsername,
                createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> FilterOptions()
        => Json(new
        {
            success = true,
            warehouses = await _service.GetWarehouseOptionsAsync(),
            products = await _service.GetProductOptionsAsync(),
            movementTypes = await _service.GetMovementTypeOptionsAsync()
        });
}
