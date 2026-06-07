using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Distritos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Distritos;

[Authorize]
public class DistritosController : Controller
{
    public IActionResult Index()
    {
        var model = new DistritosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Configuracion", "Distritos", "dashboard")
        };

        return View(model);
    }
}
