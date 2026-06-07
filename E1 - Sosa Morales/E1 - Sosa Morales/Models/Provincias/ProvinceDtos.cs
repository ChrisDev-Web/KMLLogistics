using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Provincias;

public class ProvinceListItem
{
    [Column("id_province")]
    public int IdProvince { get; set; }

    [Column("id_region")]
    public int IdRegion { get; set; }

    [Column("region_name")]
    public string RegionName { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class ProvinceDetail
{
    [Column("id_province")]
    public int IdProvince { get; set; }

    [Column("id_region")]
    public int IdRegion { get; set; }

    [Column("region_name")]
    public string RegionName { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class ProvinceFkOption
{
    [Column("id_region")]
    public int IdRegion { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class ProvinceSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_province")]
    public int? IdProvince { get; set; }
}

public class ProvincePagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
