-- ============================================================
-- KMLLogistics - Carga inicial de stock vía COMPRAS
-- Flujo: PurchaseStatuses -> ProductSuppliers -> Purchases
--        -> PurchaseDetails -> PurchaseWarehouseDetails
--        -> WarehouseDetails + InventoryMovements
--
-- Requisitos previos en BD:
--   - Products (18 celulares/accesorios ya insertados)
--   - Warehouses: Almacén Central, Sur, Norte
--   - Suppliers: ChrisDev (id_supplier = 1)
--   - Employees: al menos 1 activo
--
-- Ejecutar UNA sola vez en SSMS (SQL Server 2017+)
-- ============================================================

USE KMLLogistics;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ############################################################
-- 1. ESTADOS DE COMPRA
-- ############################################################

IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = N'Pendiente' AND deleted_at IS NULL)
    INSERT INTO PurchaseStatuses (name) VALUES (N'Pendiente');

IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = N'Completada' AND deleted_at IS NULL)
    INSERT INTO PurchaseStatuses (name) VALUES (N'Completada');

IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = N'Cancelada' AND deleted_at IS NULL)
    INSERT INTO PurchaseStatuses (name) VALUES (N'Cancelada');
GO

-- ############################################################
-- 2. TIPO DE MOVIMIENTO (kardex)
-- ############################################################

IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE name = N'Entrada por compra' AND deleted_at IS NULL)
    INSERT INTO MovementTypes (name) VALUES (N'Entrada por compra');
GO

-- ############################################################
-- 3. PRODUCTO-PROVEEDOR (enlaza cada producto con ChrisDev)
-- ############################################################

DECLARE @id_supplier INT = (SELECT TOP 1 id_supplier FROM Suppliers WHERE deleted_at IS NULL AND status = 1 ORDER BY id_supplier);

IF @id_supplier IS NULL
BEGIN
    RAISERROR(N'No hay proveedor activo. Cree al menos un registro en Suppliers.', 16, 1);
    RETURN;
END

INSERT INTO ProductSuppliers (id_product, id_supplier, supplier_cost, last_purchase_cost, is_main_supplier)
SELECT
    p.id_product,
    @id_supplier,
    p.cost,
    p.cost,
    1
FROM Products p
WHERE p.deleted_at IS NULL
  AND p.status = 1
  AND NOT EXISTS (
      SELECT 1 FROM ProductSuppliers ps
      WHERE ps.id_product = p.id_product
        AND ps.id_supplier = @id_supplier
        AND ps.deleted_at IS NULL
  );
GO

-- ############################################################
-- 4. COMPRA + DETALLE + DISTRIBUCIÓN A ALMACENES + STOCK
--    (solo si aún no hay stock cargado por esta semilla)
-- ############################################################

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF EXISTS (SELECT 1 FROM WarehouseDetails)
BEGIN
    PRINT N'WarehouseDetails ya tiene registros. Omitiendo compra semilla.';
    RETURN;
END

DECLARE @id_supplier      INT;
DECLARE @id_employee      INT;
DECLARE @id_status_ok     INT;
DECLARE @id_movement_in   INT;
DECLARE @id_purchase      INT;
DECLARE @id_wh_central    INT;
DECLARE @id_wh_sur        INT;
DECLARE @id_wh_norte      INT;
DECLARE @subtotal         DECIMAL(10,2);
DECLARE @tax              DECIMAL(10,2);
DECLARE @total            DECIMAL(10,2);
DECLARE @id_product         INT;
DECLARE @qty_central        INT;
DECLARE @qty_sur            INT;
DECLARE @qty_norte          INT;
DECLARE @qty_total          INT;
DECLARE @unit_cost          DECIMAL(10,2);
DECLARE @id_product_supplier INT;
DECLARE @id_purchase_detail INT;
DECLARE @msg                NVARCHAR(4000);

SELECT TOP 1 @id_supplier = id_supplier FROM Suppliers WHERE deleted_at IS NULL AND status = 1 ORDER BY id_supplier;
SELECT TOP 1 @id_employee = id_employee FROM Employees WHERE deleted_at IS NULL AND status = 1 ORDER BY id_employee;
SELECT @id_status_ok   = id_purchase_status FROM PurchaseStatuses WHERE name = N'Completada' AND deleted_at IS NULL;
SELECT @id_movement_in = id_movement_type   FROM MovementTypes   WHERE name = N'Entrada por compra' AND deleted_at IS NULL;
SELECT @id_wh_central  = id_warehouse FROM Warehouses WHERE deleted_at IS NULL AND status = 1 AND name LIKE N'%Central%';
SELECT @id_wh_sur      = id_warehouse FROM Warehouses WHERE deleted_at IS NULL AND status = 1 AND name LIKE N'%Sur%';
SELECT @id_wh_norte    = id_warehouse FROM Warehouses WHERE deleted_at IS NULL AND status = 1 AND name LIKE N'%Norte%';

