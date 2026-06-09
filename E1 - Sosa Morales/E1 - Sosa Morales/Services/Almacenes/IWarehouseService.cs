using E1___Sosa_Morales.Models.Almacenes;

namespace E1___Sosa_Morales.Services.Almacenes;

public interface IWarehouseService
{
    Task<WarehousePagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<WarehousePagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<WarehouseDetail?> GetByIdAsync(int id);
    Task<List<WarehouseDistrictOption>> GetDistrictOptionsAsync();
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string address, int? idDistrict);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string address, int? idDistrict);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
