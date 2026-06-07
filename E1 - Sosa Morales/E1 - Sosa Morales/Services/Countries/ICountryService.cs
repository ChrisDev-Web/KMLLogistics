using E1___Sosa_Morales.Models.Countries;

namespace E1___Sosa_Morales.Services.Countries;

public interface ICountryService
{
    Task<CountryPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<CountryPagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<CountryDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
