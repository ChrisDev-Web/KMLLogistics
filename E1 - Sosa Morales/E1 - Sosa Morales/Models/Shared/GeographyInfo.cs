using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Shared;

public class GeographyInfo
{
    [Column("country_name")]
    public string CountryName { get; set; } = string.Empty;

    [Column("region_name")]
    public string RegionName { get; set; } = string.Empty;

    [Column("province_name")]
    public string ProvinceName { get; set; } = string.Empty;

    [Column("district_name")]
    public string DistrictName { get; set; } = string.Empty;
}
