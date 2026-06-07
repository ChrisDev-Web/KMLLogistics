using E1___Sosa_Morales.Models.Usuarios;

namespace E1___Sosa_Morales.Services.Usuarios;

public interface IUsuarioService
{
    Task<UsuarioPagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<UsuarioDetail?> GetByIdAsync(int id);
    Task<List<UsuarioRoleOption>> GetRoleOptionsAsync();
    Task<(bool Success, string Message, int? Id)> CreateAsync(string username, string password, int idRole);
    Task<(bool Success, string Message)> UpdateAsync(int id, string username, int idRole, string? password);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
