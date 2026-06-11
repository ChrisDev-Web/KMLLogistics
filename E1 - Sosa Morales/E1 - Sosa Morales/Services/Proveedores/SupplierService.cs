using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Proveedores;
using E1___Sosa_Morales.Models.Users;
using E1___Sosa_Morales.Services.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace E1___Sosa_Morales.Services.Proveedores;

public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _context;

    public SupplierService(ApplicationDbContext context) => _context = context;

    public Task<SupplierPagedResult> ListActiveAsync(string? search, int? idDocumentType, int? idDistrict, int page, int pageSize)
        => QueryListAsync(true, search, idDocumentType, idDistrict, page, pageSize);

    public Task<SupplierPagedResult> ListInactiveAsync(string? search, int? idDocumentType, int? idDistrict, int page, int pageSize)
        => QueryListAsync(false, search, idDocumentType, idDistrict, page, pageSize);

    public async Task<List<SupplierListItem>> ListActiveForSelectAsync()
    {
        return await _context.Database.SqlQueryRaw<SupplierListItem>(
            "EXEC dbo.sp_supplier_list_active @search, @id_document_type, @id_district, @page, @page_size",
            Param("@search", null),
            Param("@id_document_type", null),
            Param("@id_district", null),
            new SqlParameter("@page", 1),
            new SqlParameter("@page_size", 1000)).ToListAsync();
    }

    public async Task<SupplierDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<SupplierDetail>("EXEC dbo.sp_supplier_get_by_id @id_supplier", new SqlParameter("@id_supplier", id))
            .ToListAsync();
        var item = rows.FirstOrDefault();
        if (item?.IdDistrict is int districtId)
        {
            var geo = await GeographyHelper.GetByDistrictIdAsync(_context, districtId);
            if (geo is not null)
            {
                item.CountryName = geo.CountryName;
                item.RegionName = geo.RegionName;
                item.ProvinceName = geo.ProvinceName;
                if (string.IsNullOrWhiteSpace(item.DistrictName))
                    item.DistrictName = geo.DistrictName;
            }
        }
        return item;
    }

    public async Task<List<SupplierDocumentTypeOption>> GetDocumentTypeOptionsAsync()
        => await _context.Database.SqlQueryRaw<SupplierDocumentTypeOption>("EXEC dbo.sp_supplier_document_type_list_active").ToListAsync();

    public async Task<List<SupplierDistrictOption>> GetDistrictOptionsAsync()
        => await _context.Database.SqlQueryRaw<SupplierDistrictOption>("EXEC dbo.sp_supplier_district_list_active").ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(SupplierSaveModel model)
    {
        var result = await _context.Database.SqlQueryRaw<SupplierSpResult>(
            "EXEC dbo.sp_supplier_create @id_document_type, @document_number, @name, @phone, @email, @address, @id_district",
            Param("@id_document_type", model.IdDocumentType),
            Param("@document_number", model.DocumentNumber),
            Param("@name", model.Name),
            Param("@phone", model.Phone),
            Param("@email", model.Email),
            Param("@address", model.Address),
            Param("@id_district", model.IdDistrict))
            .ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdSupplier);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, SupplierSaveModel model)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_supplier_update @id_supplier, @id_document_type, @document_number, @name, @phone, @email, @address, @id_district",
            new SqlParameter("@id_supplier", id),
            Param("@id_document_type", model.IdDocumentType),
            Param("@document_number", model.DocumentNumber),
            Param("@name", model.Name),
            Param("@phone", model.Phone),
            Param("@email", model.Email),
            Param("@address", model.Address),
            Param("@id_district", model.IdDistrict))
            .ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_supplier_delete_logic @id_supplier", new SqlParameter("@id_supplier", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_supplier_restore @id_supplier", new SqlParameter("@id_supplier", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_supplier_delete_physical @id_supplier", new SqlParameter("@id_supplier", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<SupplierPagedResult> QueryListAsync(bool active, string? search, int? idDocumentType, int? idDistrict, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_supplier_list_active @search, @id_document_type, @id_district, @page, @page_size"
            : "EXEC dbo.sp_supplier_list_inactive @search, @id_document_type, @id_district, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<SupplierListItem>(
            sql,
            Param("@search", search),
            Param("@id_document_type", idDocumentType),
            Param("@id_district", idDistrict),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new SupplierPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdSupplier,
                documentTypeName = r.DocumentTypeName,
                documentNumber = r.DocumentNumber,
                name = r.Name,
                phone = r.Phone ?? "",
                email = r.Email ?? "",
                districtName = r.DistrictName
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