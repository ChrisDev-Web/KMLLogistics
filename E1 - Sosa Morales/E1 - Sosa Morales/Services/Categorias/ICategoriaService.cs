using E1___Sosa_Morales.Models.Categorias;

namespace E1___Sosa_Morales.Services.Categorias;

public interface ICategoriaService
{
    Task<List<CategoriaListItem>> ListActiveAsync(string? search);
    Task<CategoriaDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string description);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string description);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
    Task<List<CategoriaListItem>> ListInactiveAsync(string? search);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
}