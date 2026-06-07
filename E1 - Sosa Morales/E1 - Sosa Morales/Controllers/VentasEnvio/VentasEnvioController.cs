using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.VentasEnvio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.VentasEnvio;

[Authorize]
public class VentasEnvioController : Controller
{
    public IActionResult Index()
    {
        var model = new VentasEnvioViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "VentasEnvio", "logistica")
        };

        return View(model);
    }
}
