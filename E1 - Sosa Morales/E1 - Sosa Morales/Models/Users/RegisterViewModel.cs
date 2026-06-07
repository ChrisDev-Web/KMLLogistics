using System.ComponentModel.DataAnnotations;
using E1___Sosa_Morales.Models.Roles;

namespace E1___Sosa_Morales.Models.Users;

public class RegisterViewModel
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol.")]
    [Display(Name = "Rol")]
    public int IdRole { get; set; }

    public List<Role> Roles { get; set; } = [];

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme su contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
