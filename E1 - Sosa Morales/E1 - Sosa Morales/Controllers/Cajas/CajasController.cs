using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Cajas;

[Authorize]
public class CajasController : Controller
{
    public IActionResult Index()
    {
        var model = new CajasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "Cajas", "logistica")
        };

        return View(model);
    }
}
