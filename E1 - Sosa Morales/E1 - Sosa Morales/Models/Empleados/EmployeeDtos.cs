using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Empleados;

public class EmployeeListItem
{
    [Column("id_employee")]
    public int IdEmployee { get; set; }

    [Column("user_name")]
    public string UserName { get; set; } = string.Empty;

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

    [Column("job_position_name")]
    public string JobPositionName { get; set; } = string.Empty;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("district_name")]
    public string DistrictName { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class EmployeeDetailRecord
{
    [Column("id_employee")]
    public int IdEmployee { get; set; }

    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [Column("id_job_position")]
    public int IdJobPosition { get; set; }

    [Column("job_position_name")]
    public string JobPositionName { get; set; } = string.Empty;

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

    [Column("id_district")]
    public int? IdDistrict { get; set; }

    [Column("district_name")]
    public string DistrictName { get; set; } = string.Empty;

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class EmployeeDetail
{
    public int IdEmployee { get; set; }
    public int IdUser { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int IdJobPosition { get; set; }
    public string JobPositionName { get; set; } = string.Empty;
    public int IdDocumentType { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastNamePaternal { get; set; } = string.Empty;
    public string? LastNameMaternal { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? IdDistrict { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class EmployeeDocumentTypeOption
{
    [Column("id_document_type")]
    public int IdDocumentType { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class EmployeeDistrictOption
{
    [Column("id_district")]
    public int IdDistrict { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class EmployeeJobPositionOption
{
    [Column("id_job_position")]
    public int IdJobPosition { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class EmployeeUserOption
{
    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;
}

public class EmployeeSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_employee")]
    public int? IdEmployee { get; set; }
}

public class EmployeePagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class EmployeeSaveModel
{
    public int IdUser { get; set; }
    public int IdJobPosition { get; set; }
    public int IdDocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastNamePaternal { get; set; } = string.Empty;
    public string? LastNameMaternal { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? IdDistrict { get; set; }
}
