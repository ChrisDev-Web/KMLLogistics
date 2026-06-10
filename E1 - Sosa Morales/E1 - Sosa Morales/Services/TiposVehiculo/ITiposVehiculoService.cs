using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E1___Sosa_Morales.Models.TiposVehiculo;

namespace E1___Sosa_Morales.Services.TiposVehiculo;

public interface ITiposVehiculoService
{
    Task<TipoVehiculoPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<TipoVehiculoPagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<TipoVehiculoDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(TipoVehiculoSaveModel model);
    Task<(bool Success, string Message)> UpdateAsync(int id, TipoVehiculoSaveModel model);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}