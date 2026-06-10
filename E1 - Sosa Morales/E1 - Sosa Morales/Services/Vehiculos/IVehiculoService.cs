using E1___Sosa_Morales.Models.Vehiculos;

namespace E1___Sosa_Morales.Services.Vehiculos;

public interface IVehiculoService
{
    Task<VehiculoPagedResult> ListActiveAsync(string? search, int page, int pageSize, int? vehicleTypeId);
    Task<VehiculoPagedResult> ListInactiveAsync(string? search, int page, int pageSize, int? vehicleTypeId);
    Task<List<VehiculoTypeOption>> GetVehicleTypeOptionsAsync();
    Task<VehiculoDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(VehiculoSaveModel model);
    Task<(bool Success, string Message)> UpdateAsync(int id, VehiculoSaveModel model);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
