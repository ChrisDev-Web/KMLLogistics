using E1___Sosa_Morales.Models.DetalleAlmacen;

namespace E1___Sosa_Morales.Services.DetalleAlmacen;

public interface IWarehouseDetailService
{
    Task<WarehouseDetailMetrics?> GetMetricsAsync(int? idWarehouse);
    Task<WarehouseDetailPagedResult> ListSummaryAsync(string? search, int page, int pageSize);
    Task<WarehouseDetailPagedResult> ListProductsAsync(int idWarehouse, string? search, int page, int pageSize);
    Task<WarehouseDetailHeader?> GetWarehouseHeaderAsync(int idWarehouse);
    Task<WarehouseDetailRecord?> GetByIdAsync(int id);
    Task<List<WarehouseDetailOption>> GetWarehouseOptionsAsync();
}
