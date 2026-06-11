using E1___Sosa_Morales.Models.DetalleVenta;
using E1___Sosa_Morales.Models.Shared;

namespace E1___Sosa_Morales.Services.DetalleVenta;

public interface ISaleDetailService
{
    Task<SaleDetailPagedResult> ListAsync(string? search, int? idSale, int? idProduct, int? idClient, int page, int pageSize);
    Task<SaleDetailMetrics?> GetMetricsAsync(string? search, int? idSale, int? idProduct, int? idClient);
    Task<List<CatalogFilterOption>> GetProductFilterOptionsAsync();
    Task<List<CatalogFilterOption>> GetClientFilterOptionsAsync();
}
