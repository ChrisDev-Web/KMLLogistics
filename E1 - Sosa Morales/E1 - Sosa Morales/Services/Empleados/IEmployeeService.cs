using E1___Sosa_Morales.Models.Empleados;

namespace E1___Sosa_Morales.Services.Empleados;

public interface IEmployeeService
{
    Task<EmployeePagedResult> ListActiveAsync(string? search, int? idDocumentType, int? idDistrict, int? idJobPosition, int page, int pageSize);
    Task<EmployeePagedResult> ListInactiveAsync(string? search, int? idDocumentType, int? idDistrict, int? idJobPosition, int page, int pageSize);
    Task<EmployeeDetail?> GetByIdAsync(int id);
    Task<List<EmployeeDocumentTypeOption>> GetDocumentTypeOptionsAsync();
    Task<List<EmployeeDistrictOption>> GetDistrictOptionsAsync();
    Task<List<EmployeeJobPositionOption>> GetJobPositionOptionsAsync();
    Task<List<EmployeeUserOption>> GetAvailableUserOptionsAsync(int? excludeEmployeeId);
    Task<(bool Success, string Message, int? Id)> CreateAsync(EmployeeSaveModel model);
    Task<(bool Success, string Message)> UpdateAsync(int id, EmployeeSaveModel model);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
