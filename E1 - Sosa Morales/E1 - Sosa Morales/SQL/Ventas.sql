USE KMLLogistics;
GO

-- ==========================================
-- Schema: PaymentMethods + Sales extensions
-- ==========================================
IF OBJECT_ID('dbo.PaymentMethods', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentMethods (
        id_payment_method INT IDENTITY(1,1) PRIMARY KEY,
        name VARCHAR(50) NOT NULL UNIQUE,
        description VARCHAR(255) NULL,
        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at DATETIME NULL,
        deleted_at DATETIME NULL,
        status TINYINT NOT NULL DEFAULT (1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethods WHERE name = N'Efectivo' AND deleted_at IS NULL)
    INSERT INTO dbo.PaymentMethods (name) VALUES (N'Efectivo');
IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethods WHERE name = N'Plin' AND deleted_at IS NULL)
    INSERT INTO dbo.PaymentMethods (name) VALUES (N'Plin');
IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethods WHERE name = N'Yape' AND deleted_at IS NULL)
    INSERT INTO dbo.PaymentMethods (name) VALUES (N'Yape');
IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethods WHERE name = N'Tarjeta' AND deleted_at IS NULL)
    INSERT INTO dbo.PaymentMethods (name) VALUES (N'Tarjeta');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SaleStatuses WHERE name = N'Completada' AND deleted_at IS NULL)
    INSERT INTO dbo.SaleStatuses (name, description) VALUES (N'Completada', N'Venta registrada y pagada');
IF NOT EXISTS (SELECT 1 FROM dbo.SaleStatuses WHERE name = N'Anulada' AND deleted_at IS NULL)
    INSERT INTO dbo.SaleStatuses (name, description) VALUES (N'Anulada', N'Venta anulada');
GO

IF COL_LENGTH('dbo.Sales', 'id_payment_method') IS NULL
    ALTER TABLE dbo.Sales ADD id_payment_method INT NULL;
IF COL_LENGTH('dbo.Sales', 'sale_number') IS NULL
    ALTER TABLE dbo.Sales ADD sale_number VARCHAR(20) NULL;
IF COL_LENGTH('dbo.Sales', 'receipt_type') IS NULL
    ALTER TABLE dbo.Sales ADD receipt_type VARCHAR(20) NULL;
IF COL_LENGTH('dbo.Sales', 'document_type_name') IS NULL
    ALTER TABLE dbo.Sales ADD document_type_name VARCHAR(50) NULL;
IF COL_LENGTH('dbo.Sales', 'document_number') IS NULL
    ALTER TABLE dbo.Sales ADD document_number VARCHAR(20) NULL;
IF COL_LENGTH('dbo.Sales', 'discount') IS NULL
    ALTER TABLE dbo.Sales ADD discount DECIMAL(10,2) NOT NULL CONSTRAINT DF_Sales_discount DEFAULT (0);
IF COL_LENGTH('dbo.Sales', 'amount_paid') IS NULL
    ALTER TABLE dbo.Sales ADD amount_paid DECIMAL(10,2) NULL;
IF COL_LENGTH('dbo.Sales', 'change_amount') IS NULL
    ALTER TABLE dbo.Sales ADD change_amount DECIMAL(10,2) NULL;
GO

UPDATE dbo.Sales
SET id_payment_method = (SELECT TOP 1 id_payment_method FROM dbo.PaymentMethods WHERE name = N'Efectivo' AND deleted_at IS NULL)
WHERE id_payment_method IS NULL;
UPDATE dbo.Sales SET sale_number = 'T001-' + RIGHT('00000000' + CAST(id_sale AS VARCHAR(8)), 8) WHERE sale_number IS NULL;
UPDATE dbo.Sales SET receipt_type = 'BOLETA' WHERE receipt_type IS NULL;
UPDATE dbo.Sales SET document_type_name = 'DNI' WHERE document_type_name IS NULL;
UPDATE dbo.Sales SET document_number = '00000000' WHERE document_number IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'fk_sale_payment_method')
BEGIN
    ALTER TABLE dbo.Sales ALTER COLUMN id_payment_method INT NOT NULL;
    ALTER TABLE dbo.Sales ALTER COLUMN sale_number VARCHAR(20) NOT NULL;
    ALTER TABLE dbo.Sales ALTER COLUMN receipt_type VARCHAR(20) NOT NULL;
    ALTER TABLE dbo.Sales ALTER COLUMN document_type_name VARCHAR(50) NOT NULL;
    ALTER TABLE dbo.Sales ALTER COLUMN document_number VARCHAR(20) NOT NULL;
    ALTER TABLE dbo.Sales ADD CONSTRAINT fk_sale_payment_method
        FOREIGN KEY (id_payment_method) REFERENCES dbo.PaymentMethods(id_payment_method);
