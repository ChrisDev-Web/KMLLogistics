using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Estadisticas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Estadisticas;

[Authorize]
public class EstadisticasController : Controller
{
    public IActionResult Index()
    {
        var model = new EstadisticasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Estadisticas", "Estadisticas", "dashboard")
        };

        return View(model);
    }
}
