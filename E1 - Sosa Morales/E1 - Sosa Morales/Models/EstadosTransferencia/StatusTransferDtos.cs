using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.EstadosTransferencia;

public class StatusTransferListItem
{
    [Column("id_status_transfer")]
    public int IdStatusTransfer { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class StatusTransferDetail
{
    [Column("id_status_transfer")]
    public int IdStatusTransfer { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class StatusTransferSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_status_transfer")]
    public int? IdStatusTransfer { get; set; }
}

public class StatusTransferPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
