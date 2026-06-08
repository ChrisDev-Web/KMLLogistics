using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Clientes;
using E1___Sosa_Morales.Models.Users;
using E1___Sosa_Morales.Services.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Clientes;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;

    public ClientService(ApplicationDbContext context) => _context = context;

    public Task<ClientPagedResult> ListActiveAsync(string? search, int? idDocumentType, int? idDistrict, int page, int pageSize)
        => QueryListAsync(true, search, idDocumentType, idDistrict, page, pageSize);

    public Task<ClientPagedResult> ListInactiveAsync(string? search, int? idDocumentType, int? idDistrict, int page, int pageSize)
        => QueryListAsync(false, search, idDocumentType, idDistrict, page, pageSize);

    public async Task<ClientDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<ClientDetail>("EXEC dbo.sp_client_get_by_id @id_client", new SqlParameter("@id_client", id))
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

    public async Task<List<ClientDocumentTypeOption>> GetDocumentTypeOptionsAsync()
        => await _context.Database.SqlQueryRaw<ClientDocumentTypeOption>("EXEC dbo.sp_client_document_type_list_active").ToListAsync();

    public async Task<List<ClientDistrictOption>> GetDistrictOptionsAsync()
        => await _context.Database.SqlQueryRaw<ClientDistrictOption>("EXEC dbo.sp_client_district_list_active").ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(ClientSaveModel model)
    {
        var result = await _context.Database.SqlQueryRaw<ClientSpResult>(
            "EXEC dbo.sp_client_create @id_document_type, @document_number, @name, @last_name_paternal, @last_name_maternal, @phone, @email, @address, @id_district",
            Param("@id_document_type", model.IdDocumentType),
            Param("@document_number", model.DocumentNumber),
            Param("@name", model.Name),
            Param("@last_name_paternal", model.LastNamePaternal),
            Param("@last_name_maternal", model.LastNameMaternal),
            Param("@phone", model.Phone),
            Param("@email", model.Email),
            Param("@address", model.Address),
            Param("@id_district", model.IdDistrict))
            .ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdClient);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, ClientSaveModel model)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_client_update @id_client, @id_document_type, @document_number, @name, @last_name_paternal, @last_name_maternal, @phone, @email, @address, @id_district",
            new SqlParameter("@id_client", id),
            Param("@id_document_type", model.IdDocumentType),
            Param("@document_number", model.DocumentNumber),
            Param("@name", model.Name),
            Param("@last_name_paternal", model.LastNamePaternal),
            Param("@last_name_maternal", model.LastNameMaternal),
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
            "EXEC dbo.sp_client_delete_logic @id_client", new SqlParameter("@id_client", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_client_restore @id_client", new SqlParameter("@id_client", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_client_delete_physical @id_client", new SqlParameter("@id_client", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<ClientPagedResult> QueryListAsync(bool active, string? search, int? idDocumentType, int? idDistrict, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_client_list_active @search, @id_document_type, @id_district, @page, @page_size"
            : "EXEC dbo.sp_client_list_inactive @search, @id_document_type, @id_district, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<ClientListItem>(
            sql,
            Param("@search", search),
            Param("@id_document_type", idDocumentType),
            Param("@id_district", idDistrict),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new ClientPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdClient,
                documentTypeName = r.DocumentTypeName,
                documentNumber = r.DocumentNumber,
                name = r.Name,
                lastNamePaternal = r.LastNamePaternal,
                lastNameMaternal = r.LastNameMaternal ?? "",
                fullName = BuildFullName(r.Name, r.LastNamePaternal, r.LastNameMaternal),
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

    private static string BuildFullName(string name, string paternal, string? maternal)
    {
        var full = $"{name} {paternal}".Trim();
        if (!string.IsNullOrWhiteSpace(maternal)) full += $" {maternal}";
        return full.Trim();
    }

    private static SqlParameter Param(string name, object? value)
        => new(name, value ?? DBNull.Value);
}
