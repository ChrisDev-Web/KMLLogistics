using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Productos;

[Authorize]
public class ProductosController : Controller
{
    public IActionResult Index()
    {
        var model = new ProductosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "Productos", "productos")
        };

        return View(model);
    }
}
