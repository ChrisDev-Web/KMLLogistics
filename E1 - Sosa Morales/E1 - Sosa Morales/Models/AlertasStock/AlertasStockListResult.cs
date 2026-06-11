namespace E1___Sosa_Morales.Models.AlertasStock;

public class AlertasStockListResult
{
    public List<UnifiedAlertRow> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int ShowingFrom { get; set; }
    public int ShowingTo { get; set; }
    public AlertasStockSummary Summary { get; set; } = new();
}
