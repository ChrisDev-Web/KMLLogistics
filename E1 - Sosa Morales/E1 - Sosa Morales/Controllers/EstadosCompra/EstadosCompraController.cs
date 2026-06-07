using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.EstadosCompra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.EstadosCompra;

[Authorize]
public class EstadosCompraController : Controller
{
    public IActionResult Index()
    {
        var model = new EstadosCompraViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Compras", "EstadosCompra", "compras")
        };

        return View(model);
    }
}
