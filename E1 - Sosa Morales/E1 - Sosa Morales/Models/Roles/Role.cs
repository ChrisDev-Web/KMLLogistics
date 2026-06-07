using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Roles;

public class Role
{
    [Column("id_role")]
    public int IdRole { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }
}
