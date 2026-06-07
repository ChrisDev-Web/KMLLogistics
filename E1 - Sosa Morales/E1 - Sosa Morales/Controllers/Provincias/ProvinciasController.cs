using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Provincias;
using E1___Sosa_Morales.Services.Provincias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Provincias;

[Authorize]
public class ProvinciasController : Controller
{
    private readonly IProvinceService _service;

    public ProvinciasController(IProvinceService service) => _service = service;

    public IActionResult Index()
    {
        return View(new ProvinciasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Configuracion", "Provincias", "dashboard")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10)
        => Json(await _service.ListActiveAsync(search, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> ListInactive(string? search, int page = 1, int pageSize = 10)
        => Json(await _service.ListInactiveAsync(search, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdProvince,
                idRegion = item.IdRegion,
                name = item.Name,
                regionName = item.RegionName,
                status = item.Status,
                createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> FkOptions()
    {
        var options = await _service.GetFkOptionsAsync();
        return Json(new { success = true, items = options.Select(o => new { id = o.IdRegion, name = o.Name }) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int idRegion, string name)
    {
        try
        {
            var (success, message, id) = await _service.CreateAsync(idRegion, name.Trim());
            return Json(new { success, message, id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int idRegion, string name)
    {
        try
        {
            var (success, message) = await _service.UpdateAsync(id, idRegion, name.Trim());
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogic(int id)
    {
        var (success, message) = await _service.DeleteLogicAsync(id);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var (success, message) = await _service.RestoreAsync(id);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhysical(int id)
    {
        var (success, message) = await _service.DeletePhysicalAsync(id);
        return Json(new { success, message });
    }
}
