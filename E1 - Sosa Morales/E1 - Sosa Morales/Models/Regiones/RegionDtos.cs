using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Regiones;

public class RegionListItem
{
    [Column("id_region")]
    public int IdRegion { get; set; }

    [Column("id_country")]
    public int IdCountry { get; set; }

    [Column("country_name")]
    public string CountryName { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class RegionDetail
{
    [Column("id_region")]
    public int IdRegion { get; set; }

    [Column("id_country")]
    public int IdCountry { get; set; }

    [Column("country_name")]
    public string CountryName { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class RegionFkOption
{
    [Column("id_country")]
    public int IdCountry { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class RegionSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_region")]
    public int? IdRegion { get; set; }
}

public class RegionPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
