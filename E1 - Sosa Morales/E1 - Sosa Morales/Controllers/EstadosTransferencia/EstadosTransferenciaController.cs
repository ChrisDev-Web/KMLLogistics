using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.EstadosTransferencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.EstadosTransferencia;

[Authorize]
public class EstadosTransferenciaController : Controller
{
    public IActionResult Index()
    {
        var model = new EstadosTransferenciaViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Transferencias", "EstadosTransferencia", "transferencias")
        };

        return View(model);
    }
}