IF @id_employee IS NULL OR @id_status_ok IS NULL OR @id_movement_in IS NULL
BEGIN
    RAISERROR(N'Faltan empleado activo, estado Completada o tipo Entrada por compra.', 16, 1);
    RETURN;
END

IF @id_wh_central IS NULL OR @id_wh_sur IS NULL OR @id_wh_norte IS NULL
BEGIN
    RAISERROR(N'Faltan los almacenes Central, Sur o Norte.', 16, 1);
    RETURN;
END

-- Distribución: cantidades por producto y almacén
DECLARE @Distrib TABLE (
    id_product   INT NOT NULL PRIMARY KEY,
    qty_central  INT NOT NULL,
    qty_sur      INT NOT NULL,
    qty_norte    INT NOT NULL
);

INSERT INTO @Distrib (id_product, qty_central, qty_sur, qty_norte)
SELECT p.id_product, d.qty_central, d.qty_sur, d.qty_norte
FROM (VALUES
    (N'iPhone 15 128GB',              8,  5,  5),
    (N'iPhone 14 128GB',              6,  4,  4),
    (N'Samsung Galaxy S24 Ultra',     5,  4,  4),
    (N'Xiaomi 14 Ultra',              5,  3,  3),
    (N'Samsung Galaxy A55 5G',       10,  8,  8),
    (N'Motorola Edge 40 Neo',         8,  6,  6),
    (N'Xiaomi Redmi Note 13 Pro',    12, 10, 10),
    (N'Realme 12 Pro',               10,  8,  8),
    (N'OPPO Reno 11F',               10,  8,  8),
    (N'Tecno Spark 20',              20, 15, 15),
    (N'Infinix Smart 8',             20, 15, 15),
    (N'Nokia C32',                   15, 12, 12),
    (N'Cargador Samsung 25W USB-C',  30, 25, 25),
    (N'Adaptador Apple USB-C 20W',   25, 20, 20),
    (N'Funda Xiaomi Redmi Note 13',  40, 30, 30),
    (N'Funda iPhone 15 Transparente',35, 25, 25),
    (N'Protector Galaxy A55',        50, 40, 40),
    (N'Protector iPhone 15',         45, 35, 35)
) AS d(product_name, qty_central, qty_sur, qty_norte)
INNER JOIN Products p ON p.name = d.product_name AND p.deleted_at IS NULL;

IF (SELECT COUNT(*) FROM @Distrib) <> 18
BEGIN
    RAISERROR(N'No se encontraron los 18 productos esperados. Verifique los nombres en Products.', 16, 1);
    RETURN;
