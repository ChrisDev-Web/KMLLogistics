using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.AlertasStock;

public class StockAlertCountResult
{
    [Column("active_alerts")]
    public int ActiveAlerts { get; set; }
}
