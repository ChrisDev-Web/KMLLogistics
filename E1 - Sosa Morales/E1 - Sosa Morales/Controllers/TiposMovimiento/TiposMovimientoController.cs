using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.TiposMovimiento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.TiposMovimiento;

[Authorize]
public class TiposMovimientoController : Controller
{
    public IActionResult Index()
    {
        var model = new TiposMovimientoViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Inventario", "TiposMovimiento", "inventario")
        };

        return View(model);
    }
}