END
GO

-- ==========================================
-- Sale Statuses CRUD
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_sale_status_list_active
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT ss.id_sale_status, ss.name, ss.description, ss.status, COUNT(*) OVER() AS total_count
    FROM dbo.SaleStatuses ss
    WHERE ss.deleted_at IS NULL AND ss.status = 1
      AND (@search IS NULL OR ss.name LIKE '%' + @search + '%' OR ss.description LIKE '%' + @search + '%')
    ORDER BY ss.name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_status_list_inactive
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT ss.id_sale_status, ss.name, ss.description, ss.status, COUNT(*) OVER() AS total_count
    FROM dbo.SaleStatuses ss
    WHERE (ss.deleted_at IS NOT NULL OR ss.status = 0)
      AND (@search IS NULL OR ss.name LIKE '%' + @search + '%' OR ss.description LIKE '%' + @search + '%')
    ORDER BY ss.name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_status_get_by_id @id_sale_status INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_sale_status, name, description, status, created_at, updated_at
    FROM dbo.SaleStatuses WHERE id_sale_status = @id_sale_status;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_status_create
    @name VARCHAR(50),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.SaleStatuses WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un estado con ese nombre.' AS message, CAST(NULL AS INT) AS id_sale_status; RETURN; END
    INSERT INTO dbo.SaleStatuses (name, description, status, created_at) VALUES (@name, @description, 1, GETDATE());
    SELECT 1 AS success, 'Estado registrado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_sale_status;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_status_update
    @id_sale_status INT,
    @name VARCHAR(50),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.SaleStatuses WHERE id_sale_status = @id_sale_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El estado no existe.' AS message, CAST(NULL AS INT) AS id_sale_status; RETURN; END
    IF EXISTS (SELECT 1 FROM dbo.SaleStatuses WHERE name = @name AND id_sale_status <> @id_sale_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El nombre ya esta en uso.' AS message, CAST(NULL AS INT) AS id_sale_status; RETURN; END
    UPDATE dbo.SaleStatuses SET name = @name, description = @description, updated_at = GETDATE() WHERE id_sale_status = @id_sale_status;
    SELECT 1 AS success, 'Estado actualizado correctamente.' AS message, @id_sale_status AS id_sale_status;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_status_delete_logic @id_sale_status INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Sales WHERE id_sale_status = @id_sale_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'No se puede desactivar: el estado tiene ventas asociadas.' AS message, CAST(NULL AS INT) AS id_sale_status; RETURN; END
    UPDATE dbo.SaleStatuses SET status = 0, deleted_at = GETDATE() WHERE id_sale_status = @id_sale_status;
    SELECT 1 AS success, 'Estado desactivado correctamente.' AS message, @id_sale_status AS id_sale_status;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_status_restore @id_sale_status INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SaleStatuses SET status = 1, deleted_at = NULL, updated_at = GETDATE() WHERE id_sale_status = @id_sale_status;
    SELECT 1 AS success, 'Estado restaurado correctamente.' AS message, @id_sale_status AS id_sale_status;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_status_delete_physical @id_sale_status INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Sales WHERE id_sale_status = @id_sale_status)
    BEGIN SELECT 0 AS success, 'No se puede eliminar: el estado tiene ventas asociadas.' AS message, CAST(NULL AS INT) AS id_sale_status; RETURN; END
    DELETE FROM dbo.SaleStatuses WHERE id_sale_status = @id_sale_status;
    SELECT 1 AS success, 'Estado eliminado permanentemente.' AS message, @id_sale_status AS id_sale_status;
END
GO

-- ==========================================
-- POS lookups
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_sale_pos_product_list
    @search VARCHAR(100) = NULL,
    @id_category INT = NULL,
    @id_brand INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH stock AS (
        SELECT wd.id_product,
               SUM(wd.stock) AS total_stock,
               MAX(wd.id_warehouse) AS id_warehouse
        FROM dbo.WarehouseDetails wd
        INNER JOIN dbo.Warehouses w ON w.id_warehouse = wd.id_warehouse AND w.deleted_at IS NULL AND w.status = 1
        WHERE wd.stock > 0
        GROUP BY wd.id_product
    )
    SELECT
        p.id_product,
        p.name,
        p.photo,
        p.sale_price,
        ISNULL(s.total_stock, 0) AS stock,
        ISNULL(s.id_warehouse, 0) AS id_warehouse,
        c.name AS category_name,
        b.name AS brand_name
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON c.id_category = p.id_category
    INNER JOIN dbo.Brands b ON b.id_brand = p.id_brand
    LEFT JOIN stock s ON s.id_product = p.id_product
    WHERE p.deleted_at IS NULL AND p.status = 1
      AND (@id_category IS NULL OR p.id_category = @id_category)
      AND (@id_brand IS NULL OR p.id_brand = @id_brand)
      AND (
            @search IS NULL
            OR p.name LIKE '%' + @search + '%'
            OR c.name LIKE '%' + @search + '%'
            OR b.name LIKE '%' + @search + '%'
          )
    ORDER BY p.name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_payment_method_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_payment_method AS id, name FROM dbo.PaymentMethods WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_client_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        c.id_client,
        c.name + ' ' + c.last_name_paternal AS client_name,
        dt.name AS document_type_name,
        c.document_number,
        c.id_document_type
    FROM dbo.Clients c
    INNER JOIN dbo.DocumentTypes dt ON dt.id_document_type = c.id_document_type
    WHERE c.deleted_at IS NULL AND c.status = 1
    ORDER BY c.name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_employee_get_by_user @id_user INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        e.id_employee,
        e.name + ' ' + e.last_name_paternal AS employee_name
    FROM dbo.Employees e
    WHERE e.id_user = @id_user AND e.deleted_at IS NULL AND e.status = 1;
