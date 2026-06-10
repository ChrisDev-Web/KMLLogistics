using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Productos;

public class ProductoListItem
{
    [Column("id_product")] public int IdProduct { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
    [Column("category_name")] public string CategoryName { get; set; } = string.Empty;
    [Column("brand_name")] public string BrandName { get; set; } = string.Empty;
    [Column("cost")] public decimal Cost { get; set; }
    [Column("profit_percentage")] public decimal ProfitPercentage { get; set; }
    [Column("sale_price")] public decimal SalePrice { get; set; }
    [Column("status")] public byte Status { get; set; }
}

public class ProductoDetail
{
    [Column("id_product")] public int IdProduct { get; set; }
    [Column("id_category")] public int IdCategory { get; set; }
    [Column("id_brand")] public int IdBrand { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
    [Column("description")] public string? Description { get; set; }
    [Column("cost")] public decimal Cost { get; set; }
    [Column("profit_percentage")] public decimal ProfitPercentage { get; set; }
    [Column("sale_price")] public decimal SalePrice { get; set; }
    [Column("weight")] public decimal? Weight { get; set; }
    [Column("height")] public decimal? Height { get; set; }
    [Column("width")] public decimal? Width { get; set; }
    [Column("length")] public decimal? Length { get; set; }
    [Column("volume")] public decimal? Volume { get; set; }
    [Column("status")] public byte Status { get; set; }
    [Column("created_at")] public DateTime? CreatedAt { get; set; }
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
}

public class ProductoSpResult
{
    [Column("success")] public int Success { get; set; }
    [Column("message")] public string Message { get; set; } = string.Empty;
    [Column("id_product")] public int? IdProduct { get; set; }
}