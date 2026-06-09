using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.DetalleAlmacen;

public class WarehouseDetailMetrics
{
    [Column("warehouse_count")] public int WarehouseCount { get; set; }
    [Column("total_stock")] public int TotalStock { get; set; }
    [Column("total_cost_value")] public decimal TotalCostValue { get; set; }
    [Column("total_sale_value")] public decimal TotalSaleValue { get; set; }
    [Column("product_count")] public int ProductCount { get; set; }
}

public class WarehouseDetailSummaryItem
{
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("warehouse_name")] public string WarehouseName { get; set; } = string.Empty;
    [Column("address")] public string Address { get; set; } = string.Empty;
    [Column("district_name")] public string DistrictName { get; set; } = string.Empty;
    [Column("product_count")] public int ProductCount { get; set; }
    [Column("total_stock")] public int TotalStock { get; set; }
    [Column("total_cost_value")] public decimal TotalCostValue { get; set; }
    [Column("total_sale_value")] public decimal TotalSaleValue { get; set; }
    [Column("total_count")] public int TotalCount { get; set; }
}

public class WarehouseDetailProductItem
{
    [Column("id_warehouse_detail")] public int IdWarehouseDetail { get; set; }
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("id_product")] public int IdProduct { get; set; }
    [Column("product_name")] public string ProductName { get; set; } = string.Empty;
    [Column("brand_name")] public string BrandName { get; set; } = string.Empty;
    [Column("category_name")] public string CategoryName { get; set; } = string.Empty;
    [Column("stock")] public int Stock { get; set; }
    [Column("location")] public string? Location { get; set; }
    [Column("cost")] public decimal Cost { get; set; }
    [Column("sale_price")] public decimal SalePrice { get; set; }
    [Column("line_cost_value")] public decimal LineCostValue { get; set; }
    [Column("line_sale_value")] public decimal LineSaleValue { get; set; }
    [Column("total_count")] public int TotalCount { get; set; }
}

public class WarehouseDetailHeader
{
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("warehouse_name")] public string WarehouseName { get; set; } = string.Empty;
    [Column("address")] public string Address { get; set; } = string.Empty;
    [Column("district_name")] public string DistrictName { get; set; } = string.Empty;
    [Column("status")] public byte Status { get; set; }
    [Column("created_at")] public DateTime? CreatedAt { get; set; }
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
}

public class WarehouseDetailRecord
{
    [Column("id_warehouse_detail")] public int IdWarehouseDetail { get; set; }
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("warehouse_name")] public string WarehouseName { get; set; } = string.Empty;
    [Column("id_product")] public int IdProduct { get; set; }
    [Column("product_name")] public string ProductName { get; set; } = string.Empty;
    [Column("brand_name")] public string BrandName { get; set; } = string.Empty;
    [Column("category_name")] public string CategoryName { get; set; } = string.Empty;
    [Column("stock")] public int Stock { get; set; }
    [Column("location")] public string? Location { get; set; }
    [Column("cost")] public decimal Cost { get; set; }
    [Column("sale_price")] public decimal SalePrice { get; set; }
    [Column("line_cost_value")] public decimal LineCostValue { get; set; }
    [Column("line_sale_value")] public decimal LineSaleValue { get; set; }
}

public class WarehouseDetailOption
{
    [Column("id_warehouse")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
}

public class WarehouseDetailPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
