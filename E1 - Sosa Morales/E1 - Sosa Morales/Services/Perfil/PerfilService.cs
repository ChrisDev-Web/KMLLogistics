using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Perfil;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Perfil;

public class PerfilService : IPerfilService
{
    private readonly ApplicationDbContext _context;

    public PerfilService(ApplicationDbContext context) => _context = context;

    public async Task<UserProfileDetail?> GetByIdAsync(int idUser)
    {
        var param = new SqlParameter("@id_user", idUser);
        var rows = await _context.Database
            .SqlQueryRaw<UserProfileDetail>("EXEC dbo.sp_user_profile_get_by_id @id_user", param)
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message)> UpdateAccountAsync(int idUser, string username, string? passwordHash)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_user_profile_update @id_user, @username, @password_hash",
            new SqlParameter("@id_user", idUser),
            new SqlParameter("@username", username),
            new SqlParameter("@password_hash", (object?)passwordHash ?? DBNull.Value))
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar el perfil.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> UpdatePhotoAsync(int idUser, string? photoPath)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_user_profile_update_photo @id_user, @photo",
            new SqlParameter("@id_user", idUser),
            new SqlParameter("@photo", (object?)photoPath ?? DBNull.Value))
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar la foto.") : (row.Success == 1, row.Message);
    }
}
