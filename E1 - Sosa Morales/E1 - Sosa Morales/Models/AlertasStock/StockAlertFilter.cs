namespace E1___Sosa_Morales.Models.AlertasStock;

public class StockAlertFilter
{
    public string? Search { get; set; }
    public int? IdProduct { get; set; }
    public int? IdWarehouse { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
