using E1___Sosa_Morales.Models.EstadosVenta;

namespace E1___Sosa_Morales.Services.EstadosVenta;

public interface ISaleStatusService
{
    Task<SaleStatusPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<SaleStatusPagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<SaleStatusDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
