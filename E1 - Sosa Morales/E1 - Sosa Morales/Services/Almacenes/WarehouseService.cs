using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Almacenes;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Almacenes;

public class WarehouseService : IWarehouseService
{
    private readonly ApplicationDbContext _context;

    public WarehouseService(ApplicationDbContext context) => _context = context;

    public Task<WarehousePagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<WarehousePagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<WarehouseDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<WarehouseDetail>("EXEC dbo.sp_warehouse_get_by_id @id_warehouse", new SqlParameter("@id_warehouse", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<WarehouseDistrictOption>> GetDistrictOptionsAsync()
        => await _context.Database.SqlQueryRaw<WarehouseDistrictOption>("EXEC dbo.sp_warehouse_district_list_active").ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string address, int? idDistrict)
    {
        var result = await _context.Database.SqlQueryRaw<WarehouseSpResult>(
            "EXEC dbo.sp_warehouse_create @name, @address, @id_district",
            new SqlParameter("@name", name),
            new SqlParameter("@address", address),
            Param("@id_district", idDistrict)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdWarehouse);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string address, int? idDistrict)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_warehouse_update @id_warehouse, @name, @address, @id_district",
            new SqlParameter("@id_warehouse", id),
            new SqlParameter("@name", name),
            new SqlParameter("@address", address),
            Param("@id_district", idDistrict)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_warehouse_delete_logic @id_warehouse", new SqlParameter("@id_warehouse", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_warehouse_restore @id_warehouse", new SqlParameter("@id_warehouse", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_warehouse_delete_physical @id_warehouse", new SqlParameter("@id_warehouse", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<WarehousePagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_warehouse_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_warehouse_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<WarehouseListItem>(
            sql, Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new WarehousePagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdWarehouse,
                name = r.Name,
                address = r.Address,
                districtName = r.DistrictName
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
