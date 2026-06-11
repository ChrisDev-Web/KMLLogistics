using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Marcas;
using E1___Sosa_Morales.Services.Marcas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Marcas;

[Authorize]
public class MarcasController : Controller
{
    private readonly IMarcaService _service;

    public MarcasController(IMarcaService service) => _service = service;

    public IActionResult Index()
    {
        return View(new MarcasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "Marcas", "productos")
        });
    }




    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10)
    {
        try
        {
            var result = await _service.ListActiveAsync(search, page, pageSize);
            return Json(MapPagedResult(result));
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
        if (item is null) return Json(new { success = false, message = "Marca no encontrada." });

        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdBrand,
                name = item.Name,
                description = item.Description ?? "",
                status = item.Status,
                createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "El nombre de la marca es obligatorio." });

            var (success, message, id) = await _service.CreateAsync(name.Trim(), description?.Trim());
            return Json(new { success, message, id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name, string? description)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "El nombre de la marca es obligatorio." });

            var (success, message) = await _service.UpdateAsync(id, name.Trim(), description?.Trim());
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
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
            var result = await _service.ListInactiveAsync(search, page, pageSize);
            return Json(MapPagedResult(result));
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error interno: " + ex.Message });
        }
    }

    private static object MapPagedResult(Models.Shared.CatalogPagedResult<MarcaListItem> result)
        => new
        {
            items = result.Items.Select(r => new
            {
                id = r.IdBrand,
                name = r.Name,
                description = r.Description ?? ""
            }),
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize,
            totalPages = result.TotalPages
        };

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
