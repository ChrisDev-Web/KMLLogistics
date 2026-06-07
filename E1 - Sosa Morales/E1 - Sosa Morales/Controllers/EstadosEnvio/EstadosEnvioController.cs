using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.EstadosEnvio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.EstadosEnvio;

[Authorize]
public class EstadosEnvioController : Controller
{
    public IActionResult Index()
    {
        var model = new EstadosEnvioViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "EstadosEnvio", "logistica")
        };

        return View(model);
    }
}
