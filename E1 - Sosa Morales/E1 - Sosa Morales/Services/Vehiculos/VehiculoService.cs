using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Vehiculos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Vehiculos;

public class VehiculoService : IVehiculoService
{
    private readonly ApplicationDbContext _context;

    public VehiculoService(ApplicationDbContext context) => _context = context;

    public Task<VehiculoPagedResult> ListActiveAsync(string? search, int page, int pageSize, int? vehicleTypeId)
        => QueryListAsync(true, search, page, pageSize, vehicleTypeId);

    public Task<VehiculoPagedResult> ListInactiveAsync(string? search, int page, int pageSize, int? vehicleTypeId)
        => QueryListAsync(false, search, page, pageSize, vehicleTypeId);

    public async Task<List<VehiculoTypeOption>> GetVehicleTypeOptionsAsync()
        => await _context.Database.SqlQueryRaw<VehiculoTypeOption>(
            "EXEC dbo.sp_vehicle_type_options")
            .ToListAsync();

    public async Task<VehiculoDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<VehiculoDetail>(
            "EXEC dbo.sp_vehicle_get_by_id @id_vehicle",
            new SqlParameter("@id_vehicle", id))
            .ToListAsync();

        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(VehiculoSaveModel model)
    {
        var rows = await _context.Database.SqlQueryRaw<VehiculoSpResult>(
            "EXEC dbo.sp_vehicle_create @id_vehicle_type, @plate, @maximum_weight, @height, @width, @length",
            new SqlParameter("@id_vehicle_type", model.IdVehicleType),
            Param("@plate", model.Plate),
            Param("@maximum_weight", model.MaximumWeight),
            Param("@height", model.Height),
            Param("@width", model.Width),
            Param("@length", model.Length))
            .ToListAsync();

        var row = rows.FirstOrDefault();
        return row is null
            ? (false, "No se pudo crear el vehiculo.", null)
            : (row.Success == 1, row.Message, row.IdVehicle);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, VehiculoSaveModel model)
    {
        var rows = await _context.Database.SqlQueryRaw<VehiculoSpResult>(
            "EXEC dbo.sp_vehicle_update @id_vehicle, @id_vehicle_type, @plate, @maximum_weight, @height, @width, @length",
            new SqlParameter("@id_vehicle", id),
            new SqlParameter("@id_vehicle_type", model.IdVehicleType),
            Param("@plate", model.Plate),
            Param("@maximum_weight", model.MaximumWeight),
            Param("@height", model.Height),
            Param("@width", model.Width),
            Param("@length", model.Length))
            .ToListAsync();

        var row = rows.FirstOrDefault();
        return row is null
            ? (false, "No se pudo actualizar el vehiculo.")
            : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
        => await ExecuteActionAsync("EXEC dbo.sp_vehicle_delete_logic @id_vehicle", id, "No se pudo desactivar el vehiculo.");

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
        => await ExecuteActionAsync("EXEC dbo.sp_vehicle_restore @id_vehicle", id, "No se pudo restaurar el vehiculo.");

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
        => await ExecuteActionAsync("EXEC dbo.sp_vehicle_delete_physical @id_vehicle", id, "No se pudo eliminar el vehiculo.");

    private async Task<VehiculoPagedResult> QueryListAsync(
        bool active,
        string? search,
        int page,
        int pageSize,
        int? vehicleTypeId)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<VehiculoListItem>(
            active
                ? "EXEC dbo.sp_vehicle_list_active @search, @page, @page_size, @id_vehicle_type"
                : "EXEC dbo.sp_vehicle_list_inactive @search, @page, @page_size, @id_vehicle_type",
            Param("@search", search),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize),
            Param("@id_vehicle_type", vehicleTypeId))
            .ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new VehiculoPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdVehicle,
                vehicleTypeId = r.IdVehicleType,
                vehicleTypeName = r.VehicleTypeName,
                plate = r.Plate,
                maximumWeight = r.MaximumWeight,
                height = r.Height,
                width = r.Width,
                length = r.Length,
                maximumVolume = r.MaximumVolume,
                status = r.Status
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    private async Task<(bool Success, string Message)> ExecuteActionAsync(
        string sql,
        int id,
        string defaultMessage)
    {
        var rows = await _context.Database.SqlQueryRaw<VehiculoSpResult>(
            sql,
            new SqlParameter("@id_vehicle", id))
            .ToListAsync();

        var row = rows.FirstOrDefault();
        return row is null ? (false, defaultMessage) : (row.Success == 1, row.Message);
    }

    private static SqlParameter Param(string name, object? value)
        => new(name, value ?? DBNull.Value);
}
