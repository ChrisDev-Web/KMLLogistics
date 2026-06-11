using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.ProductoProveedores;
using E1___Sosa_Morales.Services.Productos;
using E1___Sosa_Morales.Services.Proveedores;
using E1___Sosa_Morales.Services.ProductoProveedores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.ProductoProveedores;

[Authorize]
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
        return View(new ProductoProveedoresViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "ProductoProveedores", "productos"),
            Productos = (await _prodService.ListActiveAsync(null, null, null, 1, 50)).Items,
            Proveedores = (await _provService.ListActiveAsync(null, null, null, 1, 1000)).Items
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idProduct, int? idSupplier, int page = 1, int pageSize = 10)
    {
        return Json(await _prpService.ListAsync(search, idProduct, idSupplier, page, pageSize));
    }

    [HttpGet]
    public async Task<IActionResult> ProductFilters()
        => Json(new { items = await _prpService.GetProductFilterOptionsAsync() });

    [HttpGet]
    public async Task<IActionResult> SupplierFilters()
        => Json(new { items = await _prpService.GetSupplierFilterOptionsAsync() });

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
