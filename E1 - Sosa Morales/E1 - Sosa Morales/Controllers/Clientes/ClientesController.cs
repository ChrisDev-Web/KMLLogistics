using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Clientes;

[Authorize]
public class ClientesController : Controller
{
    public IActionResult Index()
    {
        var model = new ClientesViewModel
        {
            Module = ModuleRegistry.BuildModuleView("PersonasTerceros", "Clientes", "clientes")
        };

        return View(model);
    }
}
