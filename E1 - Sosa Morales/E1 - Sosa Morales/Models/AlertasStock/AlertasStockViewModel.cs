namespace E1___Sosa_Morales.Models.AlertasStock;

public class AlertasStockSummary
{
    public int ActiveCount { get; set; }
    public int HighNotifyCount { get; set; }
    public int ResolvedCount { get; set; }
    public int TotalCount { get; set; }
}

public class AlertasStockViewModel
{
    public StockAlertFilter Filter { get; set; } = new();
    public AlertasStockSummary Summary { get; set; } = new();
    public List<StockAlertItem> Alerts { get; set; } = [];
    public List<StockAlertFilterOption> Products { get; set; } = [];
    public List<StockAlertFilterOption> Warehouses { get; set; } = [];
    public int ResultCount => Alerts.Count;
}
