using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.TiposDocumento;

public class DocumentTypeListItem
{
    [Column("id_document_type")]
    public int IdDocumentType { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class DocumentTypeDetail
{
    [Column("id_document_type")]
    public int IdDocumentType { get; set; }

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

public class DocumentTypeSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_document_type")]
    public int? IdDocumentType { get; set; }
}

public class DocumentTypePagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
