using System.Globalization;
using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Vehiculos;
using E1___Sosa_Morales.Services.Vehiculos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Vehiculos;

[Authorize]
public class VehiculosController : Controller
{
    private readonly IVehiculoService _service;

    public VehiculosController(IVehiculoService service) => _service = service;

    public IActionResult Index()
    {
        return View(new VehiculosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "Vehiculos", "logistica")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10, int? vehicleTypeId = null)
        => Json(await _service.ListActiveAsync(search, page, pageSize, vehicleTypeId));

    [HttpGet]
    public async Task<IActionResult> ListInactive(string? search, int page = 1, int pageSize = 10, int? vehicleTypeId = null)
        => Json(await _service.ListInactiveAsync(search, page, pageSize, vehicleTypeId));

    [HttpGet]
    public async Task<IActionResult> TypeOptions()
        => Json(await _service.GetVehicleTypeOptionsAsync());

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
                id = item.IdVehicle,
                vehicleTypeId = item.IdVehicleType,
                vehicleTypeName = item.VehicleTypeName,
                plate = item.Plate,
                maximumWeight = item.MaximumWeight,
                height = item.Height,
                width = item.Width,
                length = item.Length,
                maximumVolume = item.MaximumVolume,
                status = item.Status,
                createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int vehicleTypeId, string plate, string? maximumWeight, string? height, string? width, string? length)
    {
        var validation = BuildSaveModel(vehicleTypeId, plate, maximumWeight, height, width, length);
        if (!validation.Success) return Json(new { success = false, message = validation.Message });

        var (success, message, id) = await _service.CreateAsync(validation.Model);
        return Json(new { success, message, id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int vehicleTypeId, string plate, string? maximumWeight, string? height, string? width, string? length)
    {
        var validation = BuildSaveModel(vehicleTypeId, plate, maximumWeight, height, width, length);
        if (!validation.Success) return Json(new { success = false, message = validation.Message });

        var (success, message) = await _service.UpdateAsync(id, validation.Model);
        return Json(new { success, message });
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

    private static (bool Success, string Message, VehiculoSaveModel Model) BuildSaveModel(
        int vehicleTypeId,
        string? plate,
        string? maximumWeight,
        string? height,
        string? width,
        string? length)
    {
        if (vehicleTypeId <= 0)
            return (false, "Seleccione un tipo de vehiculo.", new());

        if (string.IsNullOrWhiteSpace(plate))
            return (false, "Ingrese la placa del vehiculo.", new());

        var parsedWeight = ParseNullableDecimal(maximumWeight, "peso maximo");
        if (!parsedWeight.Success) return (false, parsedWeight.Message, new());

        var parsedHeight = ParseNullableDecimal(height, "alto");
        if (!parsedHeight.Success) return (false, parsedHeight.Message, new());

        var parsedWidth = ParseNullableDecimal(width, "ancho");
        if (!parsedWidth.Success) return (false, parsedWidth.Message, new());

        var parsedLength = ParseNullableDecimal(length, "largo");
        if (!parsedLength.Success) return (false, parsedLength.Message, new());

        return (true, "", new VehiculoSaveModel
        {
            IdVehicleType = vehicleTypeId,
            Plate = plate.Trim().ToUpperInvariant(),
            MaximumWeight = parsedWeight.Value,
            Height = parsedHeight.Value,
            Width = parsedWidth.Value,
            Length = parsedLength.Value
        });
    }

    private static (bool Success, string Message, decimal? Value) ParseNullableDecimal(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (true, "", null);

        var normalized = value.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            return (false, $"Ingrese un valor valido para {field}.", null);

        if (result < 0)
            return (false, $"El {field} no puede ser negativo.", null);

        return (true, "", result);
    }
}
