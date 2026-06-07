using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Regiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Regiones;

[Authorize]
public class RegionesController : Controller
{
    public IActionResult Index()
    {
        var model = new RegionesViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Configuracion", "Regiones", "dashboard")
        };

        return View(model);
    }
}
