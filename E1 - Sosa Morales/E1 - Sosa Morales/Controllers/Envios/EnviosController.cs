using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Envios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Envios;

[Authorize]
public class EnviosController : Controller
{
    public IActionResult Index()
    {
        var model = new EnviosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "Envios", "logistica")
        };

        return View(model);
    }
}
