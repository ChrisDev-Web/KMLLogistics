using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.EstadosVenta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.EstadosVenta;

[Authorize]
public class EstadosVentaController : Controller
{
    public IActionResult Index()
    {
        var model = new EstadosVentaViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Ventas", "EstadosVenta", "ventas")
        };

        return View(model);
    }
}
