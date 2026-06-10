using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Productos;
using E1___Sosa_Morales.Services.Categorias;
using E1___Sosa_Morales.Services.Marcas;
using E1___Sosa_Morales.Services.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Productos;

[Authorize]
public class ProductosController : Controller
{
    private readonly IProductoService _service;
    private readonly ICategoriaService _categoriaService;
    private readonly IMarcaService _marcaService;

    public ProductosController(IProductoService service, ICategoriaService categoriaService, IMarcaService marcaService)
    {
        _service = service;
        _categoriaService = categoriaService;
        _marcaService = marcaService;
    }

    public async Task<IActionResult> Index()
    {
        return View(new ProductosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "Productos", "productos"),
            Categorias = await _categoriaService.ListActiveAsync(null),
            Marcas = await _marcaService.ListActiveAsync(null)
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10)
    {
        try { var items = await _service.ListActiveAsync(search); return Json(new { items, totalCount = items.Count, page, pageSize, totalPages = 1 }); }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> ListInactive(string? search, int page = 1, int pageSize = 10)
    {
        try { var items = await _service.ListInactiveAsync(search); return Json(new { items, totalCount = items.Count, page, pageSize, totalPages = 1 }); }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return Json(new { success = false, message = "Producto no encontrado." });
        return Json(new { success = true, data = item });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ProductoDetail dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.IdCategory <= 0 || dto.IdBrand <= 0 || dto.Cost < 0 || dto.ProfitPercentage <= 0)
                return Json(new { success = false, message = "Complete los campos obligatorios correctamente." });

            if (dto.IdProduct == 0)
            {
                var (success, message, id) = await _service.CreateAsync(dto);
                return Json(new { success, message, id });
            }
            else
            {
                var (success, message) = await _service.UpdateAsync(dto);
                return Json(new { success, message });
            }
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogic(int id)
    {
        var (success, message) = await _service.DeleteLogicAsync(id); return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var (success, message) = await _service.RestoreAsync(id); return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhysical(int id)
    {
        var (success, message) = await _service.DeletePhysicalAsync(id); return Json(new { success, message });
    }
}