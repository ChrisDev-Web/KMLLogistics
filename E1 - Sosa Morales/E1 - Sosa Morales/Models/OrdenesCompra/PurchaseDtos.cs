using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.OrdenesCompra;

public class PurchaseListItem
{
    [Column("id_purchase")]
    public int IdPurchase { get; set; }

    [Column("fec_purchase")]
    public DateTime FecPurchase { get; set; }

    [Column("supplier_name")]
    public string SupplierName { get; set; } = string.Empty;

    [Column("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Column("purchase_status_name")]
    public string PurchaseStatusName { get; set; } = string.Empty;

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("tax")]
    public decimal Tax { get; set; }

    [Column("total")]
    public decimal Total { get; set; }

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class PurchaseDetailRecord
{
    [Column("id_purchase")]
    public int IdPurchase { get; set; }

    [Column("id_supplier")]
    public int IdSupplier { get; set; }

    [Column("supplier_name")]
    public string SupplierName { get; set; } = string.Empty;

    [Column("id_employee")]
    public int IdEmployee { get; set; }

    [Column("employee_username")]
    public string EmployeeUsername { get; set; } = string.Empty;

    [Column("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Column("id_purchase_status")]
    public int IdPurchaseStatus { get; set; }

    [Column("purchase_status_name")]
    public string PurchaseStatusName { get; set; } = string.Empty;

    [Column("fec_purchase")]
    public DateTime FecPurchase { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("tax")]
    public decimal Tax { get; set; }

    [Column("total")]
    public decimal Total { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class PurchaseLineItem
{
    [Column("id_purchase_detail")]
    public int IdPurchaseDetail { get; set; }

    [Column("id_product_supplier")]
    public int IdProductSupplier { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit_cost")]
    public decimal UnitCost { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }
}

public class PurchaseWarehouseLineItem
{
    [Column("id_purchase_warehouse_detail")]
    public int IdPurchaseWarehouseDetail { get; set; }

    [Column("id_purchase_detail")]
    public int IdPurchaseDetail { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("id_warehouse")]
    public int IdWarehouse { get; set; }

    [Column("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }
}

public class PurchaseOption
{
    [Column("id_warehouse")]
    public int IdWarehouse { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class PurchaseSupplierOption
{
    [Column("id_supplier")]
    public int IdSupplier { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class PurchaseEmployeeOption
{
    [Column("id_employee")]
    public int IdEmployee { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class PurchaseStatusOption
{
    [Column("id_purchase_status")]
    public int IdPurchaseStatus { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class PurchaseProductSupplierOption
{
    [Column("id_product_supplier")]
    public int IdProductSupplier { get; set; }

    [Column("product_name")]
    public string Name { get; set; } = string.Empty;

    [Column("supplier_cost")]
    public decimal SupplierCost { get; set; }
}

public class PurchaseSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_purchase")]
    public int? IdPurchase { get; set; }
}

public class PurchasePagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PurchaseLineSaveModel
{
    public int IdProductSupplier { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public int IdWarehouse { get; set; }
}

public class PurchaseSaveModel
{
    public int IdSupplier { get; set; }
    public int IdEmployee { get; set; }
    public DateTime FecPurchase { get; set; }
    public List<PurchaseLineSaveModel> Lines { get; set; } = [];
}
