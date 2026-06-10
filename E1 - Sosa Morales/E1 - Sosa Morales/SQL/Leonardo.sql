USE KMLLogistics;
GO

/* =========================
   TIPOS VEHICULO
========================= */

IF OBJECT_ID('dbo.sp_vehicle_type_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_type_list_active;
GO
CREATE PROCEDURE dbo.sp_vehicle_type_list_active
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    WITH Filtered AS (
        SELECT id_vehicle_type, name, description
        FROM VehicleTypes
        WHERE deleted_at IS NULL
          AND status = 1
          AND (
                @search IS NULL OR @search = ''
             OR name LIKE '%' + @search + '%'
             OR description LIKE '%' + @search + '%'
          )
    )
    SELECT
        f.id_vehicle_type AS IdVehicleType,
        f.name AS Name,
        f.description AS Description,
        COUNT(v.id_vehicle) AS VehicleCount,
        COUNT(*) OVER() AS TotalCount
    FROM Filtered f
    LEFT JOIN Vehicles v
        ON v.id_vehicle_type = f.id_vehicle_type
       AND v.deleted_at IS NULL
    GROUP BY f.id_vehicle_type, f.name, f.description
    ORDER BY f.id_vehicle_type DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_type_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_type_list_inactive;
GO
CREATE PROCEDURE dbo.sp_vehicle_type_list_inactive
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    WITH Filtered AS (
        SELECT id_vehicle_type, name, description
        FROM VehicleTypes
        WHERE deleted_at IS NULL
          AND status = 0
          AND (
                @search IS NULL OR @search = ''
             OR name LIKE '%' + @search + '%'
             OR description LIKE '%' + @search + '%'
          )
    )
    SELECT
        f.id_vehicle_type AS IdVehicleType,
        f.name AS Name,
        f.description AS Description,
        COUNT(v.id_vehicle) AS VehicleCount,
        COUNT(*) OVER() AS TotalCount
    FROM Filtered f
    LEFT JOIN Vehicles v
        ON v.id_vehicle_type = f.id_vehicle_type
       AND v.deleted_at IS NULL
    GROUP BY f.id_vehicle_type, f.name, f.description
    ORDER BY f.id_vehicle_type DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_type_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_type_get_by_id;
GO
CREATE PROCEDURE dbo.sp_vehicle_type_get_by_id
    @id_vehicle_type INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        vt.id_vehicle_type AS IdVehicleType,
        vt.name AS Name,
        vt.description AS Description,
        COUNT(v.id_vehicle) AS VehicleCount,
        vt.status AS Status,
        vt.created_at AS CreatedAt,
        vt.updated_at AS UpdatedAt
    FROM VehicleTypes vt
    LEFT JOIN Vehicles v
        ON v.id_vehicle_type = vt.id_vehicle_type
       AND v.deleted_at IS NULL
    WHERE vt.id_vehicle_type = @id_vehicle_type
      AND vt.deleted_at IS NULL
    GROUP BY
        vt.id_vehicle_type,
        vt.name,
        vt.description,
        vt.status,
        vt.created_at,
        vt.updated_at;
END
GO

/* =========================
   VEHICULOS
========================= */

IF OBJECT_ID('dbo.sp_vehicle_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_list_active;
GO
CREATE PROCEDURE dbo.sp_vehicle_list_active
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10,
    @id_vehicle_type INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.id_vehicle AS IdVehicle,
        v.id_vehicle_type AS IdVehicleType,
        vt.name AS VehicleTypeName,
        v.plate AS Plate,
        v.maximum_weight AS MaximumWeight,
        v.maximum_volume AS MaximumVolume,
        v.status AS Status,
        COUNT(*) OVER() AS TotalCount
    FROM Vehicles v
    INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    WHERE v.deleted_at IS NULL
      AND v.status = 1
      AND (@id_vehicle_type IS NULL OR v.id_vehicle_type = @id_vehicle_type)
      AND (
            @search IS NULL OR @search = ''
         OR v.plate LIKE '%' + @search + '%'
         OR vt.name LIKE '%' + @search + '%'
      )
    ORDER BY v.id_vehicle DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_list_inactive;
GO
CREATE PROCEDURE dbo.sp_vehicle_list_inactive
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10,
    @id_vehicle_type INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.id_vehicle AS IdVehicle,
        v.id_vehicle_type AS IdVehicleType,
        vt.name AS VehicleTypeName,
        v.plate AS Plate,
        v.maximum_weight AS MaximumWeight,
        v.maximum_volume AS MaximumVolume,
        v.status AS Status,
        COUNT(*) OVER() AS TotalCount
    FROM Vehicles v
    INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    WHERE v.deleted_at IS NULL
      AND v.status = 0
      AND (@id_vehicle_type IS NULL OR v.id_vehicle_type = @id_vehicle_type)
      AND (
            @search IS NULL OR @search = ''
         OR v.plate LIKE '%' + @search + '%'
         OR vt.name LIKE '%' + @search + '%'
      )
    ORDER BY v.id_vehicle DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_type_options', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_type_options;
GO
CREATE PROCEDURE dbo.sp_vehicle_type_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_vehicle_type AS IdVehicleType,
        name AS Name
    FROM VehicleTypes
    WHERE deleted_at IS NULL
      AND status = 1
    ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_get_by_id;
GO
CREATE PROCEDURE dbo.sp_vehicle_get_by_id
    @id_vehicle INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.id_vehicle AS IdVehicle,
        v.id_vehicle_type AS IdVehicleType,
        vt.name AS VehicleTypeName,
        v.plate AS Plate,
        v.maximum_weight AS MaximumWeight,
        v.maximum_volume AS MaximumVolume,
        v.status AS Status,
        v.created_at AS CreatedAt,
        v.updated_at AS UpdatedAt
    FROM Vehicles v
    INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    WHERE v.id_vehicle = @id_vehicle
      AND v.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_create;
GO
CREATE PROCEDURE dbo.sp_vehicle_create
    @id_vehicle_type INT,
    @plate VARCHAR(20),
    @maximum_weight DECIMAL(10,2) = NULL,
    @maximum_volume DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @plate = UPPER(LTRIM(RTRIM(@plate)));

    IF NOT EXISTS (
        SELECT 1 FROM VehicleTypes
        WHERE id_vehicle_type = @id_vehicle_type
          AND deleted_at IS NULL
          AND status = 1
    )
    BEGIN
        SELECT 0 AS Success, 'Seleccione un tipo de vehiculo valido.' AS Message, NULL AS IdVehicle;
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM Vehicles
        WHERE UPPER(plate) = @plate
          AND deleted_at IS NULL
    )
    BEGIN
        SELECT 0 AS Success, 'Ya existe un vehiculo con esa placa.' AS Message, NULL AS IdVehicle;
        RETURN;
    END

    INSERT INTO Vehicles (id_vehicle_type, plate, maximum_weight, maximum_volume)
    VALUES (@id_vehicle_type, @plate, @maximum_weight, @maximum_volume);

    SELECT
        1 AS Success,
        'Vehiculo creado correctamente.' AS Message,
        CAST(SCOPE_IDENTITY() AS INT) AS IdVehicle;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_update;
GO
CREATE PROCEDURE dbo.sp_vehicle_update
    @id_vehicle INT,
    @id_vehicle_type INT,
    @plate VARCHAR(20),
    @maximum_weight DECIMAL(10,2) = NULL,
    @maximum_volume DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @plate = UPPER(LTRIM(RTRIM(@plate)));

    IF NOT EXISTS (
        SELECT 1 FROM Vehicles
        WHERE id_vehicle = @id_vehicle
          AND deleted_at IS NULL
    )
    BEGIN
        SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1 FROM VehicleTypes
        WHERE id_vehicle_type = @id_vehicle_type
          AND deleted_at IS NULL
          AND status = 1
    )
    BEGIN
        SELECT 0 AS Success, 'Seleccione un tipo de vehiculo valido.' AS Message;
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM Vehicles
        WHERE UPPER(plate) = @plate
          AND id_vehicle <> @id_vehicle
          AND deleted_at IS NULL
    )
    BEGIN
        SELECT 0 AS Success, 'Ya existe otro vehiculo con esa placa.' AS Message;
        RETURN;
    END

    UPDATE Vehicles
    SET id_vehicle_type = @id_vehicle_type,
        plate = @plate,
        maximum_weight = @maximum_weight,
        maximum_volume = @maximum_volume,
        updated_at = GETDATE()
    WHERE id_vehicle = @id_vehicle
      AND deleted_at IS NULL;

    SELECT 1 AS Success, 'Vehiculo actualizado correctamente.' AS Message;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_delete_logic;
