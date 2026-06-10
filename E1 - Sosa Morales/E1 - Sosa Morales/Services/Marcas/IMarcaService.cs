using E1___Sosa_Morales.Models.Marcas;

public interface IMarcaService
{
    Task<List<MarcaListItem>> ListActiveAsync(string? search);
    Task<List<MarcaListItem>> ListInactiveAsync(string? search);
    Task<MarcaDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string description);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string description);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}

