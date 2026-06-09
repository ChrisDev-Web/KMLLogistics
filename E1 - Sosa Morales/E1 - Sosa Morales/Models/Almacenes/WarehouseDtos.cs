using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Almacenes;

public class WarehouseListItem
{
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
    [Column("address")] public string Address { get; set; } = string.Empty;
    [Column("district_name")] public string DistrictName { get; set; } = string.Empty;
    [Column("total_count")] public int TotalCount { get; set; }
}

public class WarehouseDetail
{
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
    [Column("address")] public string Address { get; set; } = string.Empty;
    [Column("id_district")] public int? IdDistrict { get; set; }
    [Column("country_name")] public string CountryName { get; set; } = string.Empty;
    [Column("region_name")] public string RegionName { get; set; } = string.Empty;
    [Column("province_name")] public string ProvinceName { get; set; } = string.Empty;
    [Column("district_name")] public string DistrictName { get; set; } = string.Empty;
    [Column("status")] public byte Status { get; set; }
    [Column("created_at")] public DateTime? CreatedAt { get; set; }
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
}

public class WarehouseDistrictOption
{
    [Column("id_district")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
}

public class WarehouseSpResult
{
    [Column("success")] public int Success { get; set; }
    [Column("message")] public string Message { get; set; } = string.Empty;
    [Column("id_warehouse")] public int? IdWarehouse { get; set; }
}

public class WarehousePagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