GO
CREATE PROCEDURE dbo.sp_vehicle_delete_logic
    @id_vehicle INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM Vehicles
        WHERE id_vehicle = @id_vehicle
          AND deleted_at IS NULL
    )
    BEGIN
        SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
        RETURN;
    END

    UPDATE Vehicles
    SET status = 0,
        updated_at = GETDATE()
    WHERE id_vehicle = @id_vehicle
      AND deleted_at IS NULL;

    SELECT 1 AS Success, 'Vehiculo desactivado correctamente.' AS Message;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_restore;
GO
CREATE PROCEDURE dbo.sp_vehicle_restore
    @id_vehicle INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM Vehicles
        WHERE id_vehicle = @id_vehicle
          AND deleted_at IS NULL
    )
    BEGIN
        SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
        RETURN;
    END

    UPDATE Vehicles
    SET status = 1,
        updated_at = GETDATE()
    WHERE id_vehicle = @id_vehicle
      AND deleted_at IS NULL;

    SELECT 1 AS Success, 'Vehiculo restaurado correctamente.' AS Message;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_delete_physical;
GO
CREATE PROCEDURE dbo.sp_vehicle_delete_physical
    @id_vehicle INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM Shipments
        WHERE id_vehicle = @id_vehicle
    )
    BEGIN
        SELECT 0 AS Success, 'No se puede eliminar: existen envios asociados.' AS Message;
        RETURN;
    END

    DELETE FROM Vehicles
    WHERE id_vehicle = @id_vehicle;

    IF @@ROWCOUNT = 0
        SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE
        SELECT 1 AS Success, 'Vehiculo eliminado permanentemente.' AS Message;
