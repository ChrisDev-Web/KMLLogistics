using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.OrdenesCompra;
using E1___Sosa_Morales.Services.OrdenesCompra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.OrdenesCompra;

[Authorize]
public class OrdenesCompraController : Controller
{
    private readonly IPurchaseService _service;

    public OrdenesCompraController(IPurchaseService service) => _service = service;

    public IActionResult Index()
    {
        return View(new OrdenesCompraViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Compras", "OrdenesCompra", "compras")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idPurchase, int? idSupplier, int? idEmployee, int? idPurchaseStatus, int page = 1, int pageSize = 10)
        => Json(await _service.ListAsync(search, idPurchase, idSupplier, idEmployee, idPurchaseStatus, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
            var lines = await _service.GetLinesByPurchaseIdAsync(id);
            var warehouseLines = await _service.GetWarehouseLinesByPurchaseIdAsync(id);
            return Json(new
            {
                success = true,
                data = new
                {
                    id = item.IdPurchase,
                    idSupplier = item.IdSupplier,
                    supplierName = item.SupplierName,
                    statusPurchaseName = item.PurchaseStatusName,
                    fecPurchase = item.FecPurchase.ToString("dd/MM/yyyy HH:mm"),
                    employeeName = item.EmployeeName,
                    employeeUsername = item.EmployeeUsername,
                    subtotal = item.Subtotal,
                    tax = item.Tax,
                    total = item.Total,
                    createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    lines = lines.Select(l => new
                    {
                        id = l.IdPurchaseDetail,
                        productName = l.ProductName,
                        quantity = l.Quantity,
                        unitCost = l.UnitCost,
                        subtotal = l.Subtotal
                    }),
                    warehouseLines = warehouseLines.Select(w => new
                    {
                        id = w.IdPurchaseWarehouseDetail,
                        idPurchaseDetail = w.IdPurchaseDetail,
                        productName = w.ProductName,
                        warehouseName = w.WarehouseName,
                        quantity = w.Quantity
                    })
                }
            });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> FilterOptions()
    {
        var suppliers = await _service.GetSupplierOptionsAsync();
        var employees = await _service.GetEmployeeOptionsAsync();
        var statuses = await _service.GetStatusOptionsAsync();
        var warehouses = await _service.GetWarehouseOptionsAsync();
        return Json(new
        {
            success = true,
            suppliers = suppliers.Select(s => new { id = s.IdSupplier, name = s.Name }),
            employees = employees.Select(e => new { id = e.IdEmployee, name = e.Name }),
            statuses = statuses.Select(s => new { id = s.IdPurchaseStatus, name = s.Name }),
            warehouses = warehouses.Select(w => new { id = w.IdWarehouse, name = w.Name })
        });
    }

    [HttpGet]
    public async Task<IActionResult> ProductSupplierOptions(int idSupplier)
    {
        try
        {
            var products = await _service.GetProductSuppliersBySupplierAsync(idSupplier);
            return Json(new
            {
                success = true,
                products = products.Select(p => new { id = p.IdProductSupplier, name = p.Name, supplierCost = p.SupplierCost })
            });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int idSupplier, int idEmployee, DateTime fecPurchase, string detailsJson)
    {
        try
        {
            var lines = System.Text.Json.JsonSerializer.Deserialize<List<PurchaseLineSaveModel>>(detailsJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            var (success, message, id) = await _service.CreateAsync(new PurchaseSaveModel
            {
                IdSupplier = idSupplier,
                IdEmployee = idEmployee,
                FecPurchase = fecPurchase,
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var (success, message) = await _service.CompleteAsync(id);
        return Json(new { success, message });
    }
}
