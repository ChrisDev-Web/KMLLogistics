using E1___Sosa_Morales.Models.Perfil;

namespace E1___Sosa_Morales.Services.Perfil;

public interface IPerfilService
{
    Task<UserProfileDetail?> GetByIdAsync(int idUser);
    Task<(bool Success, string Message)> UpdateAccountAsync(int idUser, string username, string? passwordHash);
    Task<(bool Success, string Message)> UpdatePhotoAsync(int idUser, string? photoPath);
}
