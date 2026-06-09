-- ============================================================
-- KMLLogistics - Stored Procedures: Inventario
-- Módulos: Warehouses, WarehouseDetails, MovementTypes, InventoryMovements
-- Nomenclatura: sp_warehouse_*, sp_warehouse_detail_*, sp_movement_type_*, sp_inventory_movement_*
-- ============================================================

USE KMLLogistics;
GO

-- ############################################################
-- WAREHOUSES
-- ############################################################

IF OBJECT_ID('dbo.sp_warehouse_district_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_district_list_active;
GO
CREATE PROCEDURE dbo.sp_warehouse_district_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.id_district,
        c.name + N' / ' + r.name + N' / ' + p.name + N' / ' + d.name AS name
    FROM Districts d
    INNER JOIN Provinces p ON p.id_province = d.id_province
    INNER JOIN Regions r ON r.id_region = p.id_region
    INNER JOIN Countries c ON c.id_country = r.id_country
    WHERE d.deleted_at IS NULL AND d.status = 1
      AND p.deleted_at IS NULL AND p.status = 1
      AND r.deleted_at IS NULL AND r.status = 1
      AND c.deleted_at IS NULL AND c.status = 1
    ORDER BY c.name, r.name, p.name, d.name;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_list_active;
GO
CREATE PROCEDURE dbo.sp_warehouse_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        w.id_warehouse,
        w.name,
        w.address,
        ISNULL(d.name, N'') AS district_name,
        COUNT(*) OVER() AS total_count
    FROM Warehouses w
    LEFT JOIN Districts d ON d.id_district = w.id_district
    WHERE w.deleted_at IS NULL AND w.status = 1
      AND (@search IS NULL OR @search = ''
           OR w.name LIKE N'%' + @search + N'%'
           OR w.address LIKE N'%' + @search + N'%'
           OR d.name LIKE N'%' + @search + N'%')
    ORDER BY w.id_warehouse DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_list_inactive;
GO
CREATE PROCEDURE dbo.sp_warehouse_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        w.id_warehouse,
        w.name,
        w.address,
        ISNULL(d.name, N'') AS district_name,
        COUNT(*) OVER() AS total_count
    FROM Warehouses w
    LEFT JOIN Districts d ON d.id_district = w.id_district
    WHERE w.deleted_at IS NULL AND w.status = 0
      AND (@search IS NULL OR @search = ''
           OR w.name LIKE N'%' + @search + N'%'
           OR w.address LIKE N'%' + @search + N'%'
           OR d.name LIKE N'%' + @search + N'%')
    ORDER BY w.id_warehouse DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_get_by_id;
GO
CREATE PROCEDURE dbo.sp_warehouse_get_by_id @id_warehouse INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        w.id_warehouse,
        w.name,
        w.address,
        w.id_district,
        ISNULL(c.name, N'') AS country_name,
        ISNULL(r.name, N'') AS region_name,
        ISNULL(p.name, N'') AS province_name,
        ISNULL(d.name, N'') AS district_name,
        w.status,
        w.created_at,
        w.updated_at
    FROM Warehouses w
    LEFT JOIN Districts d ON d.id_district = w.id_district
    LEFT JOIN Provinces p ON p.id_province = d.id_province
    LEFT JOIN Regions r ON r.id_region = p.id_region
    LEFT JOIN Countries c ON c.id_country = r.id_country
    WHERE w.id_warehouse = @id_warehouse AND w.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_create;
GO
CREATE PROCEDURE dbo.sp_warehouse_create
    @name        VARCHAR(100),
    @address     VARCHAR(255),
    @id_district INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Warehouses WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Ya existe un almacén con ese nombre.' AS message, NULL AS id_warehouse; RETURN; END
    IF @id_district IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'El distrito seleccionado no es válido.' AS message, NULL AS id_warehouse; RETURN; END
    INSERT INTO Warehouses (name, address, id_district) VALUES (@name, @address, @id_district);
    SELECT 1 AS success, N'Almacén creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_warehouse;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_update;
GO
CREATE PROCEDURE dbo.sp_warehouse_update
    @id_warehouse INT,
    @name           VARCHAR(100),
    @address        VARCHAR(255),
    @id_district    INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE id_warehouse = @id_warehouse AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Warehouses WHERE name = @name AND id_warehouse <> @id_warehouse AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Ya existe otro almacén con ese nombre.' AS message; RETURN; END
    IF @id_district IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'El distrito seleccionado no es válido.' AS message; RETURN; END
    UPDATE Warehouses SET name = @name, address = @address, id_district = @id_district, updated_at = GETDATE()
    WHERE id_warehouse = @id_warehouse;
    SELECT 1 AS success, N'Almacén actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_delete_logic;
GO
CREATE PROCEDURE dbo.sp_warehouse_delete_logic @id_warehouse INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE id_warehouse = @id_warehouse AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado o ya inactivo.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_warehouse = @id_warehouse AND stock > 0)
    BEGIN SELECT 0 AS success, N'No se puede desactivar: el almacén tiene stock.' AS message; RETURN; END
    UPDATE Warehouses SET status = 0, updated_at = GETDATE() WHERE id_warehouse = @id_warehouse;
    SELECT 1 AS success, N'Almacén desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_restore;
GO
CREATE PROCEDURE dbo.sp_warehouse_restore @id_warehouse INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE id_warehouse = @id_warehouse AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado o ya activo.' AS message; RETURN; END
    IF EXISTS (
        SELECT 1 FROM Warehouses
        WHERE name = (SELECT name FROM Warehouses WHERE id_warehouse = @id_warehouse)
          AND status = 1 AND deleted_at IS NULL
    )
    BEGIN SELECT 0 AS success, N'No se puede restaurar: ya existe un almacén activo con el mismo nombre.' AS message; RETURN; END
    UPDATE Warehouses SET status = 1, updated_at = GETDATE() WHERE id_warehouse = @id_warehouse;
    SELECT 1 AS success, N'Almacén restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_delete_physical;
GO
CREATE PROCEDURE dbo.sp_warehouse_delete_physical @id_warehouse INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE id_warehouse = @id_warehouse AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_warehouse = @id_warehouse)
    BEGIN SELECT 0 AS success, N'No se puede eliminar: el almacén tiene detalle de inventario.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Warehouses WHERE id_warehouse = @id_warehouse;
        SELECT 1 AS success, N'Almacén eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, N'No se puede eliminar: el registro tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- MOVEMENT TYPES
-- ############################################################

