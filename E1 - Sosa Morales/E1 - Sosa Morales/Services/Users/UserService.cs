using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Roles;
using E1___Sosa_Morales.Models.Users;
using E1___Sosa_Morales.Models.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Users;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var param = new SqlParameter("@username", username);

        var users = await _context.Database
            .SqlQueryRaw<User>(
                "EXEC sp_user_get_by_username @username",
                param)
            .ToListAsync();

        return users.FirstOrDefault();
    }

    public async Task<List<Role>> GetActiveRolesAsync()
    {
        var options = await _context.Database
            .SqlQueryRaw<UsuarioRoleOption>("EXEC dbo.sp_role_list_select_active")
            .ToListAsync();

        return options.Select(o => new Role { IdRole = o.IdRole, Name = o.Name }).ToList();
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string username, string password, int idRole)
    {
        var tempUser = new User { Username = username };
        var hash = _hasher.HashPassword(tempUser, password);

        var usernameParam = new SqlParameter("@username", username);
        var hashParam = new SqlParameter("@password_hash", hash);
        var roleParam = new SqlParameter("@id_role", idRole);

        var result = await _context.Database
            .SqlQueryRaw<SpResult>(
                "EXEC sp_user_create @username, @password_hash, @id_role",
                usernameParam, hashParam, roleParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        if (row is null)
            return (false, "No se pudo registrar el usuario.");

        return (row.Success == 1, row.Message);
    }
}
