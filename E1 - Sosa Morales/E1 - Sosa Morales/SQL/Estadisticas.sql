USE KMLLogistics;
GO

-- ============================================================
-- ESTADÍSTICAS — Ventas, compras, tendencias y métodos de pago
-- ============================================================

IF OBJECT_ID('dbo.sp_statistics_summary', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_statistics_summary;
GO
CREATE PROCEDURE dbo.sp_statistics_summary
    @date_from DATETIME,
    @date_to   DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SET @date_from = CAST(CAST(@date_from AS DATE) AS DATETIME);
    SET @date_to   = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(CAST(@date_to AS DATE) AS DATETIME)));

    DECLARE @id_sale_completed INT;
    DECLARE @id_purchase_completed INT;

    SELECT TOP 1 @id_sale_completed = id_sale_status
    FROM dbo.SaleStatuses
    WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;

    SELECT TOP 1 @id_purchase_completed = id_purchase_status
    FROM dbo.PurchaseStatuses
    WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;

    SELECT
        ISNULL((
            SELECT COUNT(*)
            FROM dbo.Sales s
            WHERE s.deleted_at IS NULL
              AND (@id_sale_completed IS NULL OR s.id_sale_status = @id_sale_completed)
              AND s.created_at BETWEEN @date_from AND @date_to
        ), 0) AS sales_count,
        ISNULL((
            SELECT SUM(s.total)
            FROM dbo.Sales s
            WHERE s.deleted_at IS NULL
              AND (@id_sale_completed IS NULL OR s.id_sale_status = @id_sale_completed)
              AND s.created_at BETWEEN @date_from AND @date_to
        ), 0) AS total_sales,
        ISNULL((
            SELECT COUNT(*)
            FROM dbo.Purchases p
            WHERE p.deleted_at IS NULL
              AND (@id_purchase_completed IS NULL OR p.id_purchase_status = @id_purchase_completed)
              AND p.fec_purchase BETWEEN @date_from AND @date_to
        ), 0) AS purchases_count,
        ISNULL((
            SELECT SUM(p.total)
            FROM dbo.Purchases p
            WHERE p.deleted_at IS NULL
              AND (@id_purchase_completed IS NULL OR p.id_purchase_status = @id_purchase_completed)
              AND p.fec_purchase BETWEEN @date_from AND @date_to
        ), 0) AS total_purchases,
        ISNULL((
            SELECT SUM(sd.subtotal - (pr.cost * sd.quantity))
            FROM dbo.SaleDetails sd
            INNER JOIN dbo.Sales s ON s.id_sale = sd.id_sale AND s.deleted_at IS NULL
            INNER JOIN dbo.Products pr ON pr.id_product = sd.id_product
            WHERE (@id_sale_completed IS NULL OR s.id_sale_status = @id_sale_completed)
              AND s.created_at BETWEEN @date_from AND @date_to
        ), 0) AS net_profit;
END
GO

