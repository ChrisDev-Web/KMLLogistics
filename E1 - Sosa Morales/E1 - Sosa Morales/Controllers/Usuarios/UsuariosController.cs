using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Usuarios;
using E1___Sosa_Morales.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Usuarios;

[Authorize]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service) => _service = service;

    public IActionResult Index()
    {
        return View(new UsuariosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Seguridad", "Usuarios", "seguridad")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10)
        => Json(await _service.ListActiveAsync(search, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdUser,
                idRole = item.IdRole,
                username = item.Username,
                roleName = item.RoleName,
                createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> FkOptions()
    {
        var options = await _service.GetRoleOptionsAsync();
        return Json(new { success = true, items = options.Select(o => new { id = o.IdRole, name = o.Name }) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string username, string password, int idRole)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
                return Json(new { success = false, message = "La contraseña es obligatoria." });

            var (success, message, id) = await _service.CreateAsync(username.Trim(), password, idRole);
            return Json(new { success, message, id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string username, int idRole, string? password)
    {
        try
        {
            var (success, message) = await _service.UpdateAsync(id, username.Trim(), idRole, password);
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhysical(int id)
    {
        var (success, message) = await _service.DeletePhysicalAsync(id);
        return Json(new { success, message });
    }
}
