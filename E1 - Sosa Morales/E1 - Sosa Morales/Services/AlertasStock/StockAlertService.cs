using System.Data;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.AlertasStock;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.AlertasStock;

public class StockAlertService : IStockAlertService
{
    private readonly ApplicationDbContext _context;

    public StockAlertService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<StockAlertItem>> GetAlertsAsync(StockAlertFilter filter)
    {
        var searchParam = new SqlParameter("@search", (object?)filter.Search ?? DBNull.Value);
        var productParam = new SqlParameter("@id_product", (object?)filter.IdProduct ?? DBNull.Value);
        var warehouseParam = new SqlParameter("@id_warehouse", (object?)filter.IdWarehouse ?? DBNull.Value);
        var statusParam = new SqlParameter("@status", filter.Status ?? "ACTIVE");

        return _context.Database
            .SqlQueryRaw<StockAlertItem>(
                "EXEC dbo.sp_stock_alert_list_active @search, @id_product, @id_warehouse, @status",
                searchParam, productParam, warehouseParam, statusParam)
            .ToListAsync();
    }

    public Task<List<StockAlertItem>> GetActiveAlertsAsync()
    {
        return GetAlertsAsync(new StockAlertFilter { Status = "ACTIVE" });
    }

    public async Task<int> GetActiveCountAsync()
    {
        var stockResult = await _context.Database
            .SqlQueryRaw<StockAlertCountResult>("EXEC dbo.sp_stock_alert_count_active")
            .ToListAsync();
        var logisticsResult = await _context.Database
            .SqlQueryRaw<LogisticsAlertCountResult>("EXEC dbo.sp_logistics_alert_count_active")
            .ToListAsync();

        return (stockResult.FirstOrDefault()?.ActiveAlerts ?? 0)
             + (logisticsResult.FirstOrDefault()?.ActiveAlerts ?? 0);
    }

    public Task<List<StockAlertFilterOption>> GetProductFilterOptionsAsync(string status = "ALL")
    {
        var statusParam = new SqlParameter("@status", status);
        return _context.Database
            .SqlQueryRaw<StockAlertFilterOption>(
                "EXEC dbo.sp_stock_alert_list_products_filter @status",
                statusParam)
            .ToListAsync();
    }

    public Task<List<StockAlertFilterOption>> GetWarehouseFilterOptionsAsync(string status = "ALL")
    {
        var statusParam = new SqlParameter("@status", status);
        return _context.Database
            .SqlQueryRaw<StockAlertFilterOption>(
                "EXEC dbo.sp_stock_alert_list_warehouses_filter @status",
                statusParam)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> ResendAsync(int idStockAlert, int idUser)
    {
        var idParam = new SqlParameter("@id_stock_alert", idStockAlert);
        var userParam = new SqlParameter("@id_user", idUser);
        var messageParam = new SqlParameter("@message", SqlDbType.VarChar, 255)
        {
            Direction = ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_stock_alert_resend @id_stock_alert, @id_user, @message OUTPUT",
            idParam, userParam, messageParam);

        var message = messageParam.Value?.ToString() ?? "No se pudo reenviar la alerta.";
        var success = message.Contains("correctamente", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cerró", StringComparison.OrdinalIgnoreCase);

        return (success, message);
    }
}
