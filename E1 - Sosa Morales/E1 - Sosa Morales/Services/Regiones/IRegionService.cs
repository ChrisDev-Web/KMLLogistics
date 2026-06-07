using E1___Sosa_Morales.Models.Regiones;

namespace E1___Sosa_Morales.Services.Regiones;

public interface IRegionService
{
    Task<RegionPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<RegionPagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<RegionDetail?> GetByIdAsync(int id);
    Task<List<RegionFkOption>> GetFkOptionsAsync();
    Task<(bool Success, string Message, int? Id)> CreateAsync(int idCountry, string name);
    Task<(bool Success, string Message)> UpdateAsync(int id, int idCountry, string name);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
