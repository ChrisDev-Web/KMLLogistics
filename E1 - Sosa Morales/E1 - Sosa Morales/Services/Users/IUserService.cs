using E1___Sosa_Morales.Models.Roles;
using E1___Sosa_Morales.Models.Users;

namespace E1___Sosa_Morales.Services.Users;

public interface IUserService
{
    Task<User?> GetByUsernameAsync(string username);
    Task<List<Role>> GetActiveRolesAsync();
    Task<(bool Success, string Message)> RegisterAsync(string username, string password, int idRole);
}
