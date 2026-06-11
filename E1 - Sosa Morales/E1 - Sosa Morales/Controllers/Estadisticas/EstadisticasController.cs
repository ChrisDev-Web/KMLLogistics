using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Estadisticas;
using E1___Sosa_Morales.Services.Estadisticas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Estadisticas;

[Authorize]
public class EstadisticasController : Controller
{
    private readonly IEstadisticasService _estadisticasService;

    public EstadisticasController(IEstadisticasService estadisticasService)
    {
        _estadisticasService = estadisticasService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new EstadisticasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Estadisticas", "Estadisticas", "dashboard"),
            Dashboard = await _estadisticasService.GetDashboardAsync("today", null, null)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Data(string? preset, DateTime? dateFrom, DateTime? dateTo)
    {
        var data = await _estadisticasService.GetDashboardAsync(preset, dateFrom, dateTo);
        return Json(data);
    }
}
