using E1___Sosa_Morales.Models.ProductoProveedores;

namespace E1___Sosa_Morales.Services.ProductoProveedores;

public interface IPrpService
{
    Task<List<PrpListItem>> ListAsync(string? search);
    Task<(bool Success, string Message)> CreateAsync(int pId, int sId, decimal cost, bool main);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}