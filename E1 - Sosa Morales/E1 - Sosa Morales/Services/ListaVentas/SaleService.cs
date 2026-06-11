using System.Text.Json;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.ListaVentas;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.ListaVentas;

public class SaleService : ISaleService
{
    private readonly ApplicationDbContext _context;

    public SaleService(ApplicationDbContext context) => _context = context;

    public async Task<List<PosProductItem>> GetPosProductsAsync(string? search, int? idCategory, int? idBrand)
    {
        var parameters = new object[]
        {
            Param("@search", search),
            Param("@id_category", idCategory),
            Param("@id_brand", idBrand)
        };
        return await _context.Database
            .SqlQueryRaw<PosProductItem>("EXEC dbo.sp_sale_pos_product_list @search, @id_category, @id_brand", parameters)
            .ToListAsync();
    }

    public async Task<List<SaleClientOption>> GetClientsAsync()
        => await _context.Database.SqlQueryRaw<SaleClientOption>("EXEC dbo.sp_sale_client_list_active").ToListAsync();

    public async Task<List<PaymentMethodOption>> GetPaymentMethodsAsync()
        => await _context.Database.SqlQueryRaw<PaymentMethodOption>("EXEC dbo.sp_sale_payment_method_list_active").ToListAsync();

    public async Task<SaleEmployeeInfo?> GetEmployeeByUserAsync(int idUser)
    {
        var rows = await _context.Database
            .SqlQueryRaw<SaleEmployeeInfo>("EXEC dbo.sp_sale_employee_get_by_user @id_user", new SqlParameter("@id_user", idUser))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? IdSale)> CreateSaleAsync(
        int idClient, int idEmployee, int idPaymentMethod, string receiptType,
        string documentTypeName, string documentNumber,
        decimal subtotal, decimal discount, decimal tax, decimal total,
        decimal? amountPaid, decimal? changeAmount, string detailsJson)
    {
        var parameters = new object[]
        {
            new SqlParameter("@id_client", idClient),
            new SqlParameter("@id_employee", idEmployee),
            new SqlParameter("@id_payment_method", idPaymentMethod),
            new SqlParameter("@receipt_type", receiptType),
            new SqlParameter("@document_type_name", documentTypeName),
            new SqlParameter("@document_number", documentNumber),
            new SqlParameter("@subtotal", subtotal),
            new SqlParameter("@discount", discount),
            new SqlParameter("@tax", tax),
            new SqlParameter("@total", total),
            Param("@amount_paid", amountPaid),
            Param("@change_amount", changeAmount),
            new SqlParameter("@details_json", detailsJson)
        };

        var result = await _context.Database.SqlQueryRaw<SaleCreateResult>(
            "EXEC dbo.sp_sale_create @id_client, @id_employee, @id_payment_method, @receipt_type, @document_type_name, @document_number, @subtotal, @discount, @tax, @total, @amount_paid, @change_amount, @details_json",
            parameters).ToListAsync();

        var row = result.FirstOrDefault();
        return row is null ? (false, "Error al registrar la venta.", null) : (row.Success == 1, row.Message, row.IdSale);
    }

    public async Task<SaleVoucherViewModel?> GetVoucherAsync(int idSale)
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "EXEC dbo.sp_sale_get_voucher @id_sale";
        command.Parameters.Add(new SqlParameter("@id_sale", idSale));

        using var reader = await command.ExecuteReaderAsync();
        SaleVoucherHeader? header = null;
        var lines = new List<SaleVoucherLine>();

        if (await reader.ReadAsync())
        {
            header = new SaleVoucherHeader
            {
                IdSale = reader.GetInt32(reader.GetOrdinal("id_sale")),
                SaleNumber = reader.GetString(reader.GetOrdinal("sale_number")),
                ReceiptType = reader.GetString(reader.GetOrdinal("receipt_type")),
                DocumentTypeName = reader.GetString(reader.GetOrdinal("document_type_name")),
                DocumentNumber = reader.GetString(reader.GetOrdinal("document_number")),
                Subtotal = reader.GetDecimal(reader.GetOrdinal("subtotal")),
                Discount = reader.GetDecimal(reader.GetOrdinal("discount")),
                Tax = reader.GetDecimal(reader.GetOrdinal("tax")),
                Total = reader.GetDecimal(reader.GetOrdinal("total")),
                AmountPaid = reader.IsDBNull(reader.GetOrdinal("amount_paid")) ? null : reader.GetDecimal(reader.GetOrdinal("amount_paid")),
                ChangeAmount = reader.IsDBNull(reader.GetOrdinal("change_amount")) ? null : reader.GetDecimal(reader.GetOrdinal("change_amount")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                PaymentMethodName = reader.GetString(reader.GetOrdinal("payment_method_name")),
                ClientName = reader.GetString(reader.GetOrdinal("client_name")),
                EmployeeName = reader.GetString(reader.GetOrdinal("employee_name"))
            };
        }

        if (header is null) return null;

        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                lines.Add(new SaleVoucherLine
                {
                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                    ProductName = reader.GetString(reader.GetOrdinal("product_name")),
                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("unit_price")),
                    Subtotal = reader.GetDecimal(reader.GetOrdinal("subtotal"))
                });
            }
        }

        return new SaleVoucherViewModel { Header = header, Lines = lines };
    }

    public static string BuildDetailsJson(IEnumerable<SalePosLineDto> lines)
        => JsonSerializer.Serialize(lines, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    public static string ResolveReceiptType(string documentTypeName)
        => documentTypeName.Contains("RUC", StringComparison.OrdinalIgnoreCase) ? "FACTURA" : "BOLETA";

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
