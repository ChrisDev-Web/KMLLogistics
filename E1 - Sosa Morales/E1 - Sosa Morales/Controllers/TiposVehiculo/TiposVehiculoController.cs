using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.TiposVehiculo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.TiposVehiculo;

[Authorize]
public class TiposVehiculoController : Controller
{
    public IActionResult Index()
    {
        var model = new TiposVehiculoViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "TiposVehiculo", "logistica")
        };

        return View(model);
    }
}
