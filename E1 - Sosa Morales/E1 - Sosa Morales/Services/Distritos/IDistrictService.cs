using E1___Sosa_Morales.Models.Distritos;

namespace E1___Sosa_Morales.Services.Distritos;

public interface IDistrictService
{
    Task<DistrictPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<DistrictPagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<DistrictDetail?> GetByIdAsync(int id);
    Task<List<DistrictFkOption>> GetFkOptionsAsync();
    Task<(bool Success, string Message, int? Id)> CreateAsync(int idProvince, string name);
    Task<(bool Success, string Message)> UpdateAsync(int id, int idProvince, string name);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
