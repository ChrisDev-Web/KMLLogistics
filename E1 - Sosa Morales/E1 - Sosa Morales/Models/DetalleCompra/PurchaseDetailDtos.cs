using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.DetalleCompra;

public class PurchaseDetailListItem
{
    [Column("id_purchase_detail")]
    public int IdPurchaseDetail { get; set; }

    [Column("id_purchase")]
    public int IdPurchase { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit_cost")]
    public decimal UnitCost { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("supplier_name")]
    public string SupplierName { get; set; } = string.Empty;

    [Column("purchase_status_name")]
    public string PurchaseStatusName { get; set; } = string.Empty;

    [Column("fec_purchase")]
    public DateTime FecPurchase { get; set; }

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class PurchaseDetailItem
{
    [Column("id_purchase_detail")]
    public int IdPurchaseDetail { get; set; }

    [Column("id_purchase")]
    public int IdPurchase { get; set; }

    [Column("id_product_supplier")]
    public int IdProductSupplier { get; set; }

    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit_cost")]
    public decimal UnitCost { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("id_supplier")]
    public int IdSupplier { get; set; }

    [Column("supplier_name")]
    public string SupplierName { get; set; } = string.Empty;

    [Column("id_purchase_status")]
    public int IdPurchaseStatus { get; set; }

    [Column("purchase_status_name")]
    public string PurchaseStatusName { get; set; } = string.Empty;

    [Column("fec_purchase")]
    public DateTime FecPurchase { get; set; }

    [Column("id_employee")]
    public int IdEmployee { get; set; }

    [Column("employee_username")]
    public string EmployeeUsername { get; set; } = string.Empty;

    [Column("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Column("purchase_created_at")]
    public DateTime? PurchaseCreatedAt { get; set; }
}

public class PurchaseDetailFilterOption
{
    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("id_supplier")]
    public int IdSupplier { get; set; }

    [Column("id_purchase_status")]
    public int IdPurchaseStatus { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class PurchaseDetailPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
