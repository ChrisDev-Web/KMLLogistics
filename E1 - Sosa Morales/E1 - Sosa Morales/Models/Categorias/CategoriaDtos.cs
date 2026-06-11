using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Categorias;

public class CategoriaListItem
{
    [Column("id_category")]
    public int IdCategory { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("photo")]
    public string? Photo { get; set; }

    [Column("status")]
    public byte Status { get; set; }

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class CategoriaDetail
{
    [Column("id_category")]
    public int IdCategory { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("photo")]
    public string? Photo { get; set; }
}

public class CategoriaSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_category")]
    public int? IdCategory { get; set; }
}
