using Microsoft.AspNetCore.Mvc;
using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.MarcasProveedor;
using E1___Sosa_Morales.Services.MarcasProveedor;
using E1___Sosa_Morales.Services.Proveedores;
using E1___Sosa_Morales.Services.Marcas;

namespace E1___Sosa_Morales.Controllers.MarcasProveedor;

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
        Module = ModuleRegistry.BuildModuleView("Logística", "Marcas Proveedor", "logistica"),
        Proveedores = (await _supService.ListActiveAsync(null, null, null, 1, 1000)).Items,
        Marcas = await _marcaService.ListActiveAsync(null)
    });

    [HttpGet] public async Task<IActionResult> List() => Json(new { items = await _sbrService.ListAsync(null) });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int idSupplier, int idBrand)
        => Json(await _sbrService.CreateAsync(idSupplier, idBrand));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int idSupplier, int idBrand)
        => Json(await _sbrService.DeleteAsync(idSupplier, idBrand));
}