IF OBJECT_ID('dbo.sp_movement_type_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_movement_type_list_active;
GO
CREATE PROCEDURE dbo.sp_movement_type_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT mt.id_movement_type, mt.name, COUNT(*) OVER() AS total_count
    FROM MovementTypes mt
    WHERE mt.deleted_at IS NULL AND mt.status = 1
      AND (@search IS NULL OR @search = '' OR mt.name LIKE N'%' + @search + N'%')
    ORDER BY mt.id_movement_type DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_movement_type_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_movement_type_list_inactive;
GO
CREATE PROCEDURE dbo.sp_movement_type_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT mt.id_movement_type, mt.name, COUNT(*) OVER() AS total_count
    FROM MovementTypes mt
    WHERE mt.deleted_at IS NULL AND mt.status = 0
      AND (@search IS NULL OR @search = '' OR mt.name LIKE N'%' + @search + N'%')
    ORDER BY mt.id_movement_type DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_movement_type_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_movement_type_get_by_id;
GO
CREATE PROCEDURE dbo.sp_movement_type_get_by_id @id_movement_type INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_movement_type, name, status, created_at, updated_at
    FROM MovementTypes
    WHERE id_movement_type = @id_movement_type AND deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_movement_type_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_movement_type_create;
GO
CREATE PROCEDURE dbo.sp_movement_type_create @name VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM MovementTypes WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Ya existe un tipo de movimiento con ese nombre.' AS message, NULL AS id_movement_type; RETURN; END
    INSERT INTO MovementTypes (name) VALUES (@name);
    SELECT 1 AS success, N'Tipo de movimiento creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_movement_type;
END
GO

IF OBJECT_ID('dbo.sp_movement_type_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_movement_type_update;
GO
CREATE PROCEDURE dbo.sp_movement_type_update
    @id_movement_type INT,
    @name             VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE id_movement_type = @id_movement_type AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM MovementTypes WHERE name = @name AND id_movement_type <> @id_movement_type AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Ya existe otro tipo con ese nombre.' AS message; RETURN; END
    UPDATE MovementTypes SET name = @name, updated_at = GETDATE() WHERE id_movement_type = @id_movement_type;
    SELECT 1 AS success, N'Tipo de movimiento actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_movement_type_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_movement_type_delete_logic;
GO
CREATE PROCEDURE dbo.sp_movement_type_delete_logic @id_movement_type INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE id_movement_type = @id_movement_type AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado o ya inactivo.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM InventoryMovements WHERE id_movement_type = @id_movement_type AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'No se puede desactivar: el tipo está en uso en movimientos.' AS message; RETURN; END
    UPDATE MovementTypes SET status = 0, updated_at = GETDATE() WHERE id_movement_type = @id_movement_type;
    SELECT 1 AS success, N'Tipo de movimiento desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_movement_type_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_movement_type_restore;
GO
CREATE PROCEDURE dbo.sp_movement_type_restore @id_movement_type INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE id_movement_type = @id_movement_type AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado o ya activo.' AS message; RETURN; END
    IF EXISTS (
        SELECT 1 FROM MovementTypes
        WHERE name = (SELECT name FROM MovementTypes WHERE id_movement_type = @id_movement_type)
          AND status = 1 AND deleted_at IS NULL
    )
    BEGIN SELECT 0 AS success, N'No se puede restaurar: ya existe un tipo activo con el mismo nombre.' AS message; RETURN; END
    UPDATE MovementTypes SET status = 1, updated_at = GETDATE() WHERE id_movement_type = @id_movement_type;
    SELECT 1 AS success, N'Tipo de movimiento restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_movement_type_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_movement_type_delete_physical;
GO
CREATE PROCEDURE dbo.sp_movement_type_delete_physical @id_movement_type INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE id_movement_type = @id_movement_type AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM InventoryMovements WHERE id_movement_type = @id_movement_type)
    BEGIN SELECT 0 AS success, N'No se puede eliminar: el tipo tiene movimientos asociados.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM MovementTypes WHERE id_movement_type = @id_movement_type;
        SELECT 1 AS success, N'Tipo de movimiento eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, N'No se puede eliminar: el registro tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- WAREHOUSE DETAILS (consulta + métricas)
-- ############################################################

IF OBJECT_ID('dbo.sp_warehouse_detail_warehouse_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_detail_warehouse_list_active;
GO
CREATE PROCEDURE dbo.sp_warehouse_detail_warehouse_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_warehouse, name
    FROM Warehouses
    WHERE deleted_at IS NULL AND status = 1
    ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_detail_metrics', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_detail_metrics;
GO
CREATE PROCEDURE dbo.sp_warehouse_detail_metrics @id_warehouse INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM Warehouses WHERE deleted_at IS NULL AND status = 1
            AND (@id_warehouse IS NULL OR id_warehouse = @id_warehouse)) AS warehouse_count,
        ISNULL(SUM(wd.stock), 0) AS total_stock,
        ISNULL(SUM(wd.stock * p.cost), 0) AS total_cost_value,
        ISNULL(SUM(wd.stock * p.sale_price), 0) AS total_sale_value,
        COUNT(DISTINCT wd.id_product) AS product_count
    FROM WarehouseDetails wd
    INNER JOIN Products p ON p.id_product = wd.id_product
    INNER JOIN Warehouses w ON w.id_warehouse = wd.id_warehouse
    WHERE p.deleted_at IS NULL AND p.status = 1
      AND w.deleted_at IS NULL AND w.status = 1
      AND (@id_warehouse IS NULL OR wd.id_warehouse = @id_warehouse);
END
GO

IF OBJECT_ID('dbo.sp_warehouse_detail_summary_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_detail_summary_list;
GO
CREATE PROCEDURE dbo.sp_warehouse_detail_summary_list
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        w.id_warehouse,
        w.name AS warehouse_name,
        w.address,
        ISNULL(d.name, N'') AS district_name,
        COUNT(DISTINCT wd.id_product) AS product_count,
        ISNULL(SUM(wd.stock), 0) AS total_stock,
        ISNULL(SUM(wd.stock * p.cost), 0) AS total_cost_value,
        ISNULL(SUM(wd.stock * p.sale_price), 0) AS total_sale_value,
        COUNT(*) OVER() AS total_count
    FROM Warehouses w
    LEFT JOIN Districts d ON d.id_district = w.id_district
    LEFT JOIN WarehouseDetails wd ON wd.id_warehouse = w.id_warehouse
    LEFT JOIN Products p ON p.id_product = wd.id_product AND p.deleted_at IS NULL AND p.status = 1
    WHERE w.deleted_at IS NULL AND w.status = 1
      AND (@search IS NULL OR @search = ''
           OR w.name LIKE N'%' + @search + N'%'
           OR w.address LIKE N'%' + @search + N'%'
           OR d.name LIKE N'%' + @search + N'%')
    GROUP BY w.id_warehouse, w.name, w.address, d.name
    ORDER BY w.id_warehouse DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_detail_get_by_warehouse', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_detail_get_by_warehouse;
GO
CREATE PROCEDURE dbo.sp_warehouse_detail_get_by_warehouse @id_warehouse INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        w.id_warehouse,
        w.name AS warehouse_name,
        w.address,
        ISNULL(d.name, N'') AS district_name,
        w.status,
        w.created_at,
        w.updated_at
    FROM Warehouses w
    LEFT JOIN Districts d ON d.id_district = w.id_district
    WHERE w.id_warehouse = @id_warehouse AND w.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_detail_product_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_detail_product_list;
GO
CREATE PROCEDURE dbo.sp_warehouse_detail_product_list
    @id_warehouse INT,
    @search       VARCHAR(100) = NULL,
    @page         INT = 1,
    @page_size    INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        wd.id_warehouse_detail,
        wd.id_warehouse,
        wd.id_product,
        p.name AS product_name,
        b.name AS brand_name,
        c.name AS category_name,
        wd.stock,
        wd.location,
        p.cost,
        p.sale_price,
        (wd.stock * p.cost) AS line_cost_value,
        (wd.stock * p.sale_price) AS line_sale_value,
        COUNT(*) OVER() AS total_count
    FROM WarehouseDetails wd
    INNER JOIN Products p ON p.id_product = wd.id_product
    INNER JOIN Brands b ON b.id_brand = p.id_brand
    INNER JOIN Categories c ON c.id_category = p.id_category
    WHERE wd.id_warehouse = @id_warehouse
      AND p.deleted_at IS NULL AND p.status = 1
      AND (@search IS NULL OR @search = ''
           OR p.name LIKE N'%' + @search + N'%'
           OR b.name LIKE N'%' + @search + N'%'
           OR c.name LIKE N'%' + @search + N'%'
           OR wd.location LIKE N'%' + @search + N'%')
    ORDER BY p.name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_warehouse_detail_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_warehouse_detail_get_by_id;
GO
CREATE PROCEDURE dbo.sp_warehouse_detail_get_by_id @id_warehouse_detail INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        wd.id_warehouse_detail,
        wd.id_warehouse,
        w.name AS warehouse_name,
        wd.id_product,
        p.name AS product_name,
        b.name AS brand_name,
        c.name AS category_name,
        wd.stock,
        wd.location,
        p.cost,
        p.sale_price,
        (wd.stock * p.cost) AS line_cost_value,
        (wd.stock * p.sale_price) AS line_sale_value
    FROM WarehouseDetails wd
    INNER JOIN Warehouses w ON w.id_warehouse = wd.id_warehouse
    INNER JOIN Products p ON p.id_product = wd.id_product
    INNER JOIN Brands b ON b.id_brand = p.id_brand
    INNER JOIN Categories c ON c.id_category = p.id_category
    WHERE wd.id_warehouse_detail = @id_warehouse_detail;
END
GO

-- ############################################################
-- INVENTORY MOVEMENTS (consulta)
-- ############################################################

IF OBJECT_ID('dbo.sp_inventory_movement_filter_options', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_inventory_movement_filter_options;
GO
CREATE PROCEDURE dbo.sp_inventory_movement_filter_options
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_warehouse AS id, name FROM Warehouses WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
    SELECT id_product AS id, name FROM Products WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
    SELECT id_movement_type AS id, name FROM MovementTypes WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_inventory_movement_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_inventory_movement_list;
GO
CREATE PROCEDURE dbo.sp_inventory_movement_list
    @search             VARCHAR(100) = NULL,
    @id_warehouse       INT = NULL,
    @id_product         INT = NULL,
    @id_movement_type   INT = NULL,
    @movement_direction VARCHAR(10) = NULL,
    @page               INT = 1,
    @page_size          INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        im.id_inventory_movement,
        im.id_product,
        p.name AS product_name,
        im.id_warehouse,
        w.name AS warehouse_name,
        im.id_movement_type,
        mt.name AS movement_type_name,
        CASE
            WHEN mt.name LIKE N'%Entrada%' OR mt.name LIKE N'%entrada%' THEN N'entrada'
            ELSE N'salida'
        END AS movement_direction,
        im.quantity,
        im.reference,
        im.fec_movement,
        e.name + N' ' + e.last_name_paternal AS employee_name,
        COUNT(*) OVER() AS total_count
    FROM InventoryMovements im
    INNER JOIN Products p ON p.id_product = im.id_product
    INNER JOIN Warehouses w ON w.id_warehouse = im.id_warehouse
    INNER JOIN MovementTypes mt ON mt.id_movement_type = im.id_movement_type
    INNER JOIN Employees e ON e.id_employee = im.id_employee
    WHERE im.deleted_at IS NULL
      AND (@id_warehouse IS NULL OR im.id_warehouse = @id_warehouse)
      AND (@id_product IS NULL OR im.id_product = @id_product)
      AND (@id_movement_type IS NULL OR im.id_movement_type = @id_movement_type)
      AND (@movement_direction IS NULL OR @movement_direction = ''
           OR (@movement_direction = N'entrada' AND (mt.name LIKE N'%Entrada%' OR mt.name LIKE N'%entrada%'))
           OR (@movement_direction = N'salida' AND NOT (mt.name LIKE N'%Entrada%' OR mt.name LIKE N'%entrada%')))
      AND (@search IS NULL OR @search = ''
           OR CAST(im.id_inventory_movement AS VARCHAR(20)) LIKE N'%' + @search + N'%'
           OR p.name LIKE N'%' + @search + N'%'
           OR w.name LIKE N'%' + @search + N'%'
           OR mt.name LIKE N'%' + @search + N'%'
           OR im.reference LIKE N'%' + @search + N'%'
           OR e.name LIKE N'%' + @search + N'%'
           OR e.last_name_paternal LIKE N'%' + @search + N'%')
    ORDER BY im.id_inventory_movement DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_inventory_movement_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_inventory_movement_get_by_id;
GO
CREATE PROCEDURE dbo.sp_inventory_movement_get_by_id @id_inventory_movement INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        im.id_inventory_movement,
        im.id_product,
        p.name AS product_name,
        im.id_warehouse,
        w.name AS warehouse_name,
        im.id_movement_type,
        mt.name AS movement_type_name,
        CASE
            WHEN mt.name LIKE N'%Entrada%' OR mt.name LIKE N'%entrada%' THEN N'entrada'
            ELSE N'salida'
        END AS movement_direction,
        im.quantity,
        im.reference,
        im.fec_movement,
        im.id_employee,
        e.name + N' ' + e.last_name_paternal AS employee_name,
        u.username AS employee_username,
        im.created_at,
        im.updated_at
    FROM InventoryMovements im
    INNER JOIN Products p ON p.id_product = im.id_product
    INNER JOIN Warehouses w ON w.id_warehouse = im.id_warehouse
    INNER JOIN MovementTypes mt ON mt.id_movement_type = im.id_movement_type
    INNER JOIN Employees e ON e.id_employee = im.id_employee
    INNER JOIN Users u ON u.id_user = e.id_user
    WHERE im.id_inventory_movement = @id_inventory_movement AND im.deleted_at IS NULL;
END
GO

-- ============================================================
-- KMLLogistics - Stored Procedures: Compras
-- Módulos: PurchaseStatuses, Purchases, PurchaseDetails, PurchaseWarehouseDetails
-- Nomenclatura: sp_purchase_status_*, sp_purchase_*, sp_purchase_detail_*, sp_purchase_warehouse_detail_*
-- ============================================================

USE KMLLogistics;
GO

-- Datos base para movimientos y estados de compra
IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE name = N'Entrada por compra' AND deleted_at IS NULL)
    INSERT INTO MovementTypes (name) VALUES (N'Entrada por compra');
IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE name = N'Salida por anulación de compra' AND deleted_at IS NULL)
    INSERT INTO MovementTypes (name) VALUES (N'Salida por anulación de compra');
IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = N'Pendiente' AND deleted_at IS NULL)
    INSERT INTO PurchaseStatuses (name) VALUES (N'Pendiente');
IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = N'Completada' AND deleted_at IS NULL)
    INSERT INTO PurchaseStatuses (name) VALUES (N'Completada');
IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = N'Cancelada' AND deleted_at IS NULL)
    INSERT INTO PurchaseStatuses (name) VALUES (N'Cancelada');
GO

-- ############################################################
-- PURCHASE STATUSES
-- ############################################################

IF OBJECT_ID('dbo.sp_purchase_status_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_status_list_active;
GO
CREATE PROCEDURE dbo.sp_purchase_status_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT ps.id_purchase_status, ps.name, COUNT(*) OVER() AS total_count
    FROM PurchaseStatuses ps
    WHERE ps.deleted_at IS NULL AND ps.status = 1
      AND (@search IS NULL OR @search = '' OR ps.name LIKE N'%' + @search + N'%')
    ORDER BY ps.id_purchase_status DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_purchase_status_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_status_list_inactive;
GO
CREATE PROCEDURE dbo.sp_purchase_status_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT ps.id_purchase_status, ps.name, COUNT(*) OVER() AS total_count
    FROM PurchaseStatuses ps
    WHERE ps.deleted_at IS NULL AND ps.status = 0
      AND (@search IS NULL OR @search = '' OR ps.name LIKE N'%' + @search + N'%')
    ORDER BY ps.id_purchase_status DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_purchase_status_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_status_get_by_id;
GO
CREATE PROCEDURE dbo.sp_purchase_status_get_by_id @id_purchase_status INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_purchase_status, name, status, created_at, updated_at
    FROM PurchaseStatuses
    WHERE id_purchase_status = @id_purchase_status AND deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_purchase_status_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_status_create;
GO
CREATE PROCEDURE dbo.sp_purchase_status_create @name VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Ya existe un estado con ese nombre.' AS message, NULL AS id_purchase_status; RETURN; END
    INSERT INTO PurchaseStatuses (name) VALUES (@name);
    SELECT 1 AS success, N'Estado creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_purchase_status;
END
GO

IF OBJECT_ID('dbo.sp_purchase_status_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_status_update;
GO
CREATE PROCEDURE dbo.sp_purchase_status_update
    @id_purchase_status INT,
    @name               VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE id_purchase_status = @id_purchase_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = @name AND id_purchase_status <> @id_purchase_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Ya existe otro estado con ese nombre.' AS message; RETURN; END
    UPDATE PurchaseStatuses SET name = @name, updated_at = GETDATE() WHERE id_purchase_status = @id_purchase_status;
    SELECT 1 AS success, N'Estado actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_purchase_status_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_status_delete_logic;
GO
CREATE PROCEDURE dbo.sp_purchase_status_delete_logic @id_purchase_status INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE id_purchase_status = @id_purchase_status AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado o ya inactivo.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Purchases WHERE id_purchase_status = @id_purchase_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'No se puede desactivar: el estado está en uso en compras.' AS message; RETURN; END
    UPDATE PurchaseStatuses SET status = 0, updated_at = GETDATE() WHERE id_purchase_status = @id_purchase_status;
    SELECT 1 AS success, N'Estado desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_purchase_status_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_status_restore;
GO
CREATE PROCEDURE dbo.sp_purchase_status_restore @id_purchase_status INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE id_purchase_status = @id_purchase_status AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado o ya activo.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM PurchaseStatuses WHERE name = (SELECT name FROM PurchaseStatuses WHERE id_purchase_status = @id_purchase_status) AND status = 1 AND deleted_at IS NULL AND id_purchase_status <> @id_purchase_status)
    BEGIN SELECT 0 AS success, N'Ya existe un estado activo con ese nombre.' AS message; RETURN; END
    UPDATE PurchaseStatuses SET status = 1, updated_at = GETDATE() WHERE id_purchase_status = @id_purchase_status;
    SELECT 1 AS success, N'Estado restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_purchase_status_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_status_delete_physical;
GO
CREATE PROCEDURE dbo.sp_purchase_status_delete_physical @id_purchase_status INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM PurchaseStatuses WHERE id_purchase_status = @id_purchase_status AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Purchases WHERE id_purchase_status = @id_purchase_status)
    BEGIN SELECT 0 AS success, N'No se puede eliminar: el estado tiene compras asociadas.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM PurchaseStatuses WHERE id_purchase_status = @id_purchase_status;
        SELECT 1 AS success, N'Estado eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, N'No se puede eliminar: el estado tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- PURCHASES - Lookups
