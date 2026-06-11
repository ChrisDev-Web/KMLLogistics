using E1___Sosa_Morales.Models.Estadisticas;

namespace E1___Sosa_Morales.Services.Estadisticas;

public interface IEstadisticasService
{
    Task<StatisticsDashboardResult> GetDashboardAsync(string? preset, DateTime? dateFrom, DateTime? dateTo);
}
