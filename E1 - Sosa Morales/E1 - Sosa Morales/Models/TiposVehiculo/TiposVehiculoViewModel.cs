using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.TiposVehiculo;

public class TiposVehiculoViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}
public class TipoVehiculoListItem
{
    public int IdVehicleType { get; set; }
    public String Name { get; set; } = string.Empty;
    public String? Description { get; set; }
    public int VehicleCount { get; set; }
    public int TotalCount { get; set; }

}



public class TipoVehiculoDetail
{
    public int IdVehicleType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int VehicleCount { get; set; }
    public byte Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TipoVehiculoSaveModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}


public class TipoVehiculoPagedResult
{
    public List<object> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class TipoVehiculoSpResult
{
    public int Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? IdVehicleType { get; set; }
}

