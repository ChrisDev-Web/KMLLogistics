using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Marcas;

public class MarcaListItem
{
    [Column("id_brand")] public int IdBrand { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
    [Column("description")] public string? Description { get; set; }
    [Column("status")] public byte Status { get; set; }
    [Column("total_count")] public int TotalCount { get; set; }
}

public class MarcaDetail : MarcaListItem
{
    [Column("created_at")] public DateTime? CreatedAt { get; set; }
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
}

public class MarcaSpResult
{
    [Column("success")] public int Success { get; set; }
    [Column("message")] public string Message { get; set; } = string.Empty;
    [Column("id_brand")] public int? IdBrand { get; set; }
}
