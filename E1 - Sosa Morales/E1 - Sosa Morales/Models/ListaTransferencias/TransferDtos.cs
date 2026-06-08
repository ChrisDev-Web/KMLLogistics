using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.ListaTransferencias;

public class TransferListItem
{
    [Column("id_transfer")]
    public int IdTransfer { get; set; }

    [Column("fec_transfer")]
    public DateTime FecTransfer { get; set; }

    [Column("warehouse_origin_name")]
    public string WarehouseOriginName { get; set; } = string.Empty;

    [Column("warehouse_destination_name")]
    public string WarehouseDestinationName { get; set; } = string.Empty;

    [Column("status_transfer_name")]
    public string StatusTransferName { get; set; } = string.Empty;

    [Column("employee_name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Column("total_count")]
    public int TotalCount { get; set; }
}

public class TransferDetailRecord
{
    [Column("id_transfer")]
    public int IdTransfer { get; set; }

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

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class TransferLineItem
{
    [Column("id_transfer_detail")]
    public int IdTransferDetail { get; set; }

    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }
}

public class TransferOption
{
    [Column("id_warehouse")]
    public int IdWarehouse { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class TransferEmployeeOption
{
    [Column("id_employee")]
    public int IdEmployee { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class TransferStatusOption
{
    [Column("id_status_transfer")]
    public int IdStatusTransfer { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

public class TransferProductOption
{
    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("stock")]
    public int Stock { get; set; }
}

public class TransferSpResult
{
    [Column("success")]
    public int Success { get; set; }

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("id_transfer")]
    public int? IdTransfer { get; set; }
}

public class TransferPagedResult
{
    public List<object> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class TransferLineSaveModel
{
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
}

public class TransferSaveModel
{
    public int IdWarehouseOrigin { get; set; }
    public int IdWarehouseDestination { get; set; }
    public int IdEmployee { get; set; }
    public DateTime FecTransfer { get; set; }
    public List<TransferLineSaveModel> Lines { get; set; } = [];
}
