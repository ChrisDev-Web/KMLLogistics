using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.TiposMovimiento;
using E1___Sosa_Morales.Services.TiposMovimiento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.TiposMovimiento;

[Authorize]
public class TiposMovimientoController : Controller
{
    private readonly IMovementTypeService _service;

    public TiposMovimientoController(IMovementTypeService service) => _service = service;

    public IActionResult Index()
        => View(new TiposMovimientoViewModel { Module = ModuleRegistry.BuildModuleView("Inventario", "TiposMovimiento", "inventario") });

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
                id = item.IdMovementType,
                name = item.Name,
                status = item.Status,
                createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        try
        {
            var (success, message, id) = await _service.CreateAsync(name.Trim());
            return Json(new { success, message, id });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name)
    {
        try
        {
            var (success, message) = await _service.UpdateAsync(id, name.Trim());
            return Json(new { success, message });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
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
