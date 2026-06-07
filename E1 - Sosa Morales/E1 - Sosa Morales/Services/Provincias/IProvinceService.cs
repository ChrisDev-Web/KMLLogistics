using E1___Sosa_Morales.Models.Provincias;

namespace E1___Sosa_Morales.Services.Provincias;

public interface IProvinceService
{
    Task<ProvincePagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<ProvincePagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<ProvinceDetail?> GetByIdAsync(int id);
    Task<List<ProvinceFkOption>> GetFkOptionsAsync();
    Task<(bool Success, string Message, int? Id)> CreateAsync(int idRegion, string name);
    Task<(bool Success, string Message)> UpdateAsync(int id, int idRegion, string name);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
