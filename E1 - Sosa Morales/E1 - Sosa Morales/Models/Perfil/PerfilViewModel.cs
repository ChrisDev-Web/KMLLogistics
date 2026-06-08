namespace E1___Sosa_Morales.Models.Perfil;

public class PerfilViewModel
{
    public int IdUser { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string Initials { get; set; } = "US";
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
