using E1___Sosa_Morales.Models.AlertasStock;

namespace E1___Sosa_Morales.Services.AlertasStock;

public interface IStockAlertService
{
    Task<List<StockAlertItem>> GetAlertsAsync(StockAlertFilter filter);
    Task<List<StockAlertItem>> GetActiveAlertsAsync();
    Task<int> GetActiveCountAsync();
    Task<List<StockAlertFilterOption>> GetProductFilterOptionsAsync(string status = "ALL");
    Task<List<StockAlertFilterOption>> GetWarehouseFilterOptionsAsync(string status = "ALL");
    Task<(bool Success, string Message)> ResendAsync(int idStockAlert, int idUser);
}
