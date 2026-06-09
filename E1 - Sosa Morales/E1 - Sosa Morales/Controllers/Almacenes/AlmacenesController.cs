using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Almacenes;
using E1___Sosa_Morales.Services.Almacenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Almacenes;

[Authorize]
public class AlmacenesController : Controller
{
    private readonly IWarehouseService _service;

    public AlmacenesController(IWarehouseService service) => _service = service;

    public IActionResult Index()
        => View(new AlmacenesViewModel { Module = ModuleRegistry.BuildModuleView("Inventario", "Almacenes", "inventario") });

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
                id = item.IdWarehouse,
                name = item.Name,
                address = item.Address,
                idDistrict = item.IdDistrict,
                countryName = item.CountryName,
                regionName = item.RegionName,
                provinceName = item.ProvinceName,
                districtName = item.DistrictName,
                status = item.Status,
                createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> DistrictOptions()
        => Json(new { success = true, items = await _service.GetDistrictOptionsAsync() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string address, int? idDistrict)
    {
        try
        {
            var (success, message, id) = await _service.CreateAsync(name.Trim(), address.Trim(), idDistrict);
            return Json(new { success, message, id });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name, string address, int? idDistrict)
    {
        try
        {
            var (success, message) = await _service.UpdateAsync(id, name.Trim(), address.Trim(), idDistrict);
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
