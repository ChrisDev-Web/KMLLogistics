using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.MovimientosInventario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.MovimientosInventario;

[Authorize]
public class MovimientosInventarioController : Controller
{
    public IActionResult Index()
    {
        var model = new MovimientosInventarioViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Inventario", "MovimientosInventario", "inventario")
        };

        return View(model);
    }
}
