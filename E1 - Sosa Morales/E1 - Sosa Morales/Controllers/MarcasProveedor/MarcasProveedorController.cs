using Microsoft.AspNetCore.Mvc;
using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.MarcasProveedor;
using E1___Sosa_Morales.Services.MarcasProveedor;
using E1___Sosa_Morales.Services.Proveedores;
using E1___Sosa_Morales.Services.Marcas;
using Microsoft.AspNetCore.Authorization;

namespace E1___Sosa_Morales.Controllers.MarcasProveedor;

[Authorize]
public class MarcasProveedorController : Controller
{
    private readonly ISbrService _sbrService;
    private readonly ISupplierService _supService;
    private readonly IMarcaService _marcaService;

    public MarcasProveedorController(ISbrService sbrS, ISupplierService supS, IMarcaService marcaS)
    {
        _sbrService = sbrS; _supService = supS; _marcaService = marcaS;
    }

    public async Task<IActionResult> Index() => View(new MarcasProveedorViewModel
    {
        Module = ModuleRegistry.BuildModuleView("Catalogo", "MarcasProveedor", "productos"),
        Proveedores = await _supService.ListActiveForSelectAsync(),
        Marcas = (await _marcaService.ListActiveAsync(null, 1, 50)).Items
    });

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idBrand, int? idSupplier, int page = 1, int pageSize = 10)
    {
        return Json(await _sbrService.ListAsync(search, idBrand, idSupplier, page, pageSize));
    }

    [HttpGet]
    public async Task<IActionResult> BrandFilters()
        => Json(new { items = await _sbrService.GetBrandFilterOptionsAsync() });

    [HttpGet]
    public async Task<IActionResult> SupplierFilters()
        => Json(new { items = await _sbrService.GetSupplierFilterOptionsAsync() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int idSupplier, int idBrand)
    {
        var (success, message) = await _sbrService.CreateAsync(idSupplier, idBrand);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int idSupplier, int idBrand)
    {
        var (success, message) = await _sbrService.DeleteAsync(idSupplier, idBrand);
        return Json(new { success, message });
    }
}
