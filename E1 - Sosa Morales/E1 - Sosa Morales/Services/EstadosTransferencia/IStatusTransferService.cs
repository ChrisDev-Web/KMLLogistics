using E1___Sosa_Morales.Models.EstadosTransferencia;

namespace E1___Sosa_Morales.Services.EstadosTransferencia;

public interface IStatusTransferService
{
    Task<StatusTransferPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<StatusTransferPagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<StatusTransferDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
