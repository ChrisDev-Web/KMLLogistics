using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Clientes;

public class ClientListItem
{
    [Column("id_client")]
    public int IdClient { get; set; }

    [Column("document_type_name")]
    public string DocumentTypeName { get; set; } = string.Empty;

    [Column("document_number")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("last_name_paternal")]
    public string LastNamePaternal { get; set; } = string.Empty;

    [Column("last_name_maternal")]
    public string? LastNameMaternal { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("district_name")]
    public string DistrictName { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class ClientDetail
{
    [Column("id_client")]
    public int IdClient { get; set; }

    [Column("id_document_type")]
    public int IdDocumentType { get; set; }

    [Column("document_type_name")]
    public string DocumentTypeName { get; set; } = string.Empty;

    [Column("document_number")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("last_name_paternal")]
    public string LastNamePaternal { get; set; } = string.Empty;

    [Column("last_name_maternal")]
    public string? LastNameMaternal { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("id_district")]
    public int? IdDistrict { get; set; }

    [Column("country_name")]
    public string CountryName { get; set; } = string.Empty;

    [Column("region_name")]
    public string RegionName { get; set; } = string.Empty;

    [Column("province_name")]
    public string ProvinceName { get; set; } = string.Empty;

    [Column("district_name")]
    public string DistrictName { get; set; } = string.Empty;

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class ClientFkOption
{
    [Column("id_document_type")]
    public int? IdDocumentType { get; set; }

    [Column("id_district")]
    public int? IdDistrict { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class ClientDocumentTypeOption
{
    [Column("id_document_type")]
    public int IdDocumentType { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class ClientDistrictOption
{
    [Column("id_district")]
    public int IdDistrict { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class ClientSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_client")]
    public int? IdClient { get; set; }
}

public class ClientPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
