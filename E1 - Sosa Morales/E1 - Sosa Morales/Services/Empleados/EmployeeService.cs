using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Empleados;
using E1___Sosa_Morales.Models.Users;
using E1___Sosa_Morales.Services.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Empleados;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context) => _context = context;

    public Task<EmployeePagedResult> ListActiveAsync(string? search, int? idDocumentType, int? idDistrict, int? idJobPosition, int page, int pageSize)
        => QueryListAsync(true, search, idDocumentType, idDistrict, idJobPosition, page, pageSize);

    public Task<EmployeePagedResult> ListInactiveAsync(string? search, int? idDocumentType, int? idDistrict, int? idJobPosition, int page, int pageSize)
        => QueryListAsync(false, search, idDocumentType, idDistrict, idJobPosition, page, pageSize);

    public async Task<EmployeeDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<EmployeeDetailRecord>("EXEC dbo.sp_employee_get_by_id @id_employee", new SqlParameter("@id_employee", id))
            .ToListAsync();
        var row = rows.FirstOrDefault();
        if (row is null) return null;

        var item = new EmployeeDetail
        {
            IdEmployee = row.IdEmployee,
            IdUser = row.IdUser,
            Username = row.Username,
            RoleName = row.RoleName,
            IdJobPosition = row.IdJobPosition,
            JobPositionName = row.JobPositionName,
            IdDocumentType = row.IdDocumentType,
            DocumentTypeName = row.DocumentTypeName,
            DocumentNumber = row.DocumentNumber,
            Name = row.Name,
            LastNamePaternal = row.LastNamePaternal,
            LastNameMaternal = row.LastNameMaternal,
            Phone = row.Phone,
            Email = row.Email,
            IdDistrict = row.IdDistrict,
            DistrictName = row.DistrictName,
            Status = row.Status,
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt
        };

        if (item.IdDistrict is int districtId)
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

    public async Task<List<EmployeeDocumentTypeOption>> GetDocumentTypeOptionsAsync()
        => await _context.Database.SqlQueryRaw<EmployeeDocumentTypeOption>("EXEC dbo.sp_employee_document_type_list_active").ToListAsync();

    public async Task<List<EmployeeDistrictOption>> GetDistrictOptionsAsync()
        => await _context.Database.SqlQueryRaw<EmployeeDistrictOption>("EXEC dbo.sp_employee_district_list_active").ToListAsync();

    public async Task<List<EmployeeJobPositionOption>> GetJobPositionOptionsAsync()
        => await _context.Database.SqlQueryRaw<EmployeeJobPositionOption>("EXEC dbo.sp_employee_job_position_list_active").ToListAsync();

    public async Task<List<EmployeeUserOption>> GetAvailableUserOptionsAsync(int? excludeEmployeeId)
        => await _context.Database.SqlQueryRaw<EmployeeUserOption>(
            "EXEC dbo.sp_employee_user_list_available @exclude_employee_id",
            Param("@exclude_employee_id", excludeEmployeeId)).ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(EmployeeSaveModel model)
    {
        var result = await _context.Database.SqlQueryRaw<EmployeeSpResult>(
            "EXEC dbo.sp_employee_create @id_user, @id_job_position, @id_document_type, @document_number, @name, @last_name_paternal, @last_name_maternal, @phone, @email, @id_district",
            Param("@id_user", model.IdUser),
            Param("@id_job_position", model.IdJobPosition),
            Param("@id_document_type", model.IdDocumentType),
            Param("@document_number", model.DocumentNumber),
            Param("@name", model.Name),
            Param("@last_name_paternal", model.LastNamePaternal),
            Param("@last_name_maternal", model.LastNameMaternal),
            Param("@phone", model.Phone),
            Param("@email", model.Email),
            Param("@id_district", model.IdDistrict)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdEmployee);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, EmployeeSaveModel model)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_employee_update @id_employee, @id_user, @id_job_position, @id_document_type, @document_number, @name, @last_name_paternal, @last_name_maternal, @phone, @email, @id_district",
            new SqlParameter("@id_employee", id),
            Param("@id_user", model.IdUser),
            Param("@id_job_position", model.IdJobPosition),
            Param("@id_document_type", model.IdDocumentType),
            Param("@document_number", model.DocumentNumber),
            Param("@name", model.Name),
            Param("@last_name_paternal", model.LastNamePaternal),
            Param("@last_name_maternal", model.LastNameMaternal),
            Param("@phone", model.Phone),
            Param("@email", model.Email),
            Param("@id_district", model.IdDistrict)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_employee_delete_logic @id_employee", new SqlParameter("@id_employee", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_employee_restore @id_employee", new SqlParameter("@id_employee", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_employee_delete_physical @id_employee", new SqlParameter("@id_employee", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<EmployeePagedResult> QueryListAsync(bool active, string? search, int? idDocumentType, int? idDistrict, int? idJobPosition, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_employee_list_active @search, @id_document_type, @id_district, @id_job_position, @page, @page_size"
            : "EXEC dbo.sp_employee_list_inactive @search, @id_document_type, @id_district, @id_job_position, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<EmployeeListItem>(
            sql,
            Param("@search", search),
            Param("@id_document_type", idDocumentType),
            Param("@id_district", idDistrict),
            Param("@id_job_position", idJobPosition),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new EmployeePagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdEmployee,
                userName = r.UserName,
                documentTypeName = r.DocumentTypeName,
                documentNumber = r.DocumentNumber,
                fullName = BuildFullName(r.Name, r.LastNamePaternal, r.LastNameMaternal),
                jobPositionName = r.JobPositionName,
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
