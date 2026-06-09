using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.MovimientosInventario;

public class InventoryMovementListItem
{
    [Column("id_inventory_movement")] public int IdInventoryMovement { get; set; }
    [Column("id_product")] public int IdProduct { get; set; }
    [Column("product_name")] public string ProductName { get; set; } = string.Empty;
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("warehouse_name")] public string WarehouseName { get; set; } = string.Empty;
    [Column("id_movement_type")] public int IdMovementType { get; set; }
    [Column("movement_type_name")] public string MovementTypeName { get; set; } = string.Empty;
    [Column("movement_direction")] public string MovementDirection { get; set; } = string.Empty;
    [Column("quantity")] public int Quantity { get; set; }
    [Column("reference")] public string? Reference { get; set; }
    [Column("fec_movement")] public DateTime FecMovement { get; set; }
    [Column("employee_name")] public string EmployeeName { get; set; } = string.Empty;
    [Column("total_count")] public int TotalCount { get; set; }
}

public class InventoryMovementDetail
{
    [Column("id_inventory_movement")] public int IdInventoryMovement { get; set; }
    [Column("id_product")] public int IdProduct { get; set; }
    [Column("product_name")] public string ProductName { get; set; } = string.Empty;
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("warehouse_name")] public string WarehouseName { get; set; } = string.Empty;
    [Column("id_movement_type")] public int IdMovementType { get; set; }
    [Column("movement_type_name")] public string MovementTypeName { get; set; } = string.Empty;
    [Column("movement_direction")] public string MovementDirection { get; set; } = string.Empty;
    [Column("quantity")] public int Quantity { get; set; }
    [Column("reference")] public string? Reference { get; set; }
    [Column("fec_movement")] public DateTime FecMovement { get; set; }
    [Column("id_employee")] public int IdEmployee { get; set; }
    [Column("employee_name")] public string EmployeeName { get; set; } = string.Empty;
    [Column("employee_username")] public string EmployeeUsername { get; set; } = string.Empty;
    [Column("created_at")] public DateTime? CreatedAt { get; set; }
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
}

public class InventoryMovementFilterOption
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
}

public class InventoryMovementPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
