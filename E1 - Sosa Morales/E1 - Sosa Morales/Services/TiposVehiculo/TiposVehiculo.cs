using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.TiposVehiculo;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace E1___Sosa_Morales.Services.TiposVehiculo;

public class TiposVehiculoService : ITiposVehiculoService
{
    private readonly ApplicationDbContext _context;

    public TiposVehiculoService(ApplicationDbContext context) => _context = context;

    public Task<TipoVehiculoPagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<TipoVehiculoPagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<TipoVehiculoDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<TipoVehiculoDetail>(
                "EXEC dbo.sp_vehicle_type_get_by_id @id_vehicle_type",
                new SqlParameter("@id_vehicle_type", id))
            .ToListAsync();

        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(TipoVehiculoSaveModel model)
    {
        var result = await _context.Database.SqlQueryRaw<TipoVehiculoSpResult>(
            "EXEC dbo.sp_vehicle_type_create @name, @description",
            Param("@name", model.Name),
            Param("@description", model.Description))
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdVehicleType);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, TipoVehiculoSaveModel model)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_vehicle_type_update @id_vehicle_type, @name, @description",
            new SqlParameter("@id_vehicle_type", id),
            Param("@name", model.Name),
            Param("@description", model.Description))
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_vehicle_type_delete_logic @id_vehicle_type",
            new SqlParameter("@id_vehicle_type", id)).ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_vehicle_type_restore @id_vehicle_type",
            new SqlParameter("@id_vehicle_type", id)).ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_vehicle_type_delete_physical @id_vehicle_type",
            new SqlParameter("@id_vehicle_type", id)).ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<TipoVehiculoPagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_vehicle_type_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_vehicle_type_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<TipoVehiculoListItem>(
                sql,
                Param("@search", search),
                new SqlParameter("@page", page),
                new SqlParameter("@page_size", pageSize))
            .ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new TipoVehiculoPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdVehicleType,
                name = r.Name,
                description = r.Description ?? "",
                vehicleCount = r.VehicleCount
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    private static SqlParameter Param(string name, object? value)
        => new(name, value ?? DBNull.Value);
}
