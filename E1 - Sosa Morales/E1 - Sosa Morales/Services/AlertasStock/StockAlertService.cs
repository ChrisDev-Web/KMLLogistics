using System.Data;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.AlertasStock;
using E1___Sosa_Morales.Models.SeguimientoVehiculo;
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

    public async Task<List<UnifiedAlertRow>> GetUnifiedAlertsAsync(StockAlertFilter filter)
    {
        var status = filter.Status ?? "ACTIVE";
        var stockAlerts = await GetAlertsAsync(filter);
        var logisticsAlerts = await GetLogisticsAlertsAsync(status);

        var rows = new List<UnifiedAlertRow>();

        foreach (var alert in stockAlerts)
        {
            rows.Add(new UnifiedAlertRow
            {
                Kind = "STOCK",
                Id = alert.IdStockAlert,
                Title = alert.ProductName,
                Subtitle = BuildStockSubtitle(alert),
                Level = alert.IsActive ? "warning" : "resolved",
                LevelLabel = alert.IsActive ? "Stock bajo" : "Resuelta",
                AlertType = "LOW_STOCK",
                EventAt = alert.LastNotifiedAt,
                NotificationCount = alert.NotificationCount,
                IsActive = alert.IsActive,
                LastSentByUsername = alert.LastSentByUsername
            });
        }

        var search = filter.Search?.Trim();
        foreach (var alert in logisticsAlerts)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search;
                if (alert.VehiclePlate.Contains(term, StringComparison.OrdinalIgnoreCase) == false
                    && alert.Message.Contains(term, StringComparison.OrdinalIgnoreCase) == false
                    && alert.AlertType.Contains(term, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }
            }

            rows.Add(MapLogisticsAlert(alert));
        }

        return rows
            .OrderByDescending(r => r.EventAt)
            .ThenBy(r => r.Title)
            .ToList();
    }

    public async Task<List<AlertNotificationItem>> GetNotificationFeedAsync()
    {
        var stockAlerts = await GetActiveAlertsAsync();
        var logisticsAlerts = await GetLogisticsAlertsAsync("ACTIVE");

        var items = new List<AlertNotificationItem>();

        foreach (var alert in stockAlerts)
        {
            var row = new UnifiedAlertRow
            {
                Kind = "STOCK",
                Id = alert.IdStockAlert,
                EventAt = alert.LastNotifiedAt
            };

            items.Add(new AlertNotificationItem
            {
                Key = row.NotificationKey,
                Kind = "STOCK",
                Id = alert.IdStockAlert,
                Level = "warning",
                Message = $"Stock bajo: {alert.ProductName} ({alert.Stock} u.)"
            });
        }

        foreach (var alert in logisticsAlerts)
        {
            var mapped = MapLogisticsAlert(alert);
            items.Add(new AlertNotificationItem
            {
                Key = mapped.NotificationKey,
                Kind = "LOGISTICS",
                Id = alert.IdLogisticsAlert,
                Level = mapped.Level,
                Message = BuildLogisticsToastMessage(alert)
            });
        }

        return items
            .OrderByDescending(i => i.Key)
            .ToList();
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

    public async Task<(bool Success, string Message)> ResendAsync(string kind, int id, int idUser)
    {
        if (string.Equals(kind, "LOGISTICS", StringComparison.OrdinalIgnoreCase))
            return await ResendLogisticsAsync(id, idUser);

        return await ResendStockAsync(id, idUser);
    }

    private Task<List<LogisticsAlertItem>> GetLogisticsAlertsAsync(string status)
    {
        var statusParam = new SqlParameter("@status", status);
        return _context.Database
            .SqlQueryRaw<LogisticsAlertItem>(
                "EXEC dbo.sp_logistics_alert_list_active @status",
                statusParam)
            .ToListAsync();
    }

    private static UnifiedAlertRow MapLogisticsAlert(LogisticsAlertItem alert)
    {
        var isReturning = string.Equals(alert.AlertType, "RETURNING", StringComparison.OrdinalIgnoreCase);
        var isActive = string.Equals(alert.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase);

        return new UnifiedAlertRow
        {
            Kind = "LOGISTICS",
            Id = alert.IdLogisticsAlert,
            Title = alert.VehiclePlate,
            Subtitle = alert.Message,
            Level = isActive
                ? (isReturning ? "info" : "success")
                : "resolved",
            LevelLabel = isReturning ? "En regreso" : "Disponible",
            AlertType = alert.AlertType,
            EventAt = alert.CreatedAt,
            NotificationCount = 1,
            IsActive = isActive,
            LastSentByUsername = null
        };
    }

    private static string BuildStockSubtitle(StockAlertItem alert)
    {
        var parts = new List<string>
        {
            alert.WarehouseName,
            $"Stock {alert.Stock} u."
        };

        if (alert.Stock <= 10)
            parts.Add("Umbral 10");

        if (!string.IsNullOrWhiteSpace(alert.Location))
            parts.Add(alert.Location);

        return string.Join(" · ", parts);
    }

    private static string BuildLogisticsToastMessage(LogisticsAlertItem alert)
    {
        if (string.Equals(alert.AlertType, "RETURNING", StringComparison.OrdinalIgnoreCase))
            return $"Vehículo {alert.VehiclePlate} regresando al almacén";

        return $"Vehículo {alert.VehiclePlate} disponible para envíos";
    }

    private async Task<(bool Success, string Message)> ResendStockAsync(int idStockAlert, int idUser)
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

    private async Task<(bool Success, string Message)> ResendLogisticsAsync(int idLogisticsAlert, int idUser)
    {
        var idParam = new SqlParameter("@id_logistics_alert", idLogisticsAlert);
        var userParam = new SqlParameter("@id_user", idUser);
        var messageParam = new SqlParameter("@message", SqlDbType.VarChar, 255)
        {
            Direction = ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_logistics_alert_resend @id_logistics_alert, @id_user, @message OUTPUT",
            idParam, userParam, messageParam);

        var message = messageParam.Value?.ToString() ?? "No se pudo reenviar la alerta.";
        var success = message.Contains("correctamente", StringComparison.OrdinalIgnoreCase);

        return (success, message);
    }
}
