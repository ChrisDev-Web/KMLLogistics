using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Usuarios;

public class UsuarioListItem
{
    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class UsuarioDetail
{
    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("id_role")]
    public int IdRole { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class UsuarioSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_user")]
    public int? IdUser { get; set; }
}

public class UsuarioPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UsuarioRoleOption
{
    [Column("id_role")]
    public int IdRole { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}
