using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.DetalleTransferencia;

public class TransferDetailListItem
{
    [Column("id_transfer_detail")]
    public int IdTransferDetail { get; set; }

    [Column("id_transfer")]
    public int IdTransfer { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("warehouse_origin_name")]
    public string WarehouseOriginName { get; set; } = string.Empty;

    [Column("warehouse_destination_name")]
    public string WarehouseDestinationName { get; set; } = string.Empty;

    [Column("status_transfer_name")]
    public string StatusTransferName { get; set; } = string.Empty;

    [Column("fec_transfer")]
    public DateTime FecTransfer { get; set; }

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class TransferDetailItem
{
    [Column("id_transfer_detail")]
    public int IdTransferDetail { get; set; }

    [Column("id_transfer")]
    public int IdTransfer { get; set; }

    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("id_warehouse_origin")]
    public int IdWarehouseOrigin { get; set; }

    [Column("warehouse_origin_name")]
    public string WarehouseOriginName { get; set; } = string.Empty;

    [Column("id_warehouse_destination")]
    public int IdWarehouseDestination { get; set; }

    [Column("warehouse_destination_name")]
    public string WarehouseDestinationName { get; set; } = string.Empty;

    [Column("id_status_transfer")]
    public int IdStatusTransfer { get; set; }

    [Column("status_transfer_name")]
    public string StatusTransferName { get; set; } = string.Empty;

    [Column("fec_transfer")]
    public DateTime FecTransfer { get; set; }

    [Column("id_employee")]
    public int IdEmployee { get; set; }

    [Column("employee_username")]
    public string EmployeeUsername { get; set; } = string.Empty;

    [Column("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Column("transfer_created_at")]
    public DateTime? TransferCreatedAt { get; set; }
}

public class TransferDetailFilterOption
{
    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("id_warehouse")]
    public int IdWarehouse { get; set; }

    [Column("id_status_transfer")]
    public int IdStatusTransfer { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class TransferDetailPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
