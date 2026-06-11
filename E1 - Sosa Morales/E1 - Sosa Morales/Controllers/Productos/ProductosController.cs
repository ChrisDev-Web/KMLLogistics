using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Productos;
using E1___Sosa_Morales.Services.Categorias;
using E1___Sosa_Morales.Services.Marcas;
using E1___Sosa_Morales.Services.Productos;
using E1___Sosa_Morales.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Productos;

[Authorize]
public class ProductosController : Controller
{
    private readonly IProductoService _service;
    private readonly ICategoriaService _categoriaService;
    private readonly IMarcaService _marcaService;
    private readonly IWebHostEnvironment _env;

    public ProductosController(IProductoService service, ICategoriaService categoriaService, IMarcaService marcaService, IWebHostEnvironment env)
    {
        _service = service;
        _categoriaService = categoriaService;
        _marcaService = marcaService;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        return View(new ProductosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "Productos", "productos"),
            Categorias = (await _categoriaService.ListActiveAsync(null, 1, 50)).Items,
            Marcas = (await _marcaService.ListActiveAsync(null, 1, 50)).Items
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idCategory, int? idBrand, int page = 1, int pageSize = 10)
    {
        try { return Json(await _service.ListActiveAsync(search, idCategory, idBrand, page, pageSize)); }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> ListInactive(string? search, int? idCategory, int? idBrand, int page = 1, int pageSize = 10)
    {
        try { return Json(await _service.ListInactiveAsync(search, idCategory, idBrand, page, pageSize)); }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> CategoryFilters()
        => Json(new { items = await _service.GetCategoryFilterOptionsAsync() });

    [HttpGet]
    public async Task<IActionResult> BrandFilters()
        => Json(new { items = await _service.GetBrandFilterOptionsAsync() });

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return Json(new { success = false, message = "Producto no encontrado." });
        return Json(new { success = true, data = item });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ProductoDetail dto, IFormFile? photo, bool removePhoto = false)
    {
        string? photoPath = null;
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.IdCategory <= 0 || dto.IdBrand <= 0 || dto.Cost < 0 || dto.ProfitPercentage <= 0)
                return Json(new { success = false, message = "Complete los campos obligatorios correctamente." });

            ProductoDetail? current = null;
            if (dto.IdProduct > 0)
            {
                current = await _service.GetByIdAsync(dto.IdProduct);
                if (current is null) return Json(new { success = false, message = "Producto no encontrado." });
            }

            var savedPhoto = await CatalogPhotoStorage.SaveAsync(photo, _env, "Products", dto.IdProduct > 0 ? $"product_{dto.IdProduct}" : "product");
            if (!savedPhoto.Success) return Json(new { success = false, message = savedPhoto.Message });
            photoPath = savedPhoto.WebPath;
            dto.Photo = photoPath;

            if (dto.IdProduct == 0)
            {
                var (success, message, id) = await _service.CreateAsync(dto);
                if (!success) CatalogPhotoStorage.Delete(_env, photoPath);
                return Json(new { success, message, id });
            }
            else
            {
                var shouldRemovePhoto = removePhoto && string.IsNullOrWhiteSpace(photoPath);
                var (success, message) = await _service.UpdateAsync(dto, shouldRemovePhoto);
                if (success && current is not null && (!string.IsNullOrWhiteSpace(photoPath) || shouldRemovePhoto))
                    CatalogPhotoStorage.Delete(_env, current.Photo);
                if (!success) CatalogPhotoStorage.Delete(_env, photoPath);
                return Json(new { success, message });
            }
        }
        catch (Exception ex)
        {
            CatalogPhotoStorage.Delete(_env, photoPath);
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
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
