using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleAlmacenCompra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleAlmacenCompra;

[Authorize]
public class DetalleAlmacenCompraController : Controller
{
    public IActionResult Index()
    {
        var model = new DetalleAlmacenCompraViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Compras", "DetalleAlmacenCompra", "compras")
        };

        return View(model);
    }
}