END
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_create
    @id_client INT,
    @id_employee INT,
    @id_payment_method INT,
    @receipt_type VARCHAR(20),
    @document_type_name VARCHAR(50),
    @document_number VARCHAR(20),
    @subtotal DECIMAL(10,2),
    @discount DECIMAL(10,2) = 0,
    @tax DECIMAL(10,2),
    @total DECIMAL(10,2),
    @amount_paid DECIMAL(10,2) = NULL,
    @change_amount DECIMAL(10,2) = NULL,
    @details_json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    DECLARE @id_sale INT;
    DECLARE @id_sale_status INT;
    DECLARE @sale_number VARCHAR(20);
    DECLARE @series_prefix VARCHAR(5);
    DECLARE @next_num INT;

    IF @details_json IS NULL OR LTRIM(RTRIM(@details_json)) = '' OR @details_json = '[]'
    BEGIN SELECT 0 AS success, 'Agregue al menos un producto al carrito.' AS message, CAST(NULL AS INT) AS id_sale; RETURN; END

    SELECT TOP 1 @id_sale_status = id_sale_status FROM dbo.SaleStatuses WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;
    IF @id_sale_status IS NULL
        SELECT TOP 1 @id_sale_status = id_sale_status FROM dbo.SaleStatuses WHERE deleted_at IS NULL AND status = 1 ORDER BY id_sale_status;

    IF @id_sale_status IS NULL
    BEGIN SELECT 0 AS success, 'No hay estados de venta configurados.' AS message, CAST(NULL AS INT) AS id_sale; RETURN; END

    SET @series_prefix = CASE WHEN UPPER(LTRIM(RTRIM(@receipt_type))) = 'FACTURA' THEN 'F001-' ELSE 'B001-' END;
    SELECT @next_num = ISNULL(MAX(CAST(RIGHT(sale_number, 8) AS INT)), 0) + 1
    FROM dbo.Sales
    WHERE sale_number LIKE @series_prefix + '%';
    SET @sale_number = @series_prefix + RIGHT('00000000' + CAST(@next_num AS VARCHAR(8)), 8);

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Sales
            (id_client, id_employee, id_sale_status, id_payment_method, sale_number, receipt_type,
             document_type_name, document_number, subtotal, discount, tax, total, amount_paid, change_amount, created_at)
        VALUES
            (@id_client, @id_employee, @id_sale_status, @id_payment_method, @sale_number, @receipt_type,
             @document_type_name, @document_number, @subtotal, @discount, @tax, @total, @amount_paid, @change_amount, GETDATE());

        SET @id_sale = SCOPE_IDENTITY();

        INSERT INTO dbo.SaleDetails (id_sale, id_product, id_warehouse, quantity, unit_price)
        SELECT @id_sale, j.id_product, j.id_warehouse, j.quantity, j.unit_price
        FROM OPENJSON(@details_json)
        WITH (
            id_product INT '$.idProduct',
            id_warehouse INT '$.idWarehouse',
            quantity INT '$.quantity',
            unit_price DECIMAL(10,2) '$.unitPrice'
        ) j;

        UPDATE wd
        SET wd.stock = wd.stock - j.quantity
        FROM dbo.WarehouseDetails wd
        INNER JOIN OPENJSON(@details_json)
        WITH (id_product INT '$.idProduct', id_warehouse INT '$.idWarehouse', quantity INT '$.quantity') j
            ON wd.id_product = j.id_product AND wd.id_warehouse = j.id_warehouse;

        COMMIT TRANSACTION;
        SELECT 1 AS success, 'Venta registrada correctamente.' AS message, @id_sale AS id_sale;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS success, ERROR_MESSAGE() AS message, CAST(NULL AS INT) AS id_sale;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_get_voucher @id_sale INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.id_sale,
        s.sale_number,
        s.receipt_type,
        s.document_type_name,
        s.document_number,
        s.subtotal,
        s.discount,
        s.tax,
        s.total,
        s.amount_paid,
        s.change_amount,
        s.created_at,
        pm.name AS payment_method_name,
        c.name + ' ' + c.last_name_paternal AS client_name,
        e.name + ' ' + e.last_name_paternal AS employee_name
    FROM dbo.Sales s
    INNER JOIN dbo.PaymentMethods pm ON pm.id_payment_method = s.id_payment_method
    INNER JOIN dbo.Clients c ON c.id_client = s.id_client
    INNER JOIN dbo.Employees e ON e.id_employee = s.id_employee
    WHERE s.id_sale = @id_sale AND s.deleted_at IS NULL;

    SELECT
        sd.quantity,
        p.name AS product_name,
        sd.unit_price,
        sd.subtotal
    FROM dbo.SaleDetails sd
    INNER JOIN dbo.Products p ON p.id_product = sd.id_product
    WHERE sd.id_sale = @id_sale
    ORDER BY p.name;
