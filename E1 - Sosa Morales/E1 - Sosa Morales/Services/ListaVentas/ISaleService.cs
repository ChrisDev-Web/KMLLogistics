using E1___Sosa_Morales.Models.ListaVentas;

namespace E1___Sosa_Morales.Services.ListaVentas;

public interface ISaleService
{
    Task<List<PosProductItem>> GetPosProductsAsync(string? search, int? idCategory, int? idBrand);
    Task<List<SaleClientOption>> GetClientsAsync();
    Task<List<PaymentMethodOption>> GetPaymentMethodsAsync();
    Task<SaleEmployeeInfo?> GetEmployeeByUserAsync(int idUser);
    Task<(bool Success, string Message, int? IdSale)> CreateSaleAsync(
        int idClient, int idEmployee, int idPaymentMethod, string receiptType,
        string documentTypeName, string documentNumber,
        decimal subtotal, decimal discount, decimal tax, decimal total,
        decimal? amountPaid, decimal? changeAmount, string detailsJson);
    Task<SaleVoucherViewModel?> GetVoucherAsync(int idSale);
}
