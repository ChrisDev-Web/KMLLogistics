using E1___Sosa_Morales.Models.TiposMovimiento;

namespace E1___Sosa_Morales.Services.TiposMovimiento;

public interface IMovementTypeService
{
    Task<MovementTypePagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<MovementTypePagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<MovementTypeDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
