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

    [Column("hour_slot")]
    public int? HourSlot { get; set; }

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

public class StatisticsCategoryRow
{
    [Column("category_name")]
    public string CategoryName { get; set; } = string.Empty;

    [Column("revenue")]
    public decimal Revenue { get; set; }

    [Column("quantity_sold")]
    public int QuantitySold { get; set; }
}

public class StatisticsHourlyRow
{
    [Column("hour_of_day")]
    public int HourOfDay { get; set; }

    [Column("sales_amount")]
    public decimal SalesAmount { get; set; }

    [Column("purchases_amount")]
    public decimal PurchasesAmount { get; set; }
}

public class StatisticsRecentSaleRow
{
    [Column("sale_number")]
    public string SaleNumber { get; set; } = string.Empty;

    [Column("client_name")]
    public string ClientName { get; set; } = string.Empty;

    [Column("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Column("total")]
    public decimal Total { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class StatisticsStockAlertRow
{
    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    [Column("stock")]
    public int Stock { get; set; }

    [Column("min_stock")]
    public int MinStock { get; set; }
}

public class StatisticsPeriodInfo
{
    public string Preset { get; set; } = "today";
    public string Label { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}

public class StatisticsComparisonMetric
{
    public decimal Current { get; set; }
    public decimal Previous { get; set; }
    public decimal ChangePercent { get; set; }
}

public class StatisticsComparison
{
    public StatisticsComparisonMetric Sales { get; set; } = new();
    public StatisticsComparisonMetric Purchases { get; set; } = new();
    public StatisticsComparisonMetric Profit { get; set; } = new();
    public StatisticsComparisonMetric NetFlow { get; set; } = new();
}

public class StatisticsDashboardResult
{
    public StatisticsPeriodInfo Period { get; set; } = new();
    public string TrendGranularity { get; set; } = "daily";
    public StatisticsSummaryRow Summary { get; set; } = new();
    public StatisticsComparison Comparison { get; set; } = new();
    public List<StatisticsTrendRow> Trend { get; set; } = [];
    public List<StatisticsPaymentRow> Payments { get; set; } = [];
    public List<StatisticsTopProductRow> TopProducts { get; set; } = [];
    public List<StatisticsCategoryRow> TopCategories { get; set; } = [];
    public List<StatisticsHourlyRow> Hourly { get; set; } = [];
    public List<StatisticsRecentSaleRow> RecentSales { get; set; } = [];
    public List<StatisticsStockAlertRow> StockAlerts { get; set; } = [];
}
