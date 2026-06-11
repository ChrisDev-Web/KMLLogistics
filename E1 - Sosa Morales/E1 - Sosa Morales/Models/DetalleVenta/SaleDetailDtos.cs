using System.ComponentModel.DataAnnotations.Schema;
using E1___Sosa_Morales.Models.Shared;

namespace E1___Sosa_Morales.Models.DetalleVenta;

public class SaleDetailListItem
{
    [Column("id_sale_detail")] public int IdSaleDetail { get; set; }
    [Column("id_sale")] public int IdSale { get; set; }
    [Column("sale_number")] public string SaleNumber { get; set; } = string.Empty;
    [Column("product_name")] public string ProductName { get; set; } = string.Empty;
    [Column("client_name")] public string ClientName { get; set; } = string.Empty;
    [Column("warehouse_name")] public string WarehouseName { get; set; } = string.Empty;
    [Column("quantity")] public int Quantity { get; set; }
    [Column("unit_price")] public decimal UnitPrice { get; set; }
    [Column("subtotal")] public decimal Subtotal { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("total_count")] public int TotalCount { get; set; }
}

public class SaleDetailMetrics
{
    [Column("sale_count")] public int SaleCount { get; set; }
    [Column("total_subtotal")] public decimal TotalSubtotal { get; set; }
    [Column("total_tax")] public decimal TotalTax { get; set; }
    [Column("total_amount")] public decimal TotalAmount { get; set; }
    [Column("net_profit")] public decimal NetProfit { get; set; }
}

public class SaleDetailPagedResult
{
    public List<SaleDetailListItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
