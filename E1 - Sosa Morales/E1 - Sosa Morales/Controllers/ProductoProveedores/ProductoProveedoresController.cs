using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.ProductoProveedores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.ProductoProveedores;

[Authorize]
public class ProductoProveedoresController : Controller
{
    public IActionResult Index()
    {
        var model = new ProductoProveedoresViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "ProductoProveedores", "productos")
        };

        return View(model);
    }
}
