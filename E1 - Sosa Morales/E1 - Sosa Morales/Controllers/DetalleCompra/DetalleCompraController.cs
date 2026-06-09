using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleCompra;
using E1___Sosa_Morales.Services.DetalleCompra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleCompra;

[Authorize]
public class DetalleCompraController : Controller
{
    private readonly IPurchaseDetailService _service;

    public DetalleCompraController(IPurchaseDetailService service) => _service = service;

    public IActionResult Index()
    {
        return View(new DetalleCompraViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Compras", "DetalleCompra", "compras")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idPurchase, int? idProduct, int? idSupplier, int? idPurchaseStatus, int page = 1, int pageSize = 10)
        => Json(await _service.ListAsync(search, idPurchase, idProduct, idSupplier, idPurchaseStatus, page, pageSize));

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
                    id = item.IdPurchaseDetail,
                    idPurchase = item.IdPurchase,
                    productName = item.ProductName,
                    quantity = item.Quantity,
                    unitCost = item.UnitCost,
                    subtotal = item.Subtotal,
                    supplierName = item.SupplierName,
                    purchaseStatusName = item.PurchaseStatusName,
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
        var suppliers = await _service.GetSupplierFilterOptionsAsync();
        var statuses = await _service.GetStatusFilterOptionsAsync();
        return Json(new
        {
            success = true,
            products = products.Select(p => new { id = p.IdProduct, name = p.Name }),
            suppliers = suppliers.Select(s => new { id = s.IdSupplier, name = s.Name }),
            statuses = statuses.Select(s => new { id = s.IdPurchaseStatus, name = s.Name })
        });
    }
}
