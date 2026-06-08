using E1___Sosa_Morales.Models.Clientes;

namespace E1___Sosa_Morales.Services.Clientes;

public interface IClientService
{
    Task<ClientPagedResult> ListActiveAsync(string? search, int? idDocumentType, int? idDistrict, int page, int pageSize);
    Task<ClientPagedResult> ListInactiveAsync(string? search, int? idDocumentType, int? idDistrict, int page, int pageSize);
    Task<ClientDetail?> GetByIdAsync(int id);
    Task<List<ClientDocumentTypeOption>> GetDocumentTypeOptionsAsync();
    Task<List<ClientDistrictOption>> GetDistrictOptionsAsync();
    Task<(bool Success, string Message, int? Id)> CreateAsync(ClientSaveModel model);
    Task<(bool Success, string Message)> UpdateAsync(int id, ClientSaveModel model);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}

public class ClientSaveModel
{
    public int IdDocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastNamePaternal { get; set; } = string.Empty;
    public string? LastNameMaternal { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int? IdDistrict { get; set; }
}
