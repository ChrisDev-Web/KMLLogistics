using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.AlertasStock;

public class StockAlertFilterOption
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}
