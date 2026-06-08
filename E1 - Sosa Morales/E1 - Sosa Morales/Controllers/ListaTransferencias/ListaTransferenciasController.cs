using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.ListaTransferencias;
using E1___Sosa_Morales.Services.ListaTransferencias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.ListaTransferencias;

[Authorize]
public class ListaTransferenciasController : Controller
{
    private readonly ITransferService _service;

    public ListaTransferenciasController(ITransferService service) => _service = service;

    public IActionResult Index()
    {
        return View(new ListaTransferenciasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Transferencias", "ListaTransferencias", "transferencias")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idWarehouseOrigin, int? idWarehouseDestination, int? idStatusTransfer, int? idEmployee, int page = 1, int pageSize = 10)
        => Json(await _service.ListAsync(search, idWarehouseOrigin, idWarehouseDestination, idStatusTransfer, idEmployee, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
            var lines = await _service.GetLinesByTransferIdAsync(id);
            return Json(new
            {
                success = true,
                data = new
                {
                    id = item.IdTransfer,
                    idWarehouseOrigin = item.IdWarehouseOrigin,
                    warehouseOriginName = item.WarehouseOriginName,
                    idWarehouseDestination = item.IdWarehouseDestination,
                    warehouseDestinationName = item.WarehouseDestinationName,
                    statusTransferName = item.StatusTransferName,
                    fecTransfer = item.FecTransfer.ToString("dd/MM/yyyy HH:mm"),
                    employeeName = item.EmployeeName,
                    employeeUsername = item.EmployeeUsername,
                    createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    lines = lines.Select(l => new { id = l.IdTransferDetail, productName = l.ProductName, quantity = l.Quantity })
                }
            });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> FilterOptions()
    {
        var warehouses = await _service.GetWarehouseOptionsAsync();
        var employees = await _service.GetEmployeeOptionsAsync();
        var statuses = await _service.GetStatusOptionsAsync();
        return Json(new
        {
            success = true,
            warehouses = warehouses.Select(w => new { id = w.IdWarehouse, name = w.Name }),
            employees = employees.Select(e => new { id = e.IdEmployee, name = e.Name }),
            statuses = statuses.Select(s => new { id = s.IdStatusTransfer, name = s.Name })
        });
    }

    [HttpGet]
    public async Task<IActionResult> ProductOptions(int idWarehouse)
    {
        var products = await _service.GetProductsByWarehouseAsync(idWarehouse);
        return Json(new
        {
            success = true,
            products = products.Select(p => new { id = p.IdProduct, name = p.Name, stock = p.Stock })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int idWarehouseOrigin, int idWarehouseDestination, int idEmployee, DateTime fecTransfer, string detailsJson)
    {
        try
        {
            var lines = System.Text.Json.JsonSerializer.Deserialize<List<TransferLineSaveModel>>(detailsJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            var (success, message, id) = await _service.CreateAsync(new TransferSaveModel
            {
                IdWarehouseOrigin = idWarehouseOrigin,
                IdWarehouseDestination = idWarehouseDestination,
                IdEmployee = idEmployee,
                FecTransfer = fecTransfer,
                Lines = lines
            });
            return Json(new { success, message, id });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, message) = await _service.CancelAsync(id);
        return Json(new { success, message });
    }
}
