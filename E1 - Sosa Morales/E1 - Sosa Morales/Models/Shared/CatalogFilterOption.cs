using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Shared;

public class CatalogFilterOption
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}
