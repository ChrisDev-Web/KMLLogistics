using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleTransferencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleTransferencia;

[Authorize]
public class DetalleTransferenciaController : Controller
{
    public IActionResult Index()
    {
        var model = new DetalleTransferenciaViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Transferencias", "DetalleTransferencia", "transferencias")
        };

        return View(model);
    }
}
