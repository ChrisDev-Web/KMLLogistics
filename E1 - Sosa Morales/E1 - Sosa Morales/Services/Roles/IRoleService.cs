using E1___Sosa_Morales.Models.Roles;

namespace E1___Sosa_Morales.Services.Roles;

public interface IRoleService
{
    Task<RolePagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<RolePagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<RoleDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
