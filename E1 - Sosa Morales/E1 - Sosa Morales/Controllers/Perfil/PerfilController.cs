using System.Security.Claims;
using E1___Sosa_Morales.Models.Perfil;
using E1___Sosa_Morales.Models.Users;
using E1___Sosa_Morales.Services.Perfil;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Perfil;

[Authorize]
public class PerfilController : Controller
{
    private static readonly string[] AllowedPhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxPhotoBytes = 2 * 1024 * 1024;

    private readonly IPerfilService _perfilService;
    private readonly IWebHostEnvironment _env;
    private readonly PasswordHasher<User> _hasher = new();

    public PerfilController(IPerfilService perfilService, IWebHostEnvironment env)
    {
        _perfilService = perfilService;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var profile = await GetCurrentProfileAsync();
        if (profile is null) return RedirectToAction("Index", "Login");

        var model = MapToViewModel(profile);
        if (TempData["SuccessMessage"] is string success) model.SuccessMessage = success;
        if (TempData["ErrorMessage"] is string error) model.ErrorMessage = error;

        ViewBag.PageTitle = "Mi perfil";
        ViewBag.SidebarActive = "dashboard";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAccount(string username, string? newPassword, string? confirmPassword)
    {
        var idUser = GetCurrentUserId();
        if (idUser is null) return RedirectToAction("Index", "Login");

        username = username.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            TempData["ErrorMessage"] = "El nombre de usuario es obligatorio.";
            return RedirectToAction(nameof(Index));
        }

        string? passwordHash = null;
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "La contraseña debe tener al menos 6 caracteres.";
                return RedirectToAction(nameof(Index));
            }
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Las contraseñas no coinciden.";
                return RedirectToAction(nameof(Index));
            }
            var tempUser = new User { Username = username };
            passwordHash = _hasher.HashPassword(tempUser, newPassword);
        }

        var (success, message) = await _perfilService.UpdateAccountAsync(idUser.Value, username, passwordHash);
        if (!success)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        await RefreshSignInAsync(idUser.Value);
        TempData["SuccessMessage"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePhoto(IFormFile? photo)
    {
        var idUser = GetCurrentUserId();
        if (idUser is null) return RedirectToAction("Index", "Login");

        if (photo is null || photo.Length == 0)
        {
            TempData["ErrorMessage"] = "Seleccione una imagen para subir.";
            return RedirectToAction(nameof(Index));
        }

        if (photo.Length > MaxPhotoBytes)
        {
            TempData["ErrorMessage"] = "La imagen no puede superar 2 MB.";
            return RedirectToAction(nameof(Index));
        }

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!AllowedPhotoExtensions.Contains(extension))
        {
            TempData["ErrorMessage"] = "Formato no permitido. Use JPG, PNG o WEBP.";
            return RedirectToAction(nameof(Index));
        }

        var profile = await _perfilService.GetByIdAsync(idUser.Value);
        if (profile is null)
        {
            TempData["ErrorMessage"] = "Perfil no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var profilesDir = Path.Combine(_env.WebRootPath, "Public", "Images", "Profiles");
        Directory.CreateDirectory(profilesDir);

        var fileName = $"user_{idUser.Value}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var physicalPath = Path.Combine(profilesDir, fileName);
        var webPath = $"/Public/Images/Profiles/{fileName}";

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
            await photo.CopyToAsync(stream);

        var (success, message) = await _perfilService.UpdatePhotoAsync(idUser.Value, webPath);
        if (!success)
        {
            if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        DeletePhotoFile(profile.Photo);

        await RefreshSignInAsync(idUser.Value);
        TempData["SuccessMessage"] = "Foto de perfil actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoto()
    {
        var idUser = GetCurrentUserId();
        if (idUser is null) return RedirectToAction("Index", "Login");

        var profile = await _perfilService.GetByIdAsync(idUser.Value);
        if (profile is null)
        {
            TempData["ErrorMessage"] = "Perfil no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var (success, message) = await _perfilService.UpdatePhotoAsync(idUser.Value, null);
        if (!success)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        DeletePhotoFile(profile.Photo);
        await RefreshSignInAsync(idUser.Value);
        TempData["SuccessMessage"] = "Foto de perfil eliminada.";
        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private async Task<UserProfileDetail?> GetCurrentProfileAsync()
    {
        var id = GetCurrentUserId();
        return id is null ? null : await _perfilService.GetByIdAsync(id.Value);
    }

    private async Task RefreshSignInAsync(int idUser)
    {
        var profile = await _perfilService.GetByIdAsync(idUser);
        if (profile is null) return;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, profile.IdUser.ToString()),
            new(ClaimTypes.Name, profile.Username),
            new(ClaimTypes.Role, profile.RoleName),
            new("id_role", profile.IdRole.ToString())
        };
        if (!string.IsNullOrWhiteSpace(profile.Photo))
            claims.Add(new Claim("photo", profile.Photo));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    private static PerfilViewModel MapToViewModel(UserProfileDetail profile) => new()
    {
        IdUser = profile.IdUser,
        Username = profile.Username,
        RoleName = profile.RoleName,
        PhotoUrl = profile.Photo,
        Initials = GetInitials(profile.Username),
        CreatedAt = profile.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
        UpdatedAt = profile.UpdatedAt?.ToString("dd/MM/yyyy HH:mm")
    };

    private static string GetInitials(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return "US";
        var trimmed = username.Trim();
        return trimmed.Length >= 2 ? trimmed[..2].ToUpperInvariant() : trimmed.ToUpperInvariant();
    }

    private void DeletePhotoFile(string? photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath)) return;
        var relative = photoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_env.WebRootPath, relative);
        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
    }
}