END
GO

-- ==========================================
-- Sale detail listing + metrics
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_sale_detail_metrics
    @search VARCHAR(100) = NULL,
    @id_sale INT = NULL,
    @id_product INT = NULL,
    @id_client INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(DISTINCT sd.id_sale) AS sale_count,
        ISNULL(SUM(sd.subtotal), 0) AS total_subtotal,
        ISNULL(SUM(sd.subtotal) * 0.18, 0) AS total_tax,
        ISNULL(SUM(sd.subtotal) * 1.18, 0) AS total_amount,
        ISNULL(SUM(sd.subtotal - (p.cost * sd.quantity)), 0) AS net_profit
    FROM dbo.SaleDetails sd
    INNER JOIN dbo.Sales s ON s.id_sale = sd.id_sale AND s.deleted_at IS NULL
    INNER JOIN dbo.Products p ON p.id_product = sd.id_product
    INNER JOIN dbo.Clients c ON c.id_client = s.id_client
    WHERE (@id_sale IS NULL OR sd.id_sale = @id_sale)
      AND (@id_product IS NULL OR sd.id_product = @id_product)
      AND (@id_client IS NULL OR s.id_client = @id_client)
      AND (
            @search IS NULL
            OR CAST(sd.id_sale AS VARCHAR(20)) LIKE '%' + @search + '%'
            OR p.name LIKE '%' + @search + '%'
            OR c.name LIKE '%' + @search + '%'
            OR s.sale_number LIKE '%' + @search + '%'
          );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_detail_list
    @search VARCHAR(100) = NULL,
    @id_sale INT = NULL,
    @id_product INT = NULL,
    @id_client INT = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        sd.id_sale_detail,
        sd.id_sale,
        s.sale_number,
        p.name AS product_name,
        c.name + ' ' + c.last_name_paternal AS client_name,
        w.name AS warehouse_name,
        sd.quantity,
        sd.unit_price,
        sd.subtotal,
        s.created_at,
        COUNT(*) OVER() AS total_count
    FROM dbo.SaleDetails sd
    INNER JOIN dbo.Sales s ON s.id_sale = sd.id_sale AND s.deleted_at IS NULL
    INNER JOIN dbo.Products p ON p.id_product = sd.id_product
    INNER JOIN dbo.Clients c ON c.id_client = s.id_client
    INNER JOIN dbo.Warehouses w ON w.id_warehouse = sd.id_warehouse
    WHERE (@id_sale IS NULL OR sd.id_sale = @id_sale)
      AND (@id_product IS NULL OR sd.id_product = @id_product)
      AND (@id_client IS NULL OR s.id_client = @id_client)
      AND (
            @search IS NULL
            OR CAST(sd.id_sale AS VARCHAR(20)) LIKE '%' + @search + '%'
            OR p.name LIKE '%' + @search + '%'
            OR c.name LIKE '%' + @search + '%'
            OR s.sale_number LIKE '%' + @search + '%'
          )
    ORDER BY s.created_at DESC, sd.id_sale_detail DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_detail_filter_product_options
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT p.id_product AS id, p.name
    FROM dbo.SaleDetails sd
    INNER JOIN dbo.Products p ON p.id_product = sd.id_product
    ORDER BY p.name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_detail_filter_client_options
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT c.id_client AS id, c.name + ' ' + c.last_name_paternal AS name
    FROM dbo.Sales s
    INNER JOIN dbo.Clients c ON c.id_client = s.id_client
    WHERE s.deleted_at IS NULL
    ORDER BY name;
END
GO
