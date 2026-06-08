using E1___Sosa_Morales.Models.Cargos;

namespace E1___Sosa_Morales.Services.Cargos;

public interface IJobPositionService
{
    Task<JobPositionPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<JobPositionPagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<JobPositionDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
