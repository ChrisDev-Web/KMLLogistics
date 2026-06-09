using E1___Sosa_Morales.Models.EstadosCompra;

namespace E1___Sosa_Morales.Services.EstadosCompra;

public interface IPurchaseStatusService
{
    Task<PurchaseStatusPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<PurchaseStatusPagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<PurchaseStatusDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
