using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Estadisticas;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Estadisticas;

public class EstadisticasService : IEstadisticasService
{
    private readonly ApplicationDbContext _context;

    public EstadisticasService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StatisticsDashboardResult> GetDashboardAsync(string? preset, DateTime? dateFrom, DateTime? dateTo)
    {
        var period = ResolvePeriod(preset, dateFrom, dateTo);
        var previous = GetPreviousPeriod(period);

        var summary = await GetSummaryAsync(period.DateFrom, period.DateTo);
        var prevSummary = await GetSummaryAsync(previous.from, previous.to);

        var fromParam = new SqlParameter("@date_from", period.DateFrom);
        var toParam = new SqlParameter("@date_to", period.DateTo);
        var topParam = new SqlParameter("@top_n", 8);
        var topCatParam = new SqlParameter("@top_n", 6);

        var periodDays = (period.DateTo.Date - period.DateFrom.Date).Days + 1;
        var trendGranularity = periodDays == 1 ? "hourly" : "daily";
        var trendSp = periodDays == 1
            ? "EXEC dbo.sp_statistics_hourly_trend @date_from, @date_to"
            : "EXEC dbo.sp_statistics_daily_trend @date_from, @date_to";

        var trend = await _context.Database
            .SqlQueryRaw<StatisticsTrendRow>(trendSp, fromParam, toParam)
            .ToListAsync();

        var payments = await _context.Database
            .SqlQueryRaw<StatisticsPaymentRow>("EXEC dbo.sp_statistics_payment_breakdown @date_from, @date_to", fromParam, toParam)
            .ToListAsync();

        var topProducts = await _context.Database
            .SqlQueryRaw<StatisticsTopProductRow>("EXEC dbo.sp_statistics_top_products @date_from, @date_to, @top_n", fromParam, toParam, topParam)
            .ToListAsync();

        var topCategories = await _context.Database
            .SqlQueryRaw<StatisticsCategoryRow>("EXEC dbo.sp_statistics_top_categories @date_from, @date_to, @top_n", fromParam, toParam, topCatParam)
            .ToListAsync();

        var hourly = await _context.Database
            .SqlQueryRaw<StatisticsHourlyRow>("EXEC dbo.sp_statistics_hourly_activity @date_from, @date_to", fromParam, toParam)
            .ToListAsync();

        var recentSales = await _context.Database
            .SqlQueryRaw<StatisticsRecentSaleRow>("EXEC dbo.sp_statistics_recent_sales @date_from, @date_to, @top_n", fromParam, toParam, topParam)
            .ToListAsync();

        var stockAlerts = await _context.Database
            .SqlQueryRaw<StatisticsStockAlertRow>("EXEC dbo.sp_statistics_stock_alerts @top_n", topParam)
            .ToListAsync();

        return new StatisticsDashboardResult
        {
            Period = period,
            TrendGranularity = trendGranularity,
            Summary = summary,
            Comparison = BuildComparison(summary, prevSummary),
            Trend = trend,
            Payments = payments,
            TopProducts = topProducts,
            TopCategories = topCategories,
            Hourly = hourly,
            RecentSales = recentSales,
            StockAlerts = stockAlerts
        };
    }

    private async Task<StatisticsSummaryRow> GetSummaryAsync(DateTime from, DateTime to)
    {
        var fromParam = new SqlParameter("@date_from", from);
        var toParam = new SqlParameter("@date_to", to);
        var rows = await _context.Database
            .SqlQueryRaw<StatisticsSummaryRow>("EXEC dbo.sp_statistics_summary @date_from, @date_to", fromParam, toParam)
            .ToListAsync();
        return rows.FirstOrDefault() ?? new StatisticsSummaryRow();
    }

    private static StatisticsComparison BuildComparison(StatisticsSummaryRow current, StatisticsSummaryRow previous)
    {
        return new StatisticsComparison
        {
            Sales = Compare(current.TotalSales, previous.TotalSales),
            Purchases = Compare(current.TotalPurchases, previous.TotalPurchases),
            Profit = Compare(current.NetProfit, previous.NetProfit),
            NetFlow = Compare(current.NetBalance, previous.NetBalance)
        };
    }

    private static StatisticsComparisonMetric Compare(decimal current, decimal previous)
    {
        decimal pct = 0;
        if (previous != 0)
            pct = Math.Round(((current - previous) / previous) * 100, 1);
        else if (current != 0)
            pct = 100;

        return new StatisticsComparisonMetric
        {
            Current = current,
            Previous = previous,
            ChangePercent = pct
        };
    }

    private static (DateTime from, DateTime to) GetPreviousPeriod(StatisticsPeriodInfo period)
    {
        var days = (period.DateTo.Date - period.DateFrom.Date).Days + 1;
        var prevTo = period.DateFrom.Date.AddDays(-1);
        var prevFrom = prevTo.AddDays(-(days - 1));
        return (prevFrom, prevTo);
    }

    private static StatisticsPeriodInfo ResolvePeriod(string? preset, DateTime? dateFrom, DateTime? dateTo)
    {
        var today = DateTime.Today;
        var key = (preset ?? "last30").Trim().ToLowerInvariant();

        return key switch
        {
            "today" => new() { Preset = "today", Label = "Hoy", DateFrom = today, DateTo = today },
            "yesterday" => new() { Preset = "yesterday", Label = "Ayer", DateFrom = today.AddDays(-1), DateTo = today.AddDays(-1) },
            "last7" => new() { Preset = "last7", Label = "Últimos 7 días", DateFrom = today.AddDays(-6), DateTo = today },
            "last30" => new() { Preset = "last30", Label = "Últimos 30 días", DateFrom = today.AddDays(-29), DateTo = today },
            "month" => new() { Preset = "month", Label = "Este mes", DateFrom = new DateTime(today.Year, today.Month, 1), DateTo = today },
            "custom" when dateFrom.HasValue && dateTo.HasValue => BuildCustomPeriod(dateFrom.Value, dateTo.Value),
            _ => new() { Preset = "last30", Label = "Últimos 30 días", DateFrom = today.AddDays(-29), DateTo = today }
        };
    }

    private static StatisticsPeriodInfo BuildCustomPeriod(DateTime from, DateTime to)
    {
        if (from > to) (from, to) = (to, from);
        return new StatisticsPeriodInfo
        {
            Preset = "custom",
            Label = $"{from:dd/MM/yyyy} — {to:dd/MM/yyyy}",
            DateFrom = from.Date,
            DateTo = to.Date
        };
    }
}
