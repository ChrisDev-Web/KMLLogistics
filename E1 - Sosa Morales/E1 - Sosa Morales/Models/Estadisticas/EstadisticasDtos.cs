using System.ComponentModel.DataAnnotations.Schema;

namespace E1___Sosa_Morales.Models.Estadisticas;

public class StatisticsSummaryRow
{
    [Column("sales_count")]
    public int SalesCount { get; set; }

    [Column("total_sales")]
    public decimal TotalSales { get; set; }

    [Column("purchases_count")]
    public int PurchasesCount { get; set; }

    [Column("total_purchases")]
    public decimal TotalPurchases { get; set; }

    [Column("net_profit")]
    public decimal NetProfit { get; set; }

    public decimal NetBalance => TotalSales - TotalPurchases;
}

public class StatisticsTrendRow
{
    [Column("period_date")]
    public DateTime PeriodDate { get; set; }

    [Column("sales_amount")]
    public decimal SalesAmount { get; set; }

    [Column("purchases_amount")]
    public decimal PurchasesAmount { get; set; }

    [Column("net_profit")]
    public decimal NetProfit { get; set; }

    [Column("sales_count")]
    public int SalesCount { get; set; }

    [Column("purchases_count")]
    public int PurchasesCount { get; set; }
}

public class StatisticsPaymentRow
{
    [Column("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Column("transaction_count")]
    public int TransactionCount { get; set; }

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }
}

public class StatisticsTopProductRow
{
    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("quantity_sold")]
    public int QuantitySold { get; set; }

    [Column("revenue")]
    public decimal Revenue { get; set; }
}

public class StatisticsPeriodInfo
{
    public string Preset { get; set; } = "today";
    public string Label { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}

public class StatisticsDashboardResult
{
    public StatisticsPeriodInfo Period { get; set; } = new();
    public StatisticsSummaryRow Summary { get; set; } = new();
    public List<StatisticsTrendRow> Trend { get; set; } = [];
    public List<StatisticsPaymentRow> Payments { get; set; } = [];
    public List<StatisticsTopProductRow> TopProducts { get; set; } = [];
}
