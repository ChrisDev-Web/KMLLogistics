using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.OrdenesCompra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.OrdenesCompra;

[Authorize]
public class OrdenesCompraController : Controller
{
    public IActionResult Index()
    {
        var model = new OrdenesCompraViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Compras", "OrdenesCompra", "compras")
        };

        return View(model);
    }
}
