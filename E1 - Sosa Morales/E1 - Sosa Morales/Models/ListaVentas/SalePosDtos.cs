using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.ListaVentas;

public class PosProductItem
{
    [Column("id_product")] public int IdProduct { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
    [Column("photo")] public string? Photo { get; set; }
    [Column("sale_price")] public decimal SalePrice { get; set; }
    [Column("stock")] public int Stock { get; set; }
    [Column("id_warehouse")] public int IdWarehouse { get; set; }
    [Column("category_name")] public string CategoryName { get; set; } = string.Empty;
    [Column("brand_name")] public string BrandName { get; set; } = string.Empty;
}

public class SaleClientOption
{
    [Column("id_client")] public int IdClient { get; set; }
    [Column("client_name")] public string ClientName { get; set; } = string.Empty;
    [Column("document_type_name")] public string DocumentTypeName { get; set; } = string.Empty;
    [Column("document_number")] public string DocumentNumber { get; set; } = string.Empty;
    [Column("id_document_type")] public int IdDocumentType { get; set; }
}

public class SaleEmployeeInfo
{
    [Column("id_employee")] public int IdEmployee { get; set; }
    [Column("employee_name")] public string EmployeeName { get; set; } = string.Empty;
}

public class PaymentMethodOption
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = string.Empty;
}

public class SaleCreateResult
{
    [Column("success")] public int Success { get; set; }
    [Column("message")] public string Message { get; set; } = string.Empty;
    [Column("id_sale")] public int? IdSale { get; set; }
}

public class SaleVoucherHeader
{
    [Column("id_sale")] public int IdSale { get; set; }
    [Column("sale_number")] public string SaleNumber { get; set; } = string.Empty;
    [Column("receipt_type")] public string ReceiptType { get; set; } = string.Empty;
    [Column("document_type_name")] public string DocumentTypeName { get; set; } = string.Empty;
    [Column("document_number")] public string DocumentNumber { get; set; } = string.Empty;
    [Column("subtotal")] public decimal Subtotal { get; set; }
    [Column("discount")] public decimal Discount { get; set; }
    [Column("tax")] public decimal Tax { get; set; }
    [Column("total")] public decimal Total { get; set; }
    [Column("amount_paid")] public decimal? AmountPaid { get; set; }
    [Column("change_amount")] public decimal? ChangeAmount { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("payment_method_name")] public string PaymentMethodName { get; set; } = string.Empty;
    [Column("client_name")] public string ClientName { get; set; } = string.Empty;
    [Column("employee_name")] public string EmployeeName { get; set; } = string.Empty;
}

public class SaleVoucherLine
{
    [Column("quantity")] public int Quantity { get; set; }
    [Column("product_name")] public string ProductName { get; set; } = string.Empty;
    [Column("unit_price")] public decimal UnitPrice { get; set; }
    [Column("subtotal")] public decimal Subtotal { get; set; }
}

public class SaleVoucherViewModel
{
    public SaleVoucherHeader Header { get; set; } = new();
    public List<SaleVoucherLine> Lines { get; set; } = [];
}

public class SalePosLineDto
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