END
GO
USE KMLLogistics;
GO

/* =========================
   1. CAJAS
========================= */

CREATE OR ALTER PROCEDURE dbo.sp_box_list_active
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_box AS IdBox,
        code AS Code,
        weight AS Weight,
        height AS Height,
        width AS Width,
        length AS Length,
        volume AS Volume,
        status AS Status,
        COUNT(*) OVER() AS TotalCount
    FROM Boxes
    WHERE deleted_at IS NULL
      AND status = 1
      AND (@search IS NULL OR @search = '' OR code LIKE '%' + @search + '%')
    ORDER BY id_box DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_list_inactive
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_box AS IdBox,
        code AS Code,
        weight AS Weight,
        height AS Height,
        width AS Width,
        length AS Length,
        volume AS Volume,
        status AS Status,
        COUNT(*) OVER() AS TotalCount
    FROM Boxes
    WHERE deleted_at IS NULL
      AND status = 0
      AND (@search IS NULL OR @search = '' OR code LIKE '%' + @search + '%')
    ORDER BY id_box DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_get_by_id
    @id_box INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_box AS IdBox,
        code AS Code,
        weight AS Weight,
        height AS Height,
        width AS Width,
        length AS Length,
        volume AS Volume,
        status AS Status,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt
    FROM Boxes
    WHERE id_box = @id_box
      AND deleted_at IS NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_create
    @code VARCHAR(50),
    @weight DECIMAL(10,2) = NULL,
    @height DECIMAL(10,2) = NULL,
    @width DECIMAL(10,2) = NULL,
    @length DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @code = UPPER(LTRIM(RTRIM(@code)));

    IF EXISTS (SELECT 1 FROM Boxes WHERE code = @code AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS Success, 'Ya existe una caja con ese codigo.' AS Message, NULL AS IdBox;
        RETURN;
    END

    INSERT INTO Boxes (code, weight, height, width, length)
    VALUES (@code, @weight, @height, @width, @length);

    SELECT 1 AS Success, 'Caja creada correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdBox;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_update
    @id_box INT,
    @code VARCHAR(50),
    @weight DECIMAL(10,2) = NULL,
    @height DECIMAL(10,2) = NULL,
    @width DECIMAL(10,2) = NULL,
    @length DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @code = UPPER(LTRIM(RTRIM(@code)));

    IF NOT EXISTS (SELECT 1 FROM Boxes WHERE id_box = @id_box AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Registro no encontrado.' AS Message; RETURN; END

    IF EXISTS (SELECT 1 FROM Boxes WHERE code = @code AND id_box <> @id_box AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Ya existe otra caja con ese codigo.' AS Message; RETURN; END

    UPDATE Boxes
    SET code = @code,
        weight = @weight,
        height = @height,
        width = @width,
        length = @length,
        updated_at = GETDATE()
    WHERE id_box = @id_box
      AND deleted_at IS NULL;

    SELECT 1 AS Success, 'Caja actualizada correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_delete_logic
    @id_box INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM ShipmentBoxes WHERE id_box = @id_box)
    BEGIN SELECT 0 AS Success, 'No se puede desactivar: existen envios asociados.' AS Message; RETURN; END

    UPDATE Boxes
    SET status = 0,
        updated_at = GETDATE()
    WHERE id_box = @id_box
      AND deleted_at IS NULL;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Caja desactivada correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_restore
    @id_box INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Boxes
    SET status = 1,
        updated_at = GETDATE()
    WHERE id_box = @id_box
      AND deleted_at IS NULL;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Caja restaurada correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_delete_physical
    @id_box INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM ShipmentBoxes WHERE id_box = @id_box)
    BEGIN SELECT 0 AS Success, 'No se puede eliminar: existen envios asociados.' AS Message; RETURN; END

    IF EXISTS (SELECT 1 FROM BoxDetails WHERE id_box = @id_box)
    BEGIN SELECT 0 AS Success, 'No se puede eliminar: existen detalles asociados.' AS Message; RETURN; END

    DELETE FROM Boxes WHERE id_box = @id_box;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Caja eliminada permanentemente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT id_box AS IdBox, code AS Code
    FROM Boxes
    WHERE deleted_at IS NULL AND status = 1
    ORDER BY code;
END
GO


/* =========================
   2. DETALLE DE CAJA
========================= */

CREATE OR ALTER PROCEDURE dbo.sp_box_detail_list
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10,
    @id_box INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        bd.id_box_detail AS IdBoxDetail,
        bd.id_box AS IdBox,
        b.code AS BoxCode,
        bd.id_sale_detail AS IdSaleDetail,
        sd.id_sale AS IdSale,
        p.name AS ProductName,
        bd.quantity AS Quantity,
        COUNT(*) OVER() AS TotalCount
    FROM BoxDetails bd
    INNER JOIN Boxes b ON b.id_box = bd.id_box
    INNER JOIN SaleDetails sd ON sd.id_sale_detail = bd.id_sale_detail
    INNER JOIN Products p ON p.id_product = sd.id_product
    WHERE (@id_box IS NULL OR bd.id_box = @id_box)
      AND (
            @search IS NULL OR @search = ''
         OR b.code LIKE '%' + @search + '%'
         OR p.name LIKE '%' + @search + '%'
         OR CAST(sd.id_sale AS VARCHAR(20)) LIKE '%' + @search + '%'
      )
    ORDER BY bd.id_box_detail DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_detail_get_by_id
    @id_box_detail INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        bd.id_box_detail AS IdBoxDetail,
        bd.id_box AS IdBox,
        b.code AS BoxCode,
        bd.id_sale_detail AS IdSaleDetail,
        sd.id_sale AS IdSale,
        p.name AS ProductName,
        bd.quantity AS Quantity
    FROM BoxDetails bd
    INNER JOIN Boxes b ON b.id_box = bd.id_box
    INNER JOIN SaleDetails sd ON sd.id_sale_detail = bd.id_sale_detail
    INNER JOIN Products p ON p.id_product = sd.id_product
    WHERE bd.id_box_detail = @id_box_detail;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_detail_create
    @id_box INT,
    @id_sale_detail INT,
    @quantity INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @quantity <= 0
    BEGIN SELECT 0 AS Success, 'La cantidad debe ser mayor que cero.' AS Message, NULL AS IdBoxDetail; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Boxes WHERE id_box = @id_box AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS Success, 'Seleccione una caja activa.' AS Message, NULL AS IdBoxDetail; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM SaleDetails WHERE id_sale_detail = @id_sale_detail)
    BEGIN SELECT 0 AS Success, 'Seleccione un detalle de venta valido.' AS Message, NULL AS IdBoxDetail; RETURN; END

    INSERT INTO BoxDetails (id_box, id_sale_detail, quantity)
    VALUES (@id_box, @id_sale_detail, @quantity);

    SELECT 1 AS Success, 'Detalle de caja creado correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdBoxDetail;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_detail_update
    @id_box_detail INT,
    @id_box INT,
    @id_sale_detail INT,
    @quantity INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @quantity <= 0
    BEGIN SELECT 0 AS Success, 'La cantidad debe ser mayor que cero.' AS Message; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM BoxDetails WHERE id_box_detail = @id_box_detail)
    BEGIN SELECT 0 AS Success, 'Registro no encontrado.' AS Message; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Boxes WHERE id_box = @id_box AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS Success, 'Seleccione una caja activa.' AS Message; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM SaleDetails WHERE id_sale_detail = @id_sale_detail)
    BEGIN SELECT 0 AS Success, 'Seleccione un detalle de venta valido.' AS Message; RETURN; END

    UPDATE BoxDetails
    SET id_box = @id_box,
        id_sale_detail = @id_sale_detail,
        quantity = @quantity
    WHERE id_box_detail = @id_box_detail;

    SELECT 1 AS Success, 'Detalle de caja actualizado correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_box_detail_delete
    @id_box_detail INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM BoxDetails WHERE id_box_detail = @id_box_detail;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Detalle de caja eliminado correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_detail_options_for_box
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sd.id_sale_detail AS IdSaleDetail,
        CONCAT('Venta #', sd.id_sale, ' - ', p.name) AS Name,
        sd.quantity AS Quantity
    FROM SaleDetails sd
    INNER JOIN Products p ON p.id_product = sd.id_product
    ORDER BY sd.id_sale_detail DESC;