IF OBJECT_ID('dbo.sp_statistics_daily_trend', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_statistics_daily_trend;
GO
CREATE PROCEDURE dbo.sp_statistics_daily_trend
    @date_from DATETIME,
    @date_to   DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SET @date_from = CAST(CAST(@date_from AS DATE) AS DATETIME);
    SET @date_to   = CAST(CAST(@date_to AS DATE) AS DATETIME);

    DECLARE @id_sale_completed INT;
    DECLARE @id_purchase_completed INT;

    SELECT TOP 1 @id_sale_completed = id_sale_status
    FROM dbo.SaleStatuses WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;

    SELECT TOP 1 @id_purchase_completed = id_purchase_status
    FROM dbo.PurchaseStatuses WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;

    ;WITH DateRange AS (
        SELECT @date_from AS period_date
        UNION ALL
        SELECT DATEADD(DAY, 1, period_date)
        FROM DateRange
        WHERE period_date < @date_to
    ),
    SalesByDay AS (
        SELECT
            CAST(s.created_at AS DATE) AS period_date,
            SUM(s.total) AS sales_amount,
            COUNT(*) AS sales_count
        FROM dbo.Sales s
        WHERE s.deleted_at IS NULL
          AND (@id_sale_completed IS NULL OR s.id_sale_status = @id_sale_completed)
          AND CAST(s.created_at AS DATE) BETWEEN @date_from AND @date_to
        GROUP BY CAST(s.created_at AS DATE)
    ),
    PurchasesByDay AS (
        SELECT
            CAST(p.fec_purchase AS DATE) AS period_date,
            SUM(p.total) AS purchases_amount,
            COUNT(*) AS purchases_count
        FROM dbo.Purchases p
        WHERE p.deleted_at IS NULL
          AND (@id_purchase_completed IS NULL OR p.id_purchase_status = @id_purchase_completed)
          AND CAST(p.fec_purchase AS DATE) BETWEEN @date_from AND @date_to
        GROUP BY CAST(p.fec_purchase AS DATE)
    ),
    ProfitByDay AS (
        SELECT
            CAST(s.created_at AS DATE) AS period_date,
            SUM(sd.subtotal - (pr.cost * sd.quantity)) AS net_profit
        FROM dbo.SaleDetails sd
        INNER JOIN dbo.Sales s ON s.id_sale = sd.id_sale AND s.deleted_at IS NULL
        INNER JOIN dbo.Products pr ON pr.id_product = sd.id_product
        WHERE (@id_sale_completed IS NULL OR s.id_sale_status = @id_sale_completed)
          AND CAST(s.created_at AS DATE) BETWEEN @date_from AND @date_to
        GROUP BY CAST(s.created_at AS DATE)
    )
    SELECT
        dr.period_date AS period_date,
        ISNULL(sb.sales_amount, 0) AS sales_amount,
        ISNULL(pb.purchases_amount, 0) AS purchases_amount,
        ISNULL(pf.net_profit, 0) AS net_profit,
        ISNULL(sb.sales_count, 0) AS sales_count,
        ISNULL(pb.purchases_count, 0) AS purchases_count
    FROM DateRange dr
    LEFT JOIN SalesByDay sb ON sb.period_date = dr.period_date
    LEFT JOIN PurchasesByDay pb ON pb.period_date = dr.period_date
    LEFT JOIN ProfitByDay pf ON pf.period_date = dr.period_date
    ORDER BY dr.period_date
    OPTION (MAXRECURSION 366);
END
GO

IF OBJECT_ID('dbo.sp_statistics_payment_breakdown', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_statistics_payment_breakdown;
GO
CREATE PROCEDURE dbo.sp_statistics_payment_breakdown
    @date_from DATETIME,
    @date_to   DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SET @date_from = CAST(CAST(@date_from AS DATE) AS DATETIME);
    SET @date_to   = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(CAST(@date_to AS DATE) AS DATETIME)));

    DECLARE @id_sale_completed INT;
    SELECT TOP 1 @id_sale_completed = id_sale_status
    FROM dbo.SaleStatuses WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;

    SELECT
        pm.name AS payment_method,
        COUNT(*) AS transaction_count,
        ISNULL(SUM(s.total), 0) AS total_amount
    FROM dbo.Sales s
    INNER JOIN dbo.PaymentMethods pm ON pm.id_payment_method = s.id_payment_method
    WHERE s.deleted_at IS NULL
      AND pm.deleted_at IS NULL
      AND (@id_sale_completed IS NULL OR s.id_sale_status = @id_sale_completed)
      AND s.created_at BETWEEN @date_from AND @date_to
    GROUP BY pm.name
    ORDER BY total_amount DESC;
END
GO

IF OBJECT_ID('dbo.sp_statistics_top_products', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_statistics_top_products;
GO
CREATE PROCEDURE dbo.sp_statistics_top_products
    @date_from DATETIME,
    @date_to   DATETIME,
    @top_n     INT = 8
AS
BEGIN
    SET NOCOUNT ON;

    SET @date_from = CAST(CAST(@date_from AS DATE) AS DATETIME);
    SET @date_to   = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(CAST(@date_to AS DATE) AS DATETIME)));
    IF @top_n IS NULL OR @top_n < 1 SET @top_n = 8;
    IF @top_n > 20 SET @top_n = 20;

    DECLARE @id_sale_completed INT;
    SELECT TOP 1 @id_sale_completed = id_sale_status
    FROM dbo.SaleStatuses WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;

    SELECT TOP (@top_n)
        p.name AS product_name,
        SUM(sd.quantity) AS quantity_sold,
        ISNULL(SUM(sd.subtotal), 0) AS revenue
    FROM dbo.SaleDetails sd
    INNER JOIN dbo.Sales s ON s.id_sale = sd.id_sale AND s.deleted_at IS NULL
    INNER JOIN dbo.Products p ON p.id_product = sd.id_product AND p.deleted_at IS NULL
    WHERE (@id_sale_completed IS NULL OR s.id_sale_status = @id_sale_completed)
      AND s.created_at BETWEEN @date_from AND @date_to
    GROUP BY p.name
    ORDER BY revenue DESC;
END
GO
