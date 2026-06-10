using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.TiposVehiculo;
using E1___Sosa_Morales.Services.TiposVehiculo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.TiposVehiculo;

[Authorize]
public class TiposVehiculoController : Controller
{
    private readonly ITiposVehiculoService _service;

    public TiposVehiculoController(ITiposVehiculoService service) => _service = service;

    public IActionResult Index()
    {
        return View(new TiposVehiculoViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "TiposVehiculo", "logistica")
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
                id = item.IdVehicleType,
                name = item.Name,
                description = item.Description ?? "",
                vehicleCount = item.VehicleCount,
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
            var (success, message, id) = await _service.CreateAsync(BuildSaveModel(name, description));
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
            var (success, message) = await _service.UpdateAsync(id, BuildSaveModel(name, description));
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

    private static TipoVehiculoSaveModel BuildSaveModel(string name, string? description)
        => new()
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };
}
