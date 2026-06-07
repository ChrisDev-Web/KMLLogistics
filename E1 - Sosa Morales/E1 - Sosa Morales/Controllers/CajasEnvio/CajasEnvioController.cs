using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.CajasEnvio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.CajasEnvio;

[Authorize]
public class CajasEnvioController : Controller
{
    public IActionResult Index()
    {
        var model = new CajasEnvioViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "CajasEnvio", "logistica")
        };

        return View(model);
    }
}
