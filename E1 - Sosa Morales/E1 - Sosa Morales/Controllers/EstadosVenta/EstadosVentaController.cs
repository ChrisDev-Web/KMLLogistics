using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.EstadosVenta;
using E1___Sosa_Morales.Services.EstadosVenta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.EstadosVenta;

[Authorize]
public class EstadosVentaController : Controller
{
    private readonly ISaleStatusService _service;

    public EstadosVentaController(ISaleStatusService service) => _service = service;

    public IActionResult Index()
        => View(new EstadosVentaViewModel { Module = ModuleRegistry.BuildModuleView("Ventas", "EstadosVenta", "ventas") });

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
        if (item is null) return Json(new { success = false, message = "Estado no encontrado." });
        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdSaleStatus,
                name = item.Name,
                description = item.Description,
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
            if (string.IsNullOrWhiteSpace(name)) return Json(new { success = false, message = "El nombre es obligatorio." });
            var (success, message, id) = await _service.CreateAsync(name.Trim(), description?.Trim());
            return Json(new { success, message, id });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name, string? description)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name)) return Json(new { success = false, message = "El nombre es obligatorio." });
            var (success, message) = await _service.UpdateAsync(id, name.Trim(), description?.Trim());
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
