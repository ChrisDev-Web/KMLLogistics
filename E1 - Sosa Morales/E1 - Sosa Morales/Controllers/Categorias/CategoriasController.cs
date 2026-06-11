using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Categorias;
using E1___Sosa_Morales.Services.Categorias;
using E1___Sosa_Morales.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Categorias;

[Authorize]
public class CategoriasController : Controller
{
    private readonly ICategoriaService _service;
    private readonly IWebHostEnvironment _env;

    public CategoriasController(ICategoriaService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    public IActionResult Index()
    {
        return View(new CategoriasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "Categorias", "productos")
        });
    }


    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10)
    {
        try
        {
            return Json(await _service.ListActiveAsync(search, page, pageSize));
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error interno: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return Json(new { success = false, message = "Categoría no encontrada." });

        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdCategory,
                name = item.Name,
                description = item.Description,
                photo = item.Photo
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, IFormFile? photo)
    {
        string? photoPath = null;
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "El nombre de la categoría es obligatorio." });

            var savedPhoto = await CatalogPhotoStorage.SaveAsync(photo, _env, "Categories", "category");
            if (!savedPhoto.Success) return Json(new { success = false, message = savedPhoto.Message });
            photoPath = savedPhoto.WebPath;

            var (success, message, id) = await _service.CreateAsync(name.Trim(), description?.Trim(), photoPath);
            if (!success) CatalogPhotoStorage.Delete(_env, photoPath);
            return Json(new { success, message, id });
        }
        catch (Exception ex)
        {
            CatalogPhotoStorage.Delete(_env, photoPath);
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name, string? description, IFormFile? photo, bool removePhoto = false)
    {
        string? photoPath = null;
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "El nombre de la categoría es obligatorio." });

            var current = await _service.GetByIdAsync(id);
            if (current is null) return Json(new { success = false, message = "Categoría no encontrada." });

            var savedPhoto = await CatalogPhotoStorage.SaveAsync(photo, _env, "Categories", $"category_{id}");
            if (!savedPhoto.Success) return Json(new { success = false, message = savedPhoto.Message });
            photoPath = savedPhoto.WebPath;

            var shouldRemovePhoto = removePhoto && string.IsNullOrWhiteSpace(photoPath);
            var (success, message) = await _service.UpdateAsync(id, name.Trim(), description?.Trim(), photoPath, shouldRemovePhoto);
            if (success && (!string.IsNullOrWhiteSpace(photoPath) || shouldRemovePhoto))
                CatalogPhotoStorage.Delete(_env, current.Photo);
            if (!success) CatalogPhotoStorage.Delete(_env, photoPath);
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            CatalogPhotoStorage.Delete(_env, photoPath);
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhysical(int id)
    {
        var (success, message) = await _service.DeletePhysicalAsync(id);
        return Json(new { success, message });
    }


    [HttpGet]
    public async Task<IActionResult> ListInactive(string? search, int page = 1, int pageSize = 10)
    {
        try
        {
            return Json(await _service.ListInactiveAsync(search, page, pageSize));
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error interno: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogic(int id)
    {
        try
        {
            var (success, message) = await _service.DeleteLogicAsync(id);
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error interno: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        try
        {
            var (success, message) = await _service.RestoreAsync(id);
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error interno: " + ex.Message });
        }
    }
}
