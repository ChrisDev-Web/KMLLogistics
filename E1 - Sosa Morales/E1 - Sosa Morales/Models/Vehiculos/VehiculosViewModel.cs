using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.Vehiculos;

public class VehiculosViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class VehiculoListItem
{
    public int IdVehicle { get; set; }
    public int IdVehicleType { get; set; }
    public string VehicleTypeName { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public decimal? MaximumWeight { get; set; }
    public decimal? MaximumVolume { get; set; }
    public byte Status { get; set; }
    public int TotalCount { get; set; }
}

public class VehiculoDetail
{
    public int IdVehicle { get; set; }
    public int IdVehicleType { get; set; }
    public string VehicleTypeName { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public decimal? MaximumWeight { get; set; }
    public decimal? MaximumVolume { get; set; }
    public byte Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class VehiculoSaveModel
{
    public int IdVehicleType { get; set; }
    public string Plate { get; set; } = string.Empty;
    public decimal? MaximumWeight { get; set; }
    public decimal? MaximumVolume { get; set; }
}

public class VehiculoTypeOption
{
    public int IdVehicleType { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class VehiculoPagedResult
{
    public List<object> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class VehiculoSpResult
{
    public int Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int IdVehicle { get; set; }
}
