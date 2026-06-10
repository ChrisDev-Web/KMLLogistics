using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.ProductoProveedores;
using E1___Sosa_Morales.Services.Productos;
using E1___Sosa_Morales.Services.Proveedores;
using E1___Sosa_Morales.Services.ProductoProveedores;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.ProductoProveedores;

public class ProductoProveedoresController : Controller
{
    private readonly IPrpService _prpService;
    private readonly IProductoService _prodService;
    private readonly ISupplierService _provService;

    public ProductoProveedoresController(IPrpService prpS, IProductoService prodS, ISupplierService provS)
    {
        _prpService = prpS; _prodService = prodS; _provService = provS;
    }

    public async Task<IActionResult> Index()
    {
        // Cambia PrpViewModel por ProductoProveedoresViewModel
        return View(new ProductoProveedoresViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logística", "Producto proveedores", "logistica"),
            Productos = await _prodService.ListActiveAsync(null),
            // Si el resultado de proveedores tiene una propiedad .Items, úsala. Si no, usa el resultado directo.
            Proveedores = (await _provService.ListActiveAsync(null, null, null, 1, 1000)).Items
        });
    }

    [HttpGet]
    public async Task<IActionResult> List() => Json(new { items = await _prpService.ListAsync(null) });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int idProduct, int idSupplier, decimal cost, bool isMain)
    {
        var (success, message) = await _prpService.CreateAsync(idProduct, idSupplier, cost, isMain);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _prpService.DeleteAsync(id);
        return Json(new { success, message });
    }
}