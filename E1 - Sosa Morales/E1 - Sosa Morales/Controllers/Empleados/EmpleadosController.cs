using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Empleados;
using E1___Sosa_Morales.Services.Empleados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Empleados;

[Authorize]
public class EmpleadosController : Controller
{
    private readonly IEmployeeService _service;

    public EmpleadosController(IEmployeeService service) => _service = service;

    public IActionResult Index()
    {
        return View(new EmpleadosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Organizacion", "Empleados", "rrhh")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idDocumentType, int? idDistrict, int? idJobPosition, int page = 1, int pageSize = 10)
        => Json(await _service.ListActiveAsync(search, idDocumentType, idDistrict, idJobPosition, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> ListInactive(string? search, int? idDocumentType, int? idDistrict, int? idJobPosition, int page = 1, int pageSize = 10)
        => Json(await _service.ListInactiveAsync(search, idDocumentType, idDistrict, idJobPosition, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
            return Json(new
            {
                success = true,
                data = new
                {
                    id = item.IdEmployee,
                    idUser = item.IdUser,
                    username = item.Username,
                    roleName = item.RoleName,
                    idJobPosition = item.IdJobPosition,
                    jobPositionName = item.JobPositionName,
                    idDocumentType = item.IdDocumentType,
                    documentTypeName = item.DocumentTypeName,
                    documentNumber = item.DocumentNumber,
                    name = item.Name,
                    lastNamePaternal = item.LastNamePaternal,
                    lastNameMaternal = item.LastNameMaternal ?? "",
                    phone = item.Phone ?? "",
                    email = item.Email ?? "",
                    idDistrict = item.IdDistrict,
                    countryName = item.CountryName,
                    regionName = item.RegionName,
                    provinceName = item.ProvinceName,
                    districtName = item.DistrictName,
                    status = item.Status,
                    createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> FilterOptions()
    {
        var docTypes = await _service.GetDocumentTypeOptionsAsync();
        var districts = await _service.GetDistrictOptionsAsync();
        var jobPositions = await _service.GetJobPositionOptionsAsync();
        return Json(new
        {
            success = true,
            documentTypes = docTypes.Select(d => new { id = d.IdDocumentType, name = d.Name }),
            districts = districts.Select(d => new { id = d.IdDistrict, name = d.Name }),
            jobPositions = jobPositions.Select(j => new { id = j.IdJobPosition, name = j.Name })
        });
    }

    [HttpGet]
    public async Task<IActionResult> UserOptions(int? excludeEmployeeId)
    {
        var users = await _service.GetAvailableUserOptionsAsync(excludeEmployeeId);
        return Json(new
        {
            success = true,
            users = users.Select(u => new { id = u.IdUser, name = u.Username })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int idUser, int idJobPosition, int idDocumentType, string documentNumber, string name, string lastNamePaternal, string? lastNameMaternal, string? phone, string? email, int? idDistrict)
    {
        try
        {
            var (success, message, id) = await _service.CreateAsync(BuildSaveModel(idUser, idJobPosition, idDocumentType, documentNumber, name, lastNamePaternal, lastNameMaternal, phone, email, idDistrict));
            return Json(new { success, message, id });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int idUser, int idJobPosition, int idDocumentType, string documentNumber, string name, string lastNamePaternal, string? lastNameMaternal, string? phone, string? email, int? idDistrict)
    {
        try
        {
            var (success, message) = await _service.UpdateAsync(id, BuildSaveModel(idUser, idJobPosition, idDocumentType, documentNumber, name, lastNamePaternal, lastNameMaternal, phone, email, idDistrict));
            return Json(new { success, message });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogic(int id)
    {
        var (success, message) = await _service.DeleteLogicAsync(id);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var (success, message) = await _service.RestoreAsync(id);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhysical(int id)
    {
        var (success, message) = await _service.DeletePhysicalAsync(id);
        return Json(new { success, message });
    }

    private static EmployeeSaveModel BuildSaveModel(int idUser, int idJobPosition, int idDocumentType, string documentNumber, string name, string lastNamePaternal, string? lastNameMaternal, string? phone, string? email, int? idDistrict)
        => new()
        {
            IdUser = idUser,
            IdJobPosition = idJobPosition,
            IdDocumentType = idDocumentType,
            DocumentNumber = documentNumber.Trim(),
            Name = name.Trim(),
            LastNamePaternal = lastNamePaternal.Trim(),
            LastNameMaternal = string.IsNullOrWhiteSpace(lastNameMaternal) ? null : lastNameMaternal.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            IdDistrict = idDistrict
        };
}
