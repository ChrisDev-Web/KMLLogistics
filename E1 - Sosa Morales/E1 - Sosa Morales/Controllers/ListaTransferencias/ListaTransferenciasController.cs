using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.ListaTransferencias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.ListaTransferencias;

[Authorize]
public class ListaTransferenciasController : Controller
{
    public IActionResult Index()
    {
        var model = new ListaTransferenciasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Transferencias", "ListaTransferencias", "transferencias")
        };

        return View(model);
    }
}
