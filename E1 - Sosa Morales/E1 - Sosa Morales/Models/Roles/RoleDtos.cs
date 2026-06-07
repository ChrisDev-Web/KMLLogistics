using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Roles;

public class RoleListItem
{
    [Column("id_role")]
    public int IdRole { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class RoleDetail
{
    [Column("id_role")]
    public int IdRole { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class RoleSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_role")]
    public int? IdRole { get; set; }
}

public class RolePagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
