using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleCompra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleCompra;

[Authorize]
public class DetalleCompraController : Controller
{
    public IActionResult Index()
    {
        var model = new DetalleCompraViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Compras", "DetalleCompra", "compras")
        };

        return View(model);
    }
}