END

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT d.id_product, d.qty_central, d.qty_sur, d.qty_norte, p.cost,
           ps.id_product_supplier
    FROM @Distrib d
    INNER JOIN Products p ON p.id_product = d.id_product
    INNER JOIN ProductSuppliers ps ON ps.id_product = d.id_product
        AND ps.id_supplier = @id_supplier AND ps.deleted_at IS NULL;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Cabecera de compra (estado Completada = mercadería ingresada)
    INSERT INTO Purchases (id_supplier, id_employee, id_purchase_status, fec_purchase, subtotal, tax, total)
    VALUES (@id_supplier, @id_employee, @id_status_ok, GETDATE(), 0, 0, 0);

    SET @id_purchase = SCOPE_IDENTITY();

    OPEN cur;
    FETCH NEXT FROM cur INTO @id_product, @qty_central, @qty_sur, @qty_norte, @unit_cost, @id_product_supplier;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @qty_total = @qty_central + @qty_sur + @qty_norte;

        INSERT INTO PurchaseDetails (id_purchase, id_product_supplier, quantity, unit_cost)
        VALUES (@id_purchase, @id_product_supplier, @qty_total, @unit_cost);

        SET @id_purchase_detail = SCOPE_IDENTITY();

        -- Almacén Central
        IF @qty_central > 0
        BEGIN
            INSERT INTO PurchaseWarehouseDetails (id_purchase_detail, id_warehouse, quantity)
            VALUES (@id_purchase_detail, @id_wh_central, @qty_central);

            IF EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_warehouse = @id_wh_central AND id_product = @id_product)
                UPDATE WarehouseDetails SET stock = stock + @qty_central WHERE id_warehouse = @id_wh_central AND id_product = @id_product;
            ELSE
                INSERT INTO WarehouseDetails (id_warehouse, id_product, stock) VALUES (@id_wh_central, @id_product, @qty_central);

            INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
            VALUES (@id_product, @id_wh_central, @id_movement_in, @id_employee, @qty_central,
                    CONCAT(N'COM-', @id_purchase), GETDATE());
        END

        -- Almacén Sur
        IF @qty_sur > 0
        BEGIN
            INSERT INTO PurchaseWarehouseDetails (id_purchase_detail, id_warehouse, quantity)
            VALUES (@id_purchase_detail, @id_wh_sur, @qty_sur);

            IF EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_warehouse = @id_wh_sur AND id_product = @id_product)
                UPDATE WarehouseDetails SET stock = stock + @qty_sur WHERE id_warehouse = @id_wh_sur AND id_product = @id_product;
            ELSE
                INSERT INTO WarehouseDetails (id_warehouse, id_product, stock) VALUES (@id_wh_sur, @id_product, @qty_sur);

            INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
            VALUES (@id_product, @id_wh_sur, @id_movement_in, @id_employee, @qty_sur,
                    CONCAT(N'COM-', @id_purchase), GETDATE());
        END

        -- Almacén Norte
        IF @qty_norte > 0
        BEGIN
            INSERT INTO PurchaseWarehouseDetails (id_purchase_detail, id_warehouse, quantity)
            VALUES (@id_purchase_detail, @id_wh_norte, @qty_norte);

            IF EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_warehouse = @id_wh_norte AND id_product = @id_product)
                UPDATE WarehouseDetails SET stock = stock + @qty_norte WHERE id_warehouse = @id_wh_norte AND id_product = @id_product;
            ELSE
                INSERT INTO WarehouseDetails (id_warehouse, id_product, stock) VALUES (@id_wh_norte, @id_product, @qty_norte);

            INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
            VALUES (@id_product, @id_wh_norte, @id_movement_in, @id_employee, @qty_norte,
                    CONCAT(N'COM-', @id_purchase), GETDATE());
        END

        FETCH NEXT FROM cur INTO @id_product, @qty_central, @qty_sur, @qty_norte, @unit_cost, @id_product_supplier;
    END

    CLOSE cur;
    DEALLOCATE cur;

    -- Totales de la compra (IGV 18% sobre subtotal)
    SELECT @subtotal = SUM(CAST(pd.quantity AS DECIMAL(10,2)) * pd.unit_cost)
    FROM PurchaseDetails pd
    WHERE pd.id_purchase = @id_purchase;

    SET @tax   = ROUND(@subtotal * 0.18, 2);
    SET @total = @subtotal + @tax;

    UPDATE Purchases
    SET subtotal = @subtotal, tax = @tax, total = @total, updated_at = GETDATE()
    WHERE id_purchase = @id_purchase;

    COMMIT TRANSACTION;

    PRINT CONCAT(N'Compra semilla registrada. id_purchase = ', @id_purchase);
    PRINT CONCAT(N'Subtotal: ', @subtotal, N' | IGV: ', @tax, N' | Total: ', @total);
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @msg = ERROR_MESSAGE();
    RAISERROR(N'Error en carga de stock.', 16, 1);
    PRINT @msg;
END CATCH
GO

-- ############################################################
-- 5. VERIFICACIÓN
-- ############################################################

SELECT
    w.name AS almacen,
    p.name AS producto,
    wd.stock
FROM WarehouseDetails wd
INNER JOIN Warehouses w ON w.id_warehouse = wd.id_warehouse
INNER JOIN Products p ON p.id_product = wd.id_product
ORDER BY w.name, p.name;

SELECT
    ps.name AS estado_compra,
    COUNT(*) AS cantidad
FROM PurchaseStatuses ps
WHERE ps.deleted_at IS NULL
GROUP BY ps.name;

SELECT
    pu.id_purchase,
    ps.name AS estado,
    s.name AS proveedor,
    pu.subtotal,
    pu.tax,
    pu.total,
    pu.fec_purchase
FROM Purchases pu
INNER JOIN PurchaseStatuses ps ON ps.id_purchase_status = pu.id_purchase_status
INNER JOIN Suppliers s ON s.id_supplier = pu.id_supplier
WHERE pu.deleted_at IS NULL
ORDER BY pu.id_purchase DESC;
GO
