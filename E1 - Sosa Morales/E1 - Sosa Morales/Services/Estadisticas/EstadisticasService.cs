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

        var fromParam = new SqlParameter("@date_from", period.DateFrom);
        var toParam = new SqlParameter("@date_to", period.DateTo);
        var topParam = new SqlParameter("@top_n", 8);

        var summary = await _context.Database
            .SqlQueryRaw<StatisticsSummaryRow>(
                "EXEC dbo.sp_statistics_summary @date_from, @date_to",
                fromParam, toParam)
            .ToListAsync();

        var trend = await _context.Database
            .SqlQueryRaw<StatisticsTrendRow>(
                "EXEC dbo.sp_statistics_daily_trend @date_from, @date_to",
                fromParam, toParam)
            .ToListAsync();

        var payments = await _context.Database
            .SqlQueryRaw<StatisticsPaymentRow>(
                "EXEC dbo.sp_statistics_payment_breakdown @date_from, @date_to",
                fromParam, toParam)
            .ToListAsync();

        var topProducts = await _context.Database
            .SqlQueryRaw<StatisticsTopProductRow>(
                "EXEC dbo.sp_statistics_top_products @date_from, @date_to, @top_n",
                fromParam, toParam, topParam)
            .ToListAsync();

        return new StatisticsDashboardResult
        {
            Period = period,
            Summary = summary.FirstOrDefault() ?? new StatisticsSummaryRow(),
            Trend = trend,
            Payments = payments,
            TopProducts = topProducts
        };
    }

    private static StatisticsPeriodInfo ResolvePeriod(string? preset, DateTime? dateFrom, DateTime? dateTo)
    {
        var today = DateTime.Today;
        var key = (preset ?? "today").Trim().ToLowerInvariant();

        return key switch
        {
            "today" => new StatisticsPeriodInfo
            {
                Preset = "today",
                Label = "Hoy",
                DateFrom = today,
                DateTo = today
            },
            "yesterday" => new StatisticsPeriodInfo
            {
                Preset = "yesterday",
                Label = "Ayer",
                DateFrom = today.AddDays(-1),
                DateTo = today.AddDays(-1)
            },
            "last7" => new StatisticsPeriodInfo
            {
                Preset = "last7",
                Label = "Últimos 7 días",
                DateFrom = today.AddDays(-6),
                DateTo = today
            },
            "last30" => new StatisticsPeriodInfo
            {
                Preset = "last30",
                Label = "Últimos 30 días",
                DateFrom = today.AddDays(-29),
                DateTo = today
            },
            "month" => new StatisticsPeriodInfo
            {
                Preset = "month",
                Label = "Este mes",
                DateFrom = new DateTime(today.Year, today.Month, 1),
                DateTo = today
            },
            "custom" when dateFrom.HasValue && dateTo.HasValue =>
                BuildCustomPeriod(dateFrom.Value, dateTo.Value),
            _ => new StatisticsPeriodInfo
            {
                Preset = "today",
                Label = "Hoy",
                DateFrom = today,
                DateTo = today
            }
        };
    }

    private static StatisticsPeriodInfo BuildCustomPeriod(DateTime from, DateTime to)
    {
        if (from > to)
            (from, to) = (to, from);

        return new StatisticsPeriodInfo
        {
            Preset = "custom",
            Label = $"{from:dd/MM/yyyy} — {to:dd/MM/yyyy}",
            DateFrom = from.Date,
            DateTo = to.Date
        };
    }
}
