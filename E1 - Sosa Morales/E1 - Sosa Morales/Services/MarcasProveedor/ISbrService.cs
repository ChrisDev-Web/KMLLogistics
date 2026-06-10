using E1___Sosa_Morales.Models.MarcasProveedor;

namespace E1___Sosa_Morales.Services.MarcasProveedor;

public interface ISbrService
{
    /// <summary>
    /// Lista todas las relaciones existentes entre proveedores y marcas.
    /// </summary>
    Task<List<SbrListItem>> ListAsync(string? search);

    /// <summary>
    /// Crea una nueva relación entre un proveedor y una marca.
    /// </summary>
    Task<(bool Success, string Message)> CreateAsync(int idSupplier, int idBrand);

    /// <summary>
    /// Elimina una relación específica entre un proveedor y una marca.
    /// </summary>
    Task<(bool Success, string Message)> DeleteAsync(int idSupplier, int idBrand);
}