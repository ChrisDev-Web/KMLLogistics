using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Perfil;

public class UserProfileDetail
{
    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("id_role")]
    public int IdRole { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("photo")]
    public string? Photo { get; set; }

    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
