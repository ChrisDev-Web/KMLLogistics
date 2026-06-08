using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleTransferencia;
using E1___Sosa_Morales.Services.DetalleTransferencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleTransferencia;

[Authorize]
public class DetalleTransferenciaController : Controller
{
    private readonly ITransferDetailService _service;

    public DetalleTransferenciaController(ITransferDetailService service) => _service = service;

    public IActionResult Index()
    {
        return View(new DetalleTransferenciaViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Transferencias", "DetalleTransferencia", "transferencias")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idTransfer, int? idProduct, int? idWarehouseOrigin, int? idWarehouseDestination, int? idStatusTransfer, int page = 1, int pageSize = 10)
        => Json(await _service.ListAsync(search, idTransfer, idProduct, idWarehouseOrigin, idWarehouseDestination, idStatusTransfer, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
            return Json(new
            {
                success = true,
                data = new
                {
                    id = item.IdTransferDetail,
                    idTransfer = item.IdTransfer,
                    productName = item.ProductName,
                    quantity = item.Quantity,
                    warehouseOriginName = item.WarehouseOriginName,
                    warehouseDestinationName = item.WarehouseDestinationName,
                    statusTransferName = item.StatusTransferName,
                    fecTransfer = item.FecTransfer.ToString("dd/MM/yyyy HH:mm"),
                    employeeName = item.EmployeeName,
                    employeeUsername = item.EmployeeUsername,
                    transferCreatedAt = item.TransferCreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
                }
            });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> FilterOptions()
    {
        var products = await _service.GetProductFilterOptionsAsync();
        var warehouses = await _service.GetWarehouseFilterOptionsAsync();
        var statuses = await _service.GetStatusFilterOptionsAsync();
        return Json(new
        {
            success = true,
            products = products.Select(p => new { id = p.IdProduct, name = p.Name }),
            warehouses = warehouses.Select(w => new { id = w.IdWarehouse, name = w.Name }),
            statuses = statuses.Select(s => new { id = s.IdStatusTransfer, name = s.Name })
        });
    }
}
