using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleCaja;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleCaja;

[Authorize]
public class DetalleCajaController : Controller
{
    public IActionResult Index()
    {
        var model = new DetalleCajaViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Logistica", "DetalleCaja", "logistica")
        };

        return View(model);
    }
}
