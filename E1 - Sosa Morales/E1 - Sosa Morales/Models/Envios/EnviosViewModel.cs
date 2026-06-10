using E1___Sosa_Morales.Models.Dashboard;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Envios;

public class EnviosViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class ShipmentListItem
{
    public int IdShipment { get; set; }
    public int IdVehicle { get; set; }
    public string VehiclePlate { get; set; } = "";
    public string VehicleTypeName { get; set; } = "";
    public int IdEmployee { get; set; }
    public string EmployeeName { get; set; } = "";
    public int IdShipmentStatus { get; set; }
    public string ShipmentStatusName { get; set; } = "";
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    [NotMapped] public decimal? UsedWeight { get; set; }
    [NotMapped] public decimal? MaximumWeight { get; set; }
    [NotMapped] public decimal? UsedVolume { get; set; }
    [NotMapped] public decimal? MaximumVolume { get; set; }
    public int TotalCount { get; set; }
}

public class ShipmentDetail
{
    public int IdShipment { get; set; }
    public int IdVehicle { get; set; }
    public string VehiclePlate { get; set; } = "";
    public string VehicleTypeName { get; set; } = "";
    public int IdEmployee { get; set; }
    public string EmployeeName { get; set; } = "";
    public int IdShipmentStatus { get; set; }
    public string ShipmentStatusName { get; set; } = "";
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    [NotMapped] public decimal? UsedWeight { get; set; }
    [NotMapped] public decimal? MaximumWeight { get; set; }
    [NotMapped] public decimal? UsedVolume { get; set; }
    [NotMapped] public decimal? MaximumVolume { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ShipmentOption { public int IdShipment { get; set; } public string Name { get; set; } = ""; }
public class ShipmentSpResult { public int Success { get; set; } public string Message { get; set; } = ""; public int? IdShipment { get; set; } }
public class ShipmentActionResult { public int Success { get; set; } public string Message { get; set; } = ""; }
public class ShipmentVehicleOption { public int IdVehicle { get; set; } public string Name { get; set; } = ""; }
public class ShipmentEmployeeOption { public int IdEmployee { get; set; } public string Name { get; set; } = ""; }
public class ShipmentCapacitySummary { public int IdShipment { get; set; } public decimal? UsedWeight { get; set; } public decimal? MaximumWeight { get; set; } public decimal? UsedVolume { get; set; } public decimal? MaximumVolume { get; set; } }
