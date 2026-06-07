using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Almacenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Almacenes;

[Authorize]
public class AlmacenesController : Controller
{
    public IActionResult Index()
    {
        var model = new AlmacenesViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Inventario", "Almacenes", "inventario")
        };

        return View(model);
    }
}