END
GO


/* =========================
   3. ESTADOS DE ENVIO
========================= */

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_list_active
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_shipment_status AS IdShipmentStatus,
        name AS Name,
        description AS Description,
        status AS Status,
        COUNT(*) OVER() AS TotalCount
    FROM ShipmentStatuses
    WHERE deleted_at IS NULL
      AND status = 1
      AND (@search IS NULL OR @search = '' OR name LIKE '%' + @search + '%' OR description LIKE '%' + @search + '%')
    ORDER BY id_shipment_status DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_list_inactive
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_shipment_status AS IdShipmentStatus,
        name AS Name,
        description AS Description,
        status AS Status,
        COUNT(*) OVER() AS TotalCount
    FROM ShipmentStatuses
    WHERE deleted_at IS NULL
      AND status = 0
      AND (@search IS NULL OR @search = '' OR name LIKE '%' + @search + '%' OR description LIKE '%' + @search + '%')
    ORDER BY id_shipment_status DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_get_by_id
    @id_shipment_status INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_shipment_status AS IdShipmentStatus,
        name AS Name,
        description AS Description,
        status AS Status,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt
    FROM ShipmentStatuses
    WHERE id_shipment_status = @id_shipment_status
      AND deleted_at IS NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_create
    @name VARCHAR(50),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @name = LTRIM(RTRIM(@name));

    IF EXISTS (SELECT 1 FROM ShipmentStatuses WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Ya existe un estado de envio con ese nombre.' AS Message, NULL AS IdShipmentStatus; RETURN; END

    INSERT INTO ShipmentStatuses (name, description)
    VALUES (@name, @description);

    SELECT 1 AS Success, 'Estado de envio creado correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdShipmentStatus;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_update
    @id_shipment_status INT,
    @name VARCHAR(50),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @name = LTRIM(RTRIM(@name));

    IF NOT EXISTS (SELECT 1 FROM ShipmentStatuses WHERE id_shipment_status = @id_shipment_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Registro no encontrado.' AS Message; RETURN; END

    IF EXISTS (SELECT 1 FROM ShipmentStatuses WHERE name = @name AND id_shipment_status <> @id_shipment_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Ya existe otro estado de envio con ese nombre.' AS Message; RETURN; END

    UPDATE ShipmentStatuses
    SET name = @name,
        description = @description,
        updated_at = GETDATE()
    WHERE id_shipment_status = @id_shipment_status
      AND deleted_at IS NULL;

    SELECT 1 AS Success, 'Estado de envio actualizado correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_delete_logic
    @id_shipment_status INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Shipments WHERE id_shipment_status = @id_shipment_status AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'No se puede desactivar: existen envios asociados.' AS Message; RETURN; END

    UPDATE ShipmentStatuses
    SET status = 0,
        updated_at = GETDATE()
    WHERE id_shipment_status = @id_shipment_status
      AND deleted_at IS NULL;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Estado de envio desactivado correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_restore
    @id_shipment_status INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ShipmentStatuses
    SET status = 1,
        updated_at = GETDATE()
    WHERE id_shipment_status = @id_shipment_status
      AND deleted_at IS NULL;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Estado de envio restaurado correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_delete_physical
    @id_shipment_status INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Shipments WHERE id_shipment_status = @id_shipment_status)
    BEGIN SELECT 0 AS Success, 'No se puede eliminar: existen envios asociados.' AS Message; RETURN; END

    DELETE FROM ShipmentStatuses WHERE id_shipment_status = @id_shipment_status;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Estado de envio eliminado permanentemente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_status_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT id_shipment_status AS IdShipmentStatus, name AS Name
    FROM ShipmentStatuses
    WHERE deleted_at IS NULL AND status = 1
    ORDER BY name;
END
GO


/* =========================
   4. ENVIOS
========================= */

CREATE OR ALTER PROCEDURE dbo.sp_shipment_list
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10,
    @id_shipment_status INT = NULL,
    @id_vehicle INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.id_shipment AS IdShipment,
        s.id_vehicle AS IdVehicle,
        v.plate AS VehiclePlate,
        vt.name AS VehicleTypeName,
        s.id_employee AS IdEmployee,
        CONCAT(e.name, ' ', e.last_name_paternal) AS EmployeeName,
        s.id_shipment_status AS IdShipmentStatus,
        ss.name AS ShipmentStatusName,
        s.departure_date AS DepartureDate,
        s.arrival_date AS ArrivalDate,
        COUNT(*) OVER() AS TotalCount
    FROM Shipments s
    INNER JOIN Vehicles v ON v.id_vehicle = s.id_vehicle
    INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    INNER JOIN Employees e ON e.id_employee = s.id_employee
    INNER JOIN ShipmentStatuses ss ON ss.id_shipment_status = s.id_shipment_status
    WHERE s.deleted_at IS NULL
      AND (@id_shipment_status IS NULL OR s.id_shipment_status = @id_shipment_status)
      AND (@id_vehicle IS NULL OR s.id_vehicle = @id_vehicle)
      AND (
            @search IS NULL OR @search = ''
         OR v.plate LIKE '%' + @search + '%'
         OR vt.name LIKE '%' + @search + '%'
         OR ss.name LIKE '%' + @search + '%'
         OR e.name LIKE '%' + @search + '%'
         OR e.last_name_paternal LIKE '%' + @search + '%'
      )
    ORDER BY s.id_shipment DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_get_by_id
    @id_shipment INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.id_shipment AS IdShipment,
        s.id_vehicle AS IdVehicle,
        v.plate AS VehiclePlate,
        vt.name AS VehicleTypeName,
        s.id_employee AS IdEmployee,
        CONCAT(e.name, ' ', e.last_name_paternal) AS EmployeeName,
        s.id_shipment_status AS IdShipmentStatus,
        ss.name AS ShipmentStatusName,
        s.departure_date AS DepartureDate,
        s.arrival_date AS ArrivalDate,
        s.created_at AS CreatedAt,
        s.updated_at AS UpdatedAt
    FROM Shipments s
    INNER JOIN Vehicles v ON v.id_vehicle = s.id_vehicle
    INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    INNER JOIN Employees e ON e.id_employee = s.id_employee
    INNER JOIN ShipmentStatuses ss ON ss.id_shipment_status = s.id_shipment_status
    WHERE s.id_shipment = @id_shipment
      AND s.deleted_at IS NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_create
    @id_vehicle INT,
    @id_employee INT,
    @id_shipment_status INT,
    @departure_date DATETIME = NULL,
    @arrival_date DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Vehicles WHERE id_vehicle = @id_vehicle AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS Success, 'Seleccione un vehiculo activo.' AS Message, NULL AS IdShipment; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Employees WHERE id_employee = @id_employee AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS Success, 'Seleccione un empleado activo.' AS Message, NULL AS IdShipment; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM ShipmentStatuses WHERE id_shipment_status = @id_shipment_status AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS Success, 'Seleccione un estado de envio activo.' AS Message, NULL AS IdShipment; RETURN; END

    IF @arrival_date IS NOT NULL AND @departure_date IS NOT NULL AND @arrival_date < @departure_date
    BEGIN SELECT 0 AS Success, 'La fecha de llegada no puede ser menor que la fecha de salida.' AS Message, NULL AS IdShipment; RETURN; END

    INSERT INTO Shipments (id_vehicle, id_employee, id_shipment_status, departure_date, arrival_date)
    VALUES (@id_vehicle, @id_employee, @id_shipment_status, @departure_date, @arrival_date);

    SELECT 1 AS Success, 'Envio creado correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdShipment;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_update
    @id_shipment INT,
    @id_vehicle INT,
    @id_employee INT,
    @id_shipment_status INT,
    @departure_date DATETIME = NULL,
    @arrival_date DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Shipments WHERE id_shipment = @id_shipment AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Registro no encontrado.' AS Message; RETURN; END

    IF @arrival_date IS NOT NULL AND @departure_date IS NOT NULL AND @arrival_date < @departure_date
    BEGIN SELECT 0 AS Success, 'La fecha de llegada no puede ser menor que la fecha de salida.' AS Message; RETURN; END

    UPDATE Shipments
    SET id_vehicle = @id_vehicle,
        id_employee = @id_employee,
        id_shipment_status = @id_shipment_status,
        departure_date = @departure_date,
        arrival_date = @arrival_date,
        updated_at = GETDATE()
    WHERE id_shipment = @id_shipment
      AND deleted_at IS NULL;

    SELECT 1 AS Success, 'Envio actualizado correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_delete_logic
    @id_shipment INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Shipments
    SET deleted_at = GETDATE(),
        updated_at = GETDATE()
    WHERE id_shipment = @id_shipment
      AND deleted_at IS NULL;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Envio eliminado correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.id_shipment AS IdShipment,
        CONCAT('Envio #', s.id_shipment, ' - ', v.plate) AS Name
    FROM Shipments s
    INNER JOIN Vehicles v ON v.id_vehicle = s.id_vehicle
    WHERE s.deleted_at IS NULL
    ORDER BY s.id_shipment DESC;
END
GO


/* =========================
   5. CAJAS DE ENVIO
========================= */

CREATE OR ALTER PROCEDURE dbo.sp_shipment_box_list
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10,
    @id_shipment INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sb.id_shipment_box AS IdShipmentBox,
        sb.id_shipment AS IdShipment,
        sb.id_box AS IdBox,
        b.code AS BoxCode,
        b.weight AS Weight,
        b.volume AS Volume,
        v.plate AS VehiclePlate,
        COUNT(*) OVER() AS TotalCount
    FROM ShipmentBoxes sb
    INNER JOIN Shipments s ON s.id_shipment = sb.id_shipment
    INNER JOIN Vehicles v ON v.id_vehicle = s.id_vehicle
    INNER JOIN Boxes b ON b.id_box = sb.id_box
    WHERE s.deleted_at IS NULL
      AND (@id_shipment IS NULL OR sb.id_shipment = @id_shipment)
      AND (@search IS NULL OR @search = '' OR b.code LIKE '%' + @search + '%' OR v.plate LIKE '%' + @search + '%')
    ORDER BY sb.id_shipment_box DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_box_create
    @id_shipment INT,
    @id_box INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Shipments WHERE id_shipment = @id_shipment AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Seleccione un envio valido.' AS Message, NULL AS IdShipmentBox; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Boxes WHERE id_box = @id_box AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS Success, 'Seleccione una caja activa.' AS Message, NULL AS IdShipmentBox; RETURN; END

    IF EXISTS (SELECT 1 FROM ShipmentBoxes WHERE id_shipment = @id_shipment AND id_box = @id_box)
    BEGIN SELECT 0 AS Success, 'La caja ya esta asociada a este envio.' AS Message, NULL AS IdShipmentBox; RETURN; END

    INSERT INTO ShipmentBoxes (id_shipment, id_box)
    VALUES (@id_shipment, @id_box);

    SELECT 1 AS Success, 'Caja asociada al envio correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdShipmentBox;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_box_delete
    @id_shipment_box INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM ShipmentBoxes WHERE id_shipment_box = @id_shipment_box;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Caja retirada del envio correctamente.' AS Message;
END
GO


/* =========================
   6. VENTAS DE ENVIO
========================= */

CREATE OR ALTER PROCEDURE dbo.sp_shipment_sale_list
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10,
    @id_shipment INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ssale.id_shipment_sale AS IdShipmentSale,
        ssale.id_shipment AS IdShipment,
        ssale.id_sale AS IdSale,
        c.name AS ClientName,
        c.last_name_paternal AS ClientLastNamePaternal,
        s.total AS SaleTotal,
        v.plate AS VehiclePlate,
        ssale.created_at AS CreatedAt,
        COUNT(*) OVER() AS TotalCount
    FROM ShipmentSales ssale
    INNER JOIN Shipments sh ON sh.id_shipment = ssale.id_shipment
    INNER JOIN Vehicles v ON v.id_vehicle = sh.id_vehicle
    INNER JOIN Sales s ON s.id_sale = ssale.id_sale
    INNER JOIN Clients c ON c.id_client = s.id_client
    WHERE sh.deleted_at IS NULL
      AND ssale.deleted_at IS NULL
      AND (@id_shipment IS NULL OR ssale.id_shipment = @id_shipment)
      AND (
            @search IS NULL OR @search = ''
         OR CAST(ssale.id_sale AS VARCHAR(20)) LIKE '%' + @search + '%'
         OR c.name LIKE '%' + @search + '%'
         OR c.last_name_paternal LIKE '%' + @search + '%'
         OR v.plate LIKE '%' + @search + '%'
      )
    ORDER BY ssale.id_shipment_sale DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_sale_create
    @id_shipment INT,
    @id_sale INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Shipments WHERE id_shipment = @id_shipment AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Seleccione un envio valido.' AS Message, NULL AS IdShipmentSale; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Sales WHERE id_sale = @id_sale AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'Seleccione una venta valida.' AS Message, NULL AS IdShipmentSale; RETURN; END

    IF EXISTS (SELECT 1 FROM ShipmentSales WHERE id_shipment = @id_shipment AND id_sale = @id_sale AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, 'La venta ya esta asociada a este envio.' AS Message, NULL AS IdShipmentSale; RETURN; END

    INSERT INTO ShipmentSales (id_shipment, id_sale)
    VALUES (@id_shipment, @id_sale);

    SELECT 1 AS Success, 'Venta asociada al envio correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdShipmentSale;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_sale_delete_logic
    @id_shipment_sale INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ShipmentSales
    SET deleted_at = GETDATE(),
        updated_at = GETDATE()
    WHERE id_shipment_sale = @id_shipment_sale
      AND deleted_at IS NULL;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Venta retirada del envio correctamente.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_shipment_sale_delete_physical
    @id_shipment_sale INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM ShipmentSales WHERE id_shipment_sale = @id_shipment_sale;

    IF @@ROWCOUNT = 0 SELECT 0 AS Success, 'Registro no encontrado.' AS Message;
    ELSE SELECT 1 AS Success, 'Venta retirada permanentemente del envio.' AS Message;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_sale_options_for_shipment
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.id_sale AS IdSale,
        CONCAT('Venta #', s.id_sale, ' - ', c.name, ' ', c.last_name_paternal) AS Name,
        s.total AS Total
    FROM Sales s
    INNER JOIN Clients c ON c.id_client = s.id_client
    WHERE s.deleted_at IS NULL
    ORDER BY s.id_sale DESC;
END
GO