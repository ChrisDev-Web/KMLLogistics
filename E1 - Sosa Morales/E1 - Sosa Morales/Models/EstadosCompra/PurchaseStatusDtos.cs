using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.EstadosCompra;

public class PurchaseStatusListItem
{
    [Column("id_purchase_status")]
    public int IdPurchaseStatus { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class PurchaseStatusDetail
{
    [Column("id_purchase_status")]
    public int IdPurchaseStatus { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("status")]
    public byte Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class PurchaseStatusSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_purchase_status")]
    public int? IdPurchaseStatus { get; set; }
}

public class PurchaseStatusPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
