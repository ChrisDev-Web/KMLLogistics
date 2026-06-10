using E1___Sosa_Morales.Models.Productos;

namespace E1___Sosa_Morales.Services.Productos;

public interface IProductoService
{
    Task<List<ProductoListItem>> ListActiveAsync(string? search);
    Task<List<ProductoListItem>> ListInactiveAsync(string? search);
    Task<ProductoDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(ProductoDetail dto);
    Task<(bool Success, string Message)> UpdateAsync(ProductoDetail dto);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}