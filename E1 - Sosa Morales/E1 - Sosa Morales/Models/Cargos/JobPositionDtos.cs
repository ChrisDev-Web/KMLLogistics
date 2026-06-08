using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Cargos;

public class JobPositionListItem
{
    [Column("id_job_position")]
    public int IdJobPosition { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class JobPositionDetail
{
    [Column("id_job_position")]
    public int IdJobPosition { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class JobPositionSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_job_position")]
    public int? IdJobPosition { get; set; }
}

public class JobPositionPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
