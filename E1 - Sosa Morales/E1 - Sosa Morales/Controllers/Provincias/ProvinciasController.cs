using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Provincias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Provincias;

[Authorize]
public class ProvinciasController : Controller
{
    public IActionResult Index()
    {
        var model = new ProvinciasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Configuracion", "Provincias", "dashboard")
        };

        return View(model);
    }
}
