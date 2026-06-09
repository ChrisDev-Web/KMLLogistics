using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleAlmacenCompra;
using E1___Sosa_Morales.Services.DetalleAlmacenCompra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleAlmacenCompra;

[Authorize]
public class DetalleAlmacenCompraController : Controller
{
    private readonly IPurchaseWarehouseDetailService _service;

    public DetalleAlmacenCompraController(IPurchaseWarehouseDetailService service) => _service = service;

    public IActionResult Index()
    {
        return View(new DetalleAlmacenCompraViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Compras", "DetalleAlmacenCompra", "compras")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idPurchase, int? idProduct, int? idWarehouse, int? idSupplier, int page = 1, int pageSize = 10)
    {
        try
        {
            return Json(await _service.ListAsync(search, idPurchase, idProduct, idWarehouse, idSupplier, page, pageSize));
        }
        catch (Exception ex)
        {
            return Json(new { items = Array.Empty<object>(), totalCount = 0, page, pageSize, totalPages = 0, message = "Error: " + ex.Message });
        }
    }

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
                    id = item.IdPurchaseWarehouseDetail,
                    idPurchaseDetail = item.IdPurchaseDetail,
                    idPurchase = item.IdPurchase,
                    productName = item.ProductName,
                    warehouseName = item.WarehouseName,
                    quantity = item.Quantity,
                    supplierName = item.SupplierName,
                    fecPurchase = item.FecPurchase.ToString("dd/MM/yyyy HH:mm"),
                    employeeName = item.EmployeeName,
                    employeeUsername = item.EmployeeUsername,
                    purchaseCreatedAt = item.PurchaseCreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
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
        var suppliers = await _service.GetSupplierFilterOptionsAsync();
        return Json(new
        {
            success = true,
            products = products.Select(p => new { id = p.IdProduct, name = p.Name }),
            warehouses = warehouses.Select(w => new { id = w.IdWarehouse, name = w.Name }),
            suppliers = suppliers.Select(s => new { id = s.IdSupplier, name = s.Name })
        });
    }
}