-- ############################################################

IF OBJECT_ID('dbo.sp_purchase_supplier_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_supplier_list_active;
GO
CREATE PROCEDURE dbo.sp_purchase_supplier_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_supplier, name FROM Suppliers WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_purchase_employee_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_employee_list_active;
GO
CREATE PROCEDURE dbo.sp_purchase_employee_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.id_employee, u.username + N' - ' + e.name + N' ' + e.last_name_paternal AS name
    FROM Employees e
    INNER JOIN Users u ON u.id_user = e.id_user
    WHERE e.deleted_at IS NULL AND e.status = 1 AND u.deleted_at IS NULL
    ORDER BY u.username;
END
GO

IF OBJECT_ID('dbo.sp_purchase_warehouse_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_warehouse_list_active;
GO
CREATE PROCEDURE dbo.sp_purchase_warehouse_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_warehouse, name FROM Warehouses WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_purchase_product_supplier_list_by_supplier', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_product_supplier_list_by_supplier;
GO
CREATE PROCEDURE dbo.sp_purchase_product_supplier_list_by_supplier @id_supplier INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ps.id_product_supplier,
        p.name AS product_name,
        ps.supplier_cost
    FROM ProductSuppliers ps
    INNER JOIN Products p ON p.id_product = ps.id_product
    WHERE ps.id_supplier = @id_supplier
      AND ps.deleted_at IS NULL AND ps.status = 1
      AND p.deleted_at IS NULL AND p.status = 1
    ORDER BY p.name;
END
GO

-- ############################################################
-- PURCHASES - CRUD
-- ############################################################

IF OBJECT_ID('dbo.sp_purchase_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_list;
GO
CREATE PROCEDURE dbo.sp_purchase_list
    @search              VARCHAR(100) = NULL,
    @id_purchase         INT = NULL,
    @id_supplier         INT = NULL,
    @id_employee         INT = NULL,
    @id_purchase_status  INT = NULL,
    @page                INT = 1,
    @page_size           INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        pu.id_purchase,
        pu.fec_purchase,
        s.name AS supplier_name,
        e.name + N' ' + e.last_name_paternal AS employee_name,
        ps.name AS purchase_status_name,
        pu.subtotal,
        pu.tax,
        pu.total,
        COUNT(*) OVER() AS total_count
    FROM Purchases pu
    INNER JOIN Suppliers s ON s.id_supplier = pu.id_supplier
    INNER JOIN Employees e ON e.id_employee = pu.id_employee
    INNER JOIN PurchaseStatuses ps ON ps.id_purchase_status = pu.id_purchase_status
    WHERE pu.deleted_at IS NULL
      AND (@id_purchase IS NULL OR pu.id_purchase = @id_purchase)
      AND (@id_supplier IS NULL OR pu.id_supplier = @id_supplier)
      AND (@id_employee IS NULL OR pu.id_employee = @id_employee)
      AND (@id_purchase_status IS NULL OR pu.id_purchase_status = @id_purchase_status)
      AND (@search IS NULL OR @search = ''
           OR s.name LIKE N'%' + @search + N'%'
           OR e.name LIKE N'%' + @search + N'%'
           OR e.last_name_paternal LIKE N'%' + @search + N'%'
           OR ps.name LIKE N'%' + @search + N'%')
    ORDER BY pu.id_purchase DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_purchase_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_get_by_id;
GO
CREATE PROCEDURE dbo.sp_purchase_get_by_id @id_purchase INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pu.id_purchase,
        pu.id_supplier,
        s.name AS supplier_name,
        pu.id_employee,
        e.name + N' ' + e.last_name_paternal AS employee_name,
        u.username AS employee_username,
        pu.id_purchase_status,
        ps.name AS purchase_status_name,
        pu.fec_purchase,
        pu.subtotal,
        pu.tax,
        pu.total,
        pu.created_at,
        pu.updated_at
    FROM Purchases pu
    INNER JOIN Suppliers s ON s.id_supplier = pu.id_supplier
    INNER JOIN Employees e ON e.id_employee = pu.id_employee
    INNER JOIN Users u ON u.id_user = e.id_user
    INNER JOIN PurchaseStatuses ps ON ps.id_purchase_status = pu.id_purchase_status
    WHERE pu.id_purchase = @id_purchase AND pu.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_purchase_detail_lines_by_purchase', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_detail_lines_by_purchase;
GO
CREATE PROCEDURE dbo.sp_purchase_detail_lines_by_purchase @id_purchase INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pd.id_purchase_detail,
        pd.id_product_supplier,
        p.name AS product_name,
        pd.quantity,
        pd.unit_cost,
        pd.subtotal
    FROM PurchaseDetails pd
    INNER JOIN ProductSuppliers ps ON ps.id_product_supplier = pd.id_product_supplier
    INNER JOIN Products p ON p.id_product = ps.id_product
    WHERE pd.id_purchase = @id_purchase
    ORDER BY pd.id_purchase_detail;
END
GO

IF OBJECT_ID('dbo.sp_purchase_warehouse_lines_by_purchase', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_warehouse_lines_by_purchase;
GO
CREATE PROCEDURE dbo.sp_purchase_warehouse_lines_by_purchase @id_purchase INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pwd.id_purchase_warehouse_detail,
        pwd.id_purchase_detail,
        pwd.id_warehouse,
        w.name AS warehouse_name,
        p.name AS product_name,
        pwd.quantity
    FROM PurchaseWarehouseDetails pwd
    INNER JOIN PurchaseDetails pd ON pd.id_purchase_detail = pwd.id_purchase_detail
    INNER JOIN ProductSuppliers ps ON ps.id_product_supplier = pd.id_product_supplier
    INNER JOIN Products p ON p.id_product = ps.id_product
    INNER JOIN Warehouses w ON w.id_warehouse = pwd.id_warehouse
    WHERE pd.id_purchase = @id_purchase
    ORDER BY pwd.id_purchase_warehouse_detail;
END
GO

IF OBJECT_ID('dbo.sp_purchase_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_create;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE dbo.sp_purchase_create
    @id_supplier  INT,
    @id_employee  INT,
    @fec_purchase DATETIME,
    @details_json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_purchase_status INT;
    DECLARE @id_purchase INT;
    DECLARE @subtotal DECIMAL(10,2);
    DECLARE @tax DECIMAL(10,2);
    DECLARE @total DECIMAL(10,2);

    IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE id_supplier = @id_supplier AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS success, N'Proveedor no válido.' AS message, NULL AS id_purchase; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Employees WHERE id_employee = @id_employee AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS success, N'Empleado no válido.' AS message, NULL AS id_purchase; RETURN; END

    IF @details_json IS NULL OR LTRIM(RTRIM(@details_json)) = '' OR @details_json = '[]'
    BEGIN SELECT 0 AS success, N'Debe agregar al menos un producto a la compra.' AS message, NULL AS id_purchase; RETURN; END

    SELECT @id_purchase_status = id_purchase_status FROM PurchaseStatuses WHERE name = N'Pendiente' AND deleted_at IS NULL AND status = 1;
    IF @id_purchase_status IS NULL
    BEGIN SELECT 0 AS success, N'No existe el estado Pendiente. Ejecute el script de compras.' AS message, NULL AS id_purchase; RETURN; END

    IF EXISTS (
        SELECT 1 FROM OPENJSON(@details_json) WITH (
            id_product_supplier INT '$.idProductSupplier',
            quantity INT '$.quantity',
            unit_cost DECIMAL(10,2) '$.unitCost',
            id_warehouse INT '$.idWarehouse'
        ) j
        WHERE j.id_product_supplier IS NULL OR j.quantity IS NULL OR j.quantity <= 0
           OR j.unit_cost IS NULL OR j.unit_cost < 0 OR j.id_warehouse IS NULL
    )
    BEGIN SELECT 0 AS success, N'Todas las líneas deben tener producto-proveedor, almacén, cantidad mayor a cero y costo válido.' AS message, NULL AS id_purchase; RETURN; END

    IF EXISTS (
        SELECT id_product_supplier FROM OPENJSON(@details_json) WITH (
            id_product_supplier INT '$.idProductSupplier',
            unit_cost DECIMAL(10,2) '$.unitCost'
        ) j
        GROUP BY j.id_product_supplier HAVING COUNT(DISTINCT j.unit_cost) > 1
    )
    BEGIN SELECT 0 AS success, N'Un mismo producto no puede tener distinto costo unitario en la compra.' AS message, NULL AS id_purchase; RETURN; END

    IF EXISTS (
        SELECT id_product_supplier, id_warehouse FROM OPENJSON(@details_json) WITH (
            id_product_supplier INT '$.idProductSupplier',
            id_warehouse INT '$.idWarehouse'
        ) j
        GROUP BY j.id_product_supplier, j.id_warehouse HAVING COUNT(*) > 1
    )
    BEGIN SELECT 0 AS success, N'No repita el mismo almacén para un producto en la compra.' AS message, NULL AS id_purchase; RETURN; END

    IF EXISTS (
        SELECT 1 FROM OPENJSON(@details_json) WITH (id_product_supplier INT '$.idProductSupplier') j
        LEFT JOIN ProductSuppliers ps ON ps.id_product_supplier = j.id_product_supplier
            AND ps.id_supplier = @id_supplier AND ps.deleted_at IS NULL AND ps.status = 1
        WHERE ps.id_product_supplier IS NULL
    )
    BEGIN SELECT 0 AS success, N'Uno o más productos no pertenecen al proveedor seleccionado.' AS message, NULL AS id_purchase; RETURN; END

    IF EXISTS (
        SELECT 1 FROM OPENJSON(@details_json) WITH (id_warehouse INT '$.idWarehouse') j
        LEFT JOIN Warehouses w ON w.id_warehouse = j.id_warehouse AND w.deleted_at IS NULL AND w.status = 1
        WHERE w.id_warehouse IS NULL
    )
    BEGIN SELECT 0 AS success, N'Uno o más almacenes no son válidos.' AS message, NULL AS id_purchase; RETURN; END

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Purchases (id_supplier, id_employee, id_purchase_status, fec_purchase, subtotal, tax, total)
        VALUES (@id_supplier, @id_employee, @id_purchase_status, @fec_purchase, 0, 0, 0);
        SET @id_purchase = SCOPE_IDENTITY();

        INSERT INTO PurchaseDetails (id_purchase, id_product_supplier, quantity, unit_cost)
        SELECT @id_purchase, j.id_product_supplier, SUM(j.quantity), j.unit_cost
        FROM OPENJSON(@details_json) WITH (
            id_product_supplier INT '$.idProductSupplier',
            quantity INT '$.quantity',
            unit_cost DECIMAL(10,2) '$.unitCost',
            id_warehouse INT '$.idWarehouse'
        ) j
        GROUP BY j.id_product_supplier, j.unit_cost;

        INSERT INTO PurchaseWarehouseDetails (id_purchase_detail, id_warehouse, quantity)
        SELECT pd.id_purchase_detail, j.id_warehouse, j.quantity
        FROM OPENJSON(@details_json) WITH (
            id_product_supplier INT '$.idProductSupplier',
            quantity INT '$.quantity',
            unit_cost DECIMAL(10,2) '$.unitCost',
            id_warehouse INT '$.idWarehouse'
        ) j
        INNER JOIN PurchaseDetails pd ON pd.id_purchase = @id_purchase
            AND pd.id_product_supplier = j.id_product_supplier
            AND pd.unit_cost = j.unit_cost;

        SELECT @subtotal = SUM(CAST(pd.quantity AS DECIMAL(10,2)) * pd.unit_cost)
        FROM PurchaseDetails pd WHERE pd.id_purchase = @id_purchase;

        SET @tax   = ROUND(@subtotal * 0.18, 2);
        SET @total = @subtotal + @tax;

        UPDATE Purchases SET subtotal = @subtotal, tax = @tax, total = @total, updated_at = GETDATE()
        WHERE id_purchase = @id_purchase;

        COMMIT TRANSACTION;
        SELECT 1 AS success, N'Compra registrada como Pendiente. Complete la compra para distribuir el stock.' AS message, @id_purchase AS id_purchase;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS success, N'Error al registrar la compra: ' + ERROR_MESSAGE() AS message, NULL AS id_purchase;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_purchase_complete', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_complete;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE dbo.sp_purchase_complete @id_purchase INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status_name NVARCHAR(50);
    DECLARE @id_status_complete INT;
    DECLARE @id_movement_in INT;
    DECLARE @id_employee INT;
    DECLARE @fec_purchase DATETIME;
    DECLARE @id_warehouse INT;
    DECLARE @id_product INT;
    DECLARE @id_product_supplier INT;
    DECLARE @quantity INT;
    DECLARE @unit_cost DECIMAL(10,2);

    IF NOT EXISTS (SELECT 1 FROM Purchases WHERE id_purchase = @id_purchase AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Compra no encontrada.' AS message; RETURN; END

    SELECT @status_name = ps.name, @id_employee = pu.id_employee, @fec_purchase = pu.fec_purchase
    FROM Purchases pu
    INNER JOIN PurchaseStatuses ps ON ps.id_purchase_status = pu.id_purchase_status
    WHERE pu.id_purchase = @id_purchase;

    IF @status_name = N'Completada'
    BEGIN SELECT 0 AS success, N'La compra ya está completada.' AS message; RETURN; END

    IF @status_name = N'Cancelada'
    BEGIN SELECT 0 AS success, N'No se puede completar una compra cancelada.' AS message; RETURN; END

    IF @status_name <> N'Pendiente'
    BEGIN SELECT 0 AS success, N'Solo se pueden completar compras en estado Pendiente.' AS message; RETURN; END

    SELECT @id_status_complete = id_purchase_status FROM PurchaseStatuses WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;
    IF @id_status_complete IS NULL
    BEGIN SELECT 0 AS success, N'No existe el estado Completada.' AS message; RETURN; END

    SELECT @id_movement_in = id_movement_type FROM MovementTypes WHERE name = N'Entrada por compra' AND deleted_at IS NULL AND status = 1;
    IF @id_movement_in IS NULL
    BEGIN SELECT 0 AS success, N'No existe el tipo de movimiento Entrada por compra.' AS message; RETURN; END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
            SELECT pwd.id_warehouse, ps.id_product, pwd.quantity, pd.id_product_supplier, pd.unit_cost
            FROM PurchaseWarehouseDetails pwd
            INNER JOIN PurchaseDetails pd ON pd.id_purchase_detail = pwd.id_purchase_detail
            INNER JOIN ProductSuppliers ps ON ps.id_product_supplier = pd.id_product_supplier
            WHERE pd.id_purchase = @id_purchase;

        OPEN cur;
        FETCH NEXT FROM cur INTO @id_warehouse, @id_product, @quantity, @id_product_supplier, @unit_cost;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_warehouse = @id_warehouse AND id_product = @id_product)
                UPDATE WarehouseDetails SET stock = stock + @quantity
                WHERE id_warehouse = @id_warehouse AND id_product = @id_product;
            ELSE
                INSERT INTO WarehouseDetails (id_warehouse, id_product, stock) VALUES (@id_warehouse, @id_product, @quantity);

            INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
            VALUES (@id_product, @id_warehouse, @id_movement_in, @id_employee, @quantity,
                    N'COM-' + CAST(@id_purchase AS NVARCHAR(20)), @fec_purchase);

            UPDATE ProductSuppliers SET last_purchase_cost = @unit_cost, updated_at = GETDATE()
            WHERE id_product_supplier = @id_product_supplier;

            FETCH NEXT FROM cur INTO @id_warehouse, @id_product, @quantity, @id_product_supplier, @unit_cost;
        END
        CLOSE cur;
        DEALLOCATE cur;

        UPDATE Purchases SET id_purchase_status = @id_status_complete, updated_at = GETDATE() WHERE id_purchase = @id_purchase;

        COMMIT TRANSACTION;
        SELECT 1 AS success, N'Compra completada. El stock fue distribuido a los almacenes.' AS message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        IF CURSOR_STATUS('local', 'cur') >= 0 BEGIN CLOSE cur; DEALLOCATE cur; END
        SELECT 0 AS success, N'Error al completar la compra: ' + ERROR_MESSAGE() AS message;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_purchase_cancel', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_cancel;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE dbo.sp_purchase_cancel @id_purchase INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_status_cancel INT;
    DECLARE @status_name NVARCHAR(50);
    DECLARE @id_employee INT;
    DECLARE @fec_purchase DATETIME;
    DECLARE @id_movement_out INT;
    DECLARE @id_warehouse INT;
    DECLARE @id_product INT;
    DECLARE @quantity INT;
    DECLARE @stock INT;

    IF NOT EXISTS (SELECT 1 FROM Purchases WHERE id_purchase = @id_purchase AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Compra no encontrada.' AS message; RETURN; END

    SELECT @status_name = ps.name, @id_employee = pu.id_employee, @fec_purchase = pu.fec_purchase
    FROM Purchases pu
    INNER JOIN PurchaseStatuses ps ON ps.id_purchase_status = pu.id_purchase_status
    WHERE pu.id_purchase = @id_purchase;

    IF @status_name = N'Cancelada'
    BEGIN SELECT 0 AS success, N'La compra ya está cancelada.' AS message; RETURN; END

    SELECT @id_status_cancel = id_purchase_status FROM PurchaseStatuses WHERE name = N'Cancelada' AND deleted_at IS NULL AND status = 1;
    IF @id_status_cancel IS NULL
    BEGIN SELECT 0 AS success, N'No existe el estado Cancelada.' AS message; RETURN; END

    SELECT @id_movement_out = id_movement_type FROM MovementTypes WHERE name = N'Salida por anulación de compra' AND deleted_at IS NULL AND status = 1;
    IF @id_movement_out IS NULL
    BEGIN SELECT 0 AS success, N'No existe el tipo de movimiento Salida por anulación de compra.' AS message; RETURN; END

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @status_name = N'Completada'
        BEGIN
            DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
                SELECT pwd.id_warehouse, ps.id_product, pwd.quantity
                FROM PurchaseWarehouseDetails pwd
                INNER JOIN PurchaseDetails pd ON pd.id_purchase_detail = pwd.id_purchase_detail
                INNER JOIN ProductSuppliers ps ON ps.id_product_supplier = pd.id_product_supplier
                WHERE pd.id_purchase = @id_purchase;

            OPEN cur;
            FETCH NEXT FROM cur INTO @id_warehouse, @id_product, @quantity;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SELECT @stock = stock FROM WarehouseDetails WHERE id_warehouse = @id_warehouse AND id_product = @id_product;
                IF @stock IS NULL OR @stock < @quantity
                BEGIN
                    ROLLBACK TRANSACTION;
                    CLOSE cur; DEALLOCATE cur;
                    SELECT 0 AS success, N'No hay stock suficiente para revertir la compra.' AS message;
                    RETURN;
                END

                UPDATE WarehouseDetails SET stock = stock - @quantity
                WHERE id_warehouse = @id_warehouse AND id_product = @id_product;

                INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
                VALUES (@id_product, @id_warehouse, @id_movement_out, @id_employee, @quantity,
                        N'COM-CAN-' + CAST(@id_purchase AS NVARCHAR(20)), GETDATE());

                FETCH NEXT FROM cur INTO @id_warehouse, @id_product, @quantity;
            END
            CLOSE cur;
            DEALLOCATE cur;
        END

        UPDATE Purchases SET id_purchase_status = @id_status_cancel, updated_at = GETDATE() WHERE id_purchase = @id_purchase;

        COMMIT TRANSACTION;
        SELECT 1 AS success,
            CASE WHEN @status_name = N'Completada'
                 THEN N'Compra cancelada correctamente. El stock fue revertido.'
                 ELSE N'Compra cancelada correctamente.' END AS message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS success, N'Error al cancelar: ' + ERROR_MESSAGE() AS message;
    END CATCH
END
GO

-- ############################################################
-- PURCHASE DETAILS - Consulta
-- ############################################################

IF OBJECT_ID('dbo.sp_purchase_detail_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_detail_list;
GO
CREATE PROCEDURE dbo.sp_purchase_detail_list
    @search              VARCHAR(100) = NULL,
    @id_purchase         INT = NULL,
    @id_product          INT = NULL,
    @id_supplier         INT = NULL,
    @id_purchase_status  INT = NULL,
    @page                INT = 1,
    @page_size           INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        pd.id_purchase_detail,
        pd.id_purchase,
        p.name AS product_name,
        s.name AS supplier_name,
        pd.quantity,
        pd.unit_cost,
        pd.subtotal,
        ps2.name AS purchase_status_name,
        pu.fec_purchase,
        COUNT(*) OVER() AS total_count
    FROM PurchaseDetails pd
    INNER JOIN Purchases pu ON pu.id_purchase = pd.id_purchase
    INNER JOIN ProductSuppliers ps ON ps.id_product_supplier = pd.id_product_supplier
    INNER JOIN Products p ON p.id_product = ps.id_product
    INNER JOIN Suppliers s ON s.id_supplier = pu.id_supplier
    INNER JOIN PurchaseStatuses ps2 ON ps2.id_purchase_status = pu.id_purchase_status
    WHERE pu.deleted_at IS NULL
      AND (@id_purchase IS NULL OR pd.id_purchase = @id_purchase)
      AND (@id_product IS NULL OR ps.id_product = @id_product)
      AND (@id_supplier IS NULL OR pu.id_supplier = @id_supplier)
      AND (@id_purchase_status IS NULL OR pu.id_purchase_status = @id_purchase_status)
      AND (@search IS NULL OR @search = ''
           OR CAST(pd.id_purchase_detail AS VARCHAR(20)) LIKE N'%' + @search + N'%'
           OR p.name LIKE N'%' + @search + N'%'
           OR s.name LIKE N'%' + @search + N'%'
           OR ps2.name LIKE N'%' + @search + N'%')
    ORDER BY pd.id_purchase_detail DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_purchase_detail_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_detail_get_by_id;
GO
CREATE PROCEDURE dbo.sp_purchase_detail_get_by_id @id_purchase_detail INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pd.id_purchase_detail,
        pd.id_purchase,
        pd.id_product_supplier,
        ps.id_product,
        p.name AS product_name,
        pu.id_supplier,
        s.name AS supplier_name,
        pd.quantity,
        pd.unit_cost,
        pd.subtotal,
        pu.id_purchase_status,
        pst.name AS purchase_status_name,
        pu.fec_purchase,
        pu.id_employee,
        u.username AS employee_username,
        e.name + N' ' + e.last_name_paternal AS employee_name,
        pu.created_at AS purchase_created_at
    FROM PurchaseDetails pd
    INNER JOIN Purchases pu ON pu.id_purchase = pd.id_purchase
    INNER JOIN ProductSuppliers ps ON ps.id_product_supplier = pd.id_product_supplier
    INNER JOIN Products p ON p.id_product = ps.id_product
    INNER JOIN Suppliers s ON s.id_supplier = pu.id_supplier
    INNER JOIN PurchaseStatuses pst ON pst.id_purchase_status = pu.id_purchase_status
    INNER JOIN Employees e ON e.id_employee = pu.id_employee
    INNER JOIN Users u ON u.id_user = e.id_user
    WHERE pd.id_purchase_detail = @id_purchase_detail AND pu.deleted_at IS NULL;
END
GO

-- ############################################################
-- PURCHASE WAREHOUSE DETAILS - Consulta
-- ############################################################

IF OBJECT_ID('dbo.sp_purchase_warehouse_detail_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_warehouse_detail_list;
GO
CREATE PROCEDURE dbo.sp_purchase_warehouse_detail_list
    @search      VARCHAR(100) = NULL,
    @id_purchase INT = NULL,
    @id_product  INT = NULL,
    @id_warehouse INT = NULL,
    @id_supplier INT = NULL,
    @page        INT = 1,
    @page_size   INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        pwd.id_purchase_warehouse_detail,
        pd.id_purchase,
        pd.id_purchase_detail,
        p.name AS product_name,
        w.name AS warehouse_name,
        s.name AS supplier_name,
        pwd.quantity,
        pu.fec_purchase,
        COUNT(*) OVER() AS total_count
    FROM PurchaseWarehouseDetails pwd
    INNER JOIN PurchaseDetails pd ON pd.id_purchase_detail = pwd.id_purchase_detail
    INNER JOIN Purchases pu ON pu.id_purchase = pd.id_purchase
    INNER JOIN PurchaseStatuses pst ON pst.id_purchase_status = pu.id_purchase_status
    INNER JOIN ProductSuppliers ps ON ps.id_product_supplier = pd.id_product_supplier
    INNER JOIN Products p ON p.id_product = ps.id_product
    INNER JOIN Warehouses w ON w.id_warehouse = pwd.id_warehouse
    INNER JOIN Suppliers s ON s.id_supplier = pu.id_supplier
    WHERE pu.deleted_at IS NULL
      AND pst.name = N'Completada'
      AND (@id_purchase IS NULL OR pd.id_purchase = @id_purchase)
      AND (@id_product IS NULL OR ps.id_product = @id_product)
      AND (@id_warehouse IS NULL OR pwd.id_warehouse = @id_warehouse)
      AND (@id_supplier IS NULL OR pu.id_supplier = @id_supplier)
      AND (@search IS NULL OR @search = ''
           OR CAST(pwd.id_purchase_warehouse_detail AS VARCHAR(20)) LIKE N'%' + @search + N'%'
           OR p.name LIKE N'%' + @search + N'%'
           OR w.name LIKE N'%' + @search + N'%'
           OR s.name LIKE N'%' + @search + N'%')
    ORDER BY pwd.id_purchase_warehouse_detail DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_purchase_warehouse_detail_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_purchase_warehouse_detail_get_by_id;
GO
CREATE PROCEDURE dbo.sp_purchase_warehouse_detail_get_by_id @id_purchase_warehouse_detail INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pwd.id_purchase_warehouse_detail,
        pwd.id_purchase_detail,
        pd.id_purchase,
        pwd.id_warehouse,
        w.name AS warehouse_name,
        ps.id_product,
        p.name AS product_name,
        pwd.quantity,
        pu.id_supplier,
        s.name AS supplier_name,
        pu.fec_purchase,
        pu.id_purchase_status,
        pst.name AS purchase_status_name,
        pu.id_employee,
        u.username AS employee_username,
        e.name + N' ' + e.last_name_paternal AS employee_name,
        pu.created_at AS purchase_created_at
    FROM PurchaseWarehouseDetails pwd
    INNER JOIN PurchaseDetails pd ON pd.id_purchase_detail = pwd.id_purchase_detail
    INNER JOIN Purchases pu ON pu.id_purchase = pd.id_purchase
    INNER JOIN ProductSuppliers ps ON ps.id_product_supplier = pd.id_product_supplier
    INNER JOIN Products p ON p.id_product = ps.id_product
    INNER JOIN Warehouses w ON w.id_warehouse = pwd.id_warehouse
    INNER JOIN Suppliers s ON s.id_supplier = pu.id_supplier
    INNER JOIN PurchaseStatuses pst ON pst.id_purchase_status = pu.id_purchase_status
    INNER JOIN Employees e ON e.id_employee = pu.id_employee
    INNER JOIN Users u ON u.id_user = e.id_user
    WHERE pwd.id_purchase_warehouse_detail = @id_purchase_warehouse_detail AND pu.deleted_at IS NULL;
END
GO
