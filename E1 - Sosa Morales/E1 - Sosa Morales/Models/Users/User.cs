using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Users;

public class User
{
    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("id_role")]
    public int IdRole { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;
}
