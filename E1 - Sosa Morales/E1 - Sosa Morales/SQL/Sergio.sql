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
