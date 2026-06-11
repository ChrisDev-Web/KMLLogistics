using E1___Sosa_Morales.Models.Proveedores;

namespace E1___Sosa_Morales.Services.Proveedores;

public interface ISupplierService
{
    Task<SupplierPagedResult> ListActiveAsync(string? search, int? idDocumentType, int? idDistrict, int page, int pageSize);
    Task<SupplierPagedResult> ListInactiveAsync(string? search, int? idDocumentType, int? idDistrict, int page, int pageSize);
    Task<List<SupplierListItem>> ListActiveForSelectAsync();
    Task<SupplierDetail?> GetByIdAsync(int id);
    Task<List<SupplierDocumentTypeOption>> GetDocumentTypeOptionsAsync();
    Task<List<SupplierDistrictOption>> GetDistrictOptionsAsync();
    Task<(bool Success, string Message, int? Id)> CreateAsync(SupplierSaveModel model);
    Task<(bool Success, string Message)> UpdateAsync(int id, SupplierSaveModel model);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}

public class SupplierSaveModel
{
    public int IdDocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int? IdDistrict { get; set; }
}
