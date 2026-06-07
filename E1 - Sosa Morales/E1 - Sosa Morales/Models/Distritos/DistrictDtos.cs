using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Distritos;

public class DistrictListItem
{
    [Column("id_district")]
    public int IdDistrict { get; set; }

    [Column("id_province")]
    public int IdProvince { get; set; }

    [Column("province_name")]
    public string ProvinceName { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class DistrictDetail
{
    [Column("id_district")]
    public int IdDistrict { get; set; }

    [Column("id_province")]
    public int IdProvince { get; set; }

    [Column("province_name")]
    public string ProvinceName { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class DistrictFkOption
{
    [Column("id_province")]
    public int IdProvince { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class DistrictSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_district")]
    public int? IdDistrict { get; set; }
}

public class DistrictPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
