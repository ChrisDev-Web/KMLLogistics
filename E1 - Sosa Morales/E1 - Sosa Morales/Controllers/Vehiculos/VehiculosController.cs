using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Vehiculos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Vehiculos;

[Authorize]
public class VehiculosController : Controller
{
    public IActionResult Index()
    {
        var model = new VehiculosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "Vehiculos", "logistica")
        };

        return View(model);
    }
}
