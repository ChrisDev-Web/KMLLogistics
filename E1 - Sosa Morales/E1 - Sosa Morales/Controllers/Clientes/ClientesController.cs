using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Clientes;
using E1___Sosa_Morales.Services.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Clientes;

[Authorize]
public class ClientesController : Controller
{
    private readonly IClientService _service;

    public ClientesController(IClientService service) => _service = service;

    public IActionResult Index()
    {
        return View(new ClientesViewModel
        {
            Module = ModuleRegistry.BuildModuleView("PersonasTerceros", "Clientes", "clientes")
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idDocumentType, int? idDistrict, int page = 1, int pageSize = 10)
        => Json(await _service.ListActiveAsync(search, idDocumentType, idDistrict, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> ListInactive(string? search, int? idDocumentType, int? idDistrict, int page = 1, int pageSize = 10)
        => Json(await _service.ListInactiveAsync(search, idDocumentType, idDistrict, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdClient,
                idDocumentType = item.IdDocumentType,
                documentTypeName = item.DocumentTypeName,
                documentNumber = item.DocumentNumber,
                name = item.Name,
                lastNamePaternal = item.LastNamePaternal,
                lastNameMaternal = item.LastNameMaternal ?? "",
                phone = item.Phone ?? "",
                email = item.Email ?? "",
                address = item.Address ?? "",
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

    [HttpGet]
    public async Task<IActionResult> FilterOptions()
    {
        var docTypes = await _service.GetDocumentTypeOptionsAsync();
        var districts = await _service.GetDistrictOptionsAsync();
        return Json(new
        {
            success = true,
            documentTypes = docTypes.Select(d => new { id = d.IdDocumentType, name = d.Name }),
            districts = districts.Select(d => new { id = d.IdDistrict, name = d.Name })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int idDocumentType, string documentNumber, string name, string lastNamePaternal, string? lastNameMaternal, string? phone, string? email, string? address, int? idDistrict)
    {
        try
        {
            var (success, message, id) = await _service.CreateAsync(BuildSaveModel(idDocumentType, documentNumber, name, lastNamePaternal, lastNameMaternal, phone, email, address, idDistrict));
            return Json(new { success, message, id });
        }
        catch (Exception ex) { return Json(new { success = false, message = "Error: " + ex.Message }); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int idDocumentType, string documentNumber, string name, string lastNamePaternal, string? lastNameMaternal, string? phone, string? email, string? address, int? idDistrict)
    {
        try
        {
            var (success, message) = await _service.UpdateAsync(id, BuildSaveModel(idDocumentType, documentNumber, name, lastNamePaternal, lastNameMaternal, phone, email, address, idDistrict));
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

    private static ClientSaveModel BuildSaveModel(int idDocumentType, string documentNumber, string name, string lastNamePaternal, string? lastNameMaternal, string? phone, string? email, string? address, int? idDistrict)
        => new()
        {
            IdDocumentType = idDocumentType,
            DocumentNumber = documentNumber.Trim(),
            Name = name.Trim(),
            LastNamePaternal = lastNamePaternal.Trim(),
            LastNameMaternal = string.IsNullOrWhiteSpace(lastNameMaternal) ? null : lastNameMaternal.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
            IdDistrict = idDistrict
        };
}
