using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.ListaVentas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.ListaVentas;

[Authorize]
public class ListaVentasController : Controller
{
    public IActionResult Index()
    {
        var model = new ListaVentasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Ventas", "ListaVentas", "ventas")
        };

        return View(model);
    }
}
