using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Users;
using E1___Sosa_Morales.Models.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Usuarios;

public class UsuarioService : IUsuarioService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public UsuarioService(ApplicationDbContext context) => _context = context;

    public async Task<UsuarioPagedResult> ListActiveAsync(string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<UsuarioListItem>(
            "EXEC dbo.sp_user_list_active @search, @page, @page_size",
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new UsuarioPagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdUser, username = r.Username, roleName = r.RoleName }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<UsuarioDetail?> GetByIdAsync(int id)
    {
        var param = new SqlParameter("@id_user", id);
        var rows = await _context.Database
            .SqlQueryRaw<UsuarioDetail>("EXEC dbo.sp_user_get_by_id @id_user", param)
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<UsuarioRoleOption>> GetRoleOptionsAsync()
    {
        return await _context.Database
            .SqlQueryRaw<UsuarioRoleOption>("EXEC dbo.sp_user_role_list_active")
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string username, string password, int idRole)
    {
        var tempUser = new User { Username = username };
        var hash = _hasher.HashPassword(tempUser, password);

        var result = await _context.Database.SqlQueryRaw<UsuarioSpResult>(
            "EXEC dbo.sp_user_create @username, @password_hash, @id_role",
            new SqlParameter("@username", username),
            new SqlParameter("@password_hash", hash),
            new SqlParameter("@id_role", idRole))
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el usuario.", null) : (row.Success == 1, row.Message, row.IdUser);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string username, int idRole, string? password)
    {
        string? hash = null;
        if (!string.IsNullOrWhiteSpace(password))
        {
            var tempUser = new User { Username = username };
            hash = _hasher.HashPassword(tempUser, password);
        }

        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_user_update @id_user, @username, @id_role, @password_hash",
            new SqlParameter("@id_user", id),
            new SqlParameter("@username", username),
            new SqlParameter("@id_role", idRole),
            new SqlParameter("@password_hash", (object?)hash ?? DBNull.Value))
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_user_delete_physical @id_user",
            new SqlParameter("@id_user", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }
}
