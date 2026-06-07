using System.Security.Claims;
using E1___Sosa_Morales.Models.Perfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Perfil;

[Authorize]
public class PerfilController : Controller
{
    public IActionResult Index()
    {
        var model = new PerfilViewModel
        {
            Username = User.Identity?.Name ?? "Usuario",
            RoleName = User.FindFirst(ClaimTypes.Role)?.Value ?? "Sin rol"
        };

        ViewBag.PageTitle = "Mi perfil";
        ViewBag.SidebarActive = "dashboard";
        return View(model);
    }
}
