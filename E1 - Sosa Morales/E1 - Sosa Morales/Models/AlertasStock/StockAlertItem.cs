using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.AlertasStock;

public class StockAlertItem
{
    [Column("id_stock_alert")]
    public int IdStockAlert { get; set; }

    [Column("id_warehouse_detail")]
    public int IdWarehouseDetail { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("first_triggered_at")]
    public DateTime FirstTriggeredAt { get; set; }

    [Column("last_notified_at")]
    public DateTime LastNotifiedAt { get; set; }

    [Column("notification_count")]
    public int NotificationCount { get; set; }

    [Column("last_sent_by_user")]
    public int? LastSentByUser { get; set; }

    [Column("id_warehouse")]
    public int IdWarehouse { get; set; }

    [Column("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("stock")]
    public int Stock { get; set; }

    [Column("min_stock")]
    public int MinStock { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("last_sent_by_username")]
    public string? LastSentByUsername { get; set; }

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    [Column("stock_deficit")]
    public int StockDeficit { get; set; }

    public bool IsActive => string.Equals(Status, "ACTIVE", StringComparison.OrdinalIgnoreCase);
}
