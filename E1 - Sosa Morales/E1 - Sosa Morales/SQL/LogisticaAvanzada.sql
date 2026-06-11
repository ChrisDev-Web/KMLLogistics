USE KMLLogistics;
GO

/* ============================================================
   LOGISTICA AVANZADA
   - Dimensiones en vehiculos (volumen calculado)
   - Detalle de envio (direccion + distancia simulada)
   - Disponibilidad de vehiculos por horario
   - Transiciones automaticas de estado + alertas
   - Seguimiento GPS simulado
============================================================ */

-- Vehiculos: dimensiones
IF COL_LENGTH('dbo.Vehicles', 'height') IS NULL
    ALTER TABLE dbo.Vehicles ADD height DECIMAL(10,2) NULL;
IF COL_LENGTH('dbo.Vehicles', 'width') IS NULL
    ALTER TABLE dbo.Vehicles ADD width DECIMAL(10,2) NULL;
IF COL_LENGTH('dbo.Vehicles', 'length') IS NULL
    ALTER TABLE dbo.Vehicles ADD length DECIMAL(10,2) NULL;
GO

-- Shipments: hora en que el vehiculo vuelve a estar disponible
IF COL_LENGTH('dbo.Shipments', 'return_available_at') IS NULL
    ALTER TABLE dbo.Shipments ADD return_available_at DATETIME NULL;
GO

-- Detalle de envio (una direccion por envio)
IF OBJECT_ID('dbo.ShipmentDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShipmentDetails (
        id_shipment_detail   INT IDENTITY(1,1) PRIMARY KEY,
        id_shipment          INT NOT NULL UNIQUE,
        delivery_address     VARCHAR(255) NOT NULL,
        id_district          INT NULL,
        client_name          VARCHAR(200) NULL,
        simulated_distance_km DECIMAL(10,2) NOT NULL DEFAULT (0),
        travel_minutes       INT NOT NULL DEFAULT (0),
        origin_latitude      DECIMAL(9,6) NOT NULL DEFAULT (-12.046374),
        origin_longitude     DECIMAL(9,6) NOT NULL DEFAULT (-77.042793),
        dest_latitude        DECIMAL(9,6) NOT NULL,
        dest_longitude       DECIMAL(9,6) NOT NULL,
        created_at           DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at           DATETIME NULL,
        CONSTRAINT fk_shipment_detail_shipment
            FOREIGN KEY (id_shipment) REFERENCES dbo.Shipments(id_shipment),
        CONSTRAINT fk_shipment_detail_district
            FOREIGN KEY (id_district) REFERENCES dbo.Districts(id_district)
    );
END
GO

-- Alertas de logistica
IF OBJECT_ID('dbo.LogisticsAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LogisticsAlerts (
        id_logistics_alert INT IDENTITY(1,1) PRIMARY KEY,
        id_shipment        INT NOT NULL,
        id_vehicle         INT NOT NULL,
        alert_type         VARCHAR(30) NOT NULL,
        message            VARCHAR(500) NOT NULL,
        status             VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        created_at         DATETIME NOT NULL DEFAULT GETDATE(),
        resolved_at        DATETIME NULL,
        CONSTRAINT fk_logistics_alert_shipment FOREIGN KEY (id_shipment) REFERENCES dbo.Shipments(id_shipment),
        CONSTRAINT fk_logistics_alert_vehicle FOREIGN KEY (id_vehicle) REFERENCES dbo.Vehicles(id_vehicle),
        CONSTRAINT chk_logistics_alert_status CHECK (status IN ('ACTIVE', 'RESOLVED')),
        CONSTRAINT chk_logistics_alert_type CHECK (alert_type IN ('RETURNING', 'AVAILABLE'))
    );
    CREATE INDEX ix_logistics_alerts_status ON dbo.LogisticsAlerts(status, created_at DESC);
END
GO

/* Helper: distancia y coordenadas simuladas desde direccion */
IF OBJECT_ID('dbo.fn_logistics_simulate_route', 'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_logistics_simulate_route;
GO
CREATE FUNCTION dbo.fn_logistics_simulate_route
(
    @address    VARCHAR(255),
    @id_district INT = NULL
)
RETURNS TABLE
AS
RETURN
(
    WITH Seed AS (
        SELECT ABS(CHECKSUM(ISNULL(@address, '') + '|' + CAST(ISNULL(@id_district, 0) AS VARCHAR(20)))) AS h
    )
    SELECT
        CAST(10.0 + (s.h % 46) AS DECIMAL(10,2)) AS simulated_distance_km,
        CAST(25 + (s.h % 90) AS INT) AS travel_minutes,
        CAST(-12.046374 + ((10.0 + (s.h % 46)) / 111.0) * COS((s.h % 360) * PI() / 180.0) AS DECIMAL(9,6)) AS dest_latitude,
        CAST(-77.042793 + ((10.0 + (s.h % 46)) / 111.0) * SIN((s.h % 360) * PI() / 180.0) / COS(-12.046374 * PI() / 180.0) AS DECIMAL(9,6)) AS dest_longitude
    FROM Seed s
);
GO

/* Sincroniza detalle de envio y recalcula llegada desde la primera venta asociada */
IF OBJECT_ID('dbo.sp_shipment_detail_sync', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_shipment_detail_sync;
GO
CREATE PROCEDURE dbo.sp_shipment_detail_sync
    @id_shipment INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ShipmentDetails WHERE id_shipment = @id_shipment)
        RETURN;

    DECLARE @address VARCHAR(255);
    DECLARE @id_district INT;
    DECLARE @client_name VARCHAR(200);

    SELECT TOP 1
        @address = ISNULL(NULLIF(LTRIM(RTRIM(c.address)), ''), 'Direccion no registrada'),
        @id_district = c.id_district,
        @client_name = CONCAT(c.name, ' ', c.last_name_paternal)
    FROM dbo.ShipmentSales ss
    INNER JOIN dbo.Sales s ON s.id_sale = ss.id_sale
    INNER JOIN dbo.Clients c ON c.id_client = s.id_client
    WHERE ss.id_shipment = @id_shipment
      AND ss.deleted_at IS NULL
      AND s.deleted_at IS NULL
    ORDER BY ss.id_shipment_sale;

    IF @address IS NULL RETURN;

    DECLARE @distance DECIMAL(10,2);
    DECLARE @travel INT;
    DECLARE @dest_lat DECIMAL(9,6);
    DECLARE @dest_lng DECIMAL(9,6);

    SELECT
        @distance = simulated_distance_km,
        @travel = travel_minutes,
        @dest_lat = dest_latitude,
        @dest_lng = dest_longitude
    FROM dbo.fn_logistics_simulate_route(@address, @id_district);

    INSERT INTO dbo.ShipmentDetails (
        id_shipment, delivery_address, id_district, client_name,
        simulated_distance_km, travel_minutes, dest_latitude, dest_longitude
    )
    VALUES (
        @id_shipment, @address, @id_district, @client_name,
        @distance, @travel, @dest_lat, @dest_lng
    );

    UPDATE sh
    SET arrival_date = DATEADD(MINUTE, @travel, sh.departure_date),
        updated_at = GETDATE()
    FROM dbo.Shipments sh
    WHERE sh.id_shipment = @id_shipment
      AND sh.deleted_at IS NULL
      AND sh.departure_date IS NOT NULL;
END
GO

/* Transiciones automaticas + alertas */
IF OBJECT_ID('dbo.sp_logistics_sync_shipments', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_logistics_sync_shipments;
GO
CREATE PROCEDURE dbo.sp_logistics_sync_shipments
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status_pendiente INT = (SELECT TOP 1 id_shipment_status FROM dbo.ShipmentStatuses WHERE name = N'Pendiente' AND deleted_at IS NULL);
    DECLARE @status_empaquetado INT = (SELECT TOP 1 id_shipment_status FROM dbo.ShipmentStatuses WHERE name = N'Empaquetado' AND deleted_at IS NULL);
    DECLARE @status_transito INT = (SELECT TOP 1 id_shipment_status FROM dbo.ShipmentStatuses WHERE name = N'En Transito' AND deleted_at IS NULL);
    DECLARE @status_entregado INT = (SELECT TOP 1 id_shipment_status FROM dbo.ShipmentStatuses WHERE name = N'Entregado' AND deleted_at IS NULL);
    DECLARE @now DATETIME = GETDATE();

    UPDATE s
    SET id_shipment_status = @status_transito,
        updated_at = @now
    FROM dbo.Shipments s
    WHERE s.deleted_at IS NULL
      AND s.departure_date IS NOT NULL
      AND s.departure_date <= @now
      AND s.id_shipment_status IN (@status_pendiente, @status_empaquetado)
      AND (@status_transito IS NOT NULL);

    DECLARE @Arrived TABLE (id_shipment INT, id_vehicle INT, plate VARCHAR(20));

    UPDATE s
    SET id_shipment_status = @status_entregado,
        return_available_at = DATEADD(HOUR, 2, s.arrival_date),
        updated_at = @now
    OUTPUT INSERTED.id_shipment, INSERTED.id_vehicle INTO @Arrived(id_shipment, id_vehicle)
    FROM dbo.Shipments s
    WHERE s.deleted_at IS NULL
      AND s.arrival_date IS NOT NULL
      AND s.arrival_date <= @now
      AND s.id_shipment_status = @status_transito
      AND s.return_available_at IS NULL
      AND (@status_entregado IS NOT NULL);

    INSERT INTO dbo.LogisticsAlerts (id_shipment, id_vehicle, alert_type, message)
    SELECT
        a.id_shipment,
        a.id_vehicle,
        'RETURNING',
        CONCAT(N'El vehiculo ', v.plate, N' ya llego a su destino. Estara de regreso en aproximadamente 2 horas.')
    FROM @Arrived a
    INNER JOIN dbo.Vehicles v ON v.id_vehicle = a.id_vehicle
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.LogisticsAlerts la
        WHERE la.id_shipment = a.id_shipment AND la.alert_type = 'RETURNING' AND la.status = 'ACTIVE'
    );

    DECLARE @Available TABLE (id_shipment INT, id_vehicle INT, plate VARCHAR(20));

    INSERT INTO @Available (id_shipment, id_vehicle, plate)
    SELECT s.id_shipment, s.id_vehicle, v.plate
    FROM dbo.Shipments s
    INNER JOIN dbo.Vehicles v ON v.id_vehicle = s.id_vehicle
    WHERE s.deleted_at IS NULL
      AND s.return_available_at IS NOT NULL
      AND s.return_available_at <= @now
      AND NOT EXISTS (
          SELECT 1 FROM dbo.LogisticsAlerts la
          WHERE la.id_shipment = s.id_shipment AND la.alert_type = 'AVAILABLE'
      );

    UPDATE la
    SET status = 'RESOLVED', resolved_at = @now
    FROM dbo.LogisticsAlerts la
    INNER JOIN dbo.Shipments s ON s.id_shipment = la.id_shipment
    WHERE la.alert_type = 'RETURNING'
      AND la.status = 'ACTIVE'
      AND s.return_available_at IS NOT NULL
      AND s.return_available_at <= @now;

    INSERT INTO dbo.LogisticsAlerts (id_shipment, id_vehicle, alert_type, message)
    SELECT
        a.id_shipment,
        a.id_vehicle,
        'AVAILABLE',
        CONCAT(N'El vehiculo ', a.plate, N' ya esta disponible para un nuevo envio.')
    FROM @Available a;
END
GO

IF OBJECT_ID('dbo.sp_logistics_alert_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_logistics_alert_list_active;
GO
CREATE PROCEDURE dbo.sp_logistics_alert_list_active
    @status VARCHAR(20) = 'ACTIVE'
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_logistics_sync_shipments;

    SET @status = UPPER(NULLIF(LTRIM(RTRIM(@status)), ''));
    IF @status IS NULL OR @status NOT IN ('ACTIVE', 'RESOLVED', 'ALL')
        SET @status = 'ACTIVE';

    SELECT
        la.id_logistics_alert AS IdLogisticsAlert,
        la.id_shipment AS IdShipment,
        la.id_vehicle AS IdVehicle,
        v.plate AS VehiclePlate,
        la.alert_type AS AlertType,
        la.message AS Message,
        la.status AS Status,
        la.created_at AS CreatedAt
    FROM dbo.LogisticsAlerts la
    INNER JOIN dbo.Vehicles v ON v.id_vehicle = la.id_vehicle
    WHERE (
            (@status = 'ALL' AND la.status IN ('ACTIVE', 'RESOLVED'))
         OR (@status = 'ACTIVE' AND la.status = 'ACTIVE')
         OR (@status = 'RESOLVED' AND la.status = 'RESOLVED')
    )
    ORDER BY la.created_at DESC;
END
GO

IF OBJECT_ID('dbo.sp_logistics_alert_resend', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_logistics_alert_resend;
GO
CREATE PROCEDURE dbo.sp_logistics_alert_resend
    @id_logistics_alert INT,
    @id_user            INT,
    @message            VARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.LogisticsAlerts
        WHERE id_logistics_alert = @id_logistics_alert AND status = 'ACTIVE'
    )
    BEGIN
        SET @message = 'La alerta no existe o ya fue resuelta.';
        RETURN;
    END

    UPDATE dbo.LogisticsAlerts
    SET created_at = GETDATE()
    WHERE id_logistics_alert = @id_logistics_alert
      AND status = 'ACTIVE';

    SET @message = 'Alerta reenviada correctamente.';
END
GO

IF OBJECT_ID('dbo.sp_logistics_alert_count_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_logistics_alert_count_active;
GO
CREATE PROCEDURE dbo.sp_logistics_alert_count_active
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_logistics_sync_shipments;
    SELECT COUNT(*) AS active_alerts FROM dbo.LogisticsAlerts WHERE status = 'ACTIVE';
END
GO

IF OBJECT_ID('dbo.sp_vehicle_options_available', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_options_available;
GO
CREATE PROCEDURE dbo.sp_vehicle_options_available
    @for_shipment_id INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_logistics_sync_shipments;

    DECLARE @now DATETIME = GETDATE();

    SELECT
        v.id_vehicle AS IdVehicle,
        CONCAT(v.plate, N' - ', vt.name) AS Name
    FROM dbo.Vehicles v
    INNER JOIN dbo.VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    WHERE v.deleted_at IS NULL
      AND v.status = 1
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.Shipments s
          WHERE s.deleted_at IS NULL
            AND s.id_vehicle = v.id_vehicle
            AND s.departure_date IS NOT NULL
            AND s.departure_date <= @now
            AND (s.return_available_at IS NULL OR s.return_available_at > @now)
            AND (@for_shipment_id IS NULL OR s.id_shipment <> @for_shipment_id)
      )
    ORDER BY v.plate;
END
GO

IF OBJECT_ID('dbo.sp_shipment_tracking_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_shipment_tracking_list;
GO
CREATE PROCEDURE dbo.sp_shipment_tracking_list
    @in_transit_only BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_logistics_sync_shipments;

    DECLARE @status_transito INT = (SELECT TOP 1 id_shipment_status FROM dbo.ShipmentStatuses WHERE name = N'En Transito' AND deleted_at IS NULL);
    DECLARE @now DATETIME = GETDATE();

    SELECT
        s.id_shipment AS IdShipment,
        s.id_vehicle AS IdVehicle,
        v.plate AS VehiclePlate,
        vt.name AS VehicleTypeName,
        ss.name AS ShipmentStatusName,
        s.departure_date AS DepartureDate,
        s.arrival_date AS ArrivalDate,
        s.return_available_at AS ReturnAvailableAt,
        ISNULL(sd.delivery_address, N'Sin direccion - asocie una venta') AS DeliveryAddress,
        ISNULL(sd.client_name, N'—') AS ClientName,
        ISNULL(sd.simulated_distance_km, 0) AS SimulatedDistanceKm,
        ISNULL(sd.travel_minutes, 0) AS TravelMinutes,
        ISNULL(sd.origin_latitude, CAST(-12.046374 AS DECIMAL(9,6))) AS OriginLatitude,
        ISNULL(sd.origin_longitude, CAST(-77.042793 AS DECIMAL(9,6))) AS OriginLongitude,
        ISNULL(sd.dest_latitude, CAST(-12.046374 AS DECIMAL(9,6))) AS DestLatitude,
        ISNULL(sd.dest_longitude, CAST(-77.042793 AS DECIMAL(9,6))) AS DestLongitude,
        CAST(CASE
            WHEN s.departure_date IS NULL OR s.arrival_date IS NULL THEN 0.0
            WHEN @now <= s.departure_date THEN 0.0
            WHEN @now >= s.arrival_date THEN 1.0
            ELSE CAST(DATEDIFF(SECOND, s.departure_date, @now) AS FLOAT) / NULLIF(CAST(DATEDIFF(SECOND, s.departure_date, s.arrival_date) AS FLOAT), 0)
        END AS DECIMAL(10,6)) AS RouteProgress,
        CAST(CASE
            WHEN sd.dest_latitude IS NULL OR sd.dest_longitude IS NULL THEN ISNULL(sd.origin_latitude, -12.046374)
            ELSE sd.origin_latitude + ((sd.dest_latitude - sd.origin_latitude) * CASE
                WHEN s.departure_date IS NULL OR s.arrival_date IS NULL THEN 0.0
                WHEN @now <= s.departure_date THEN 0.0
                WHEN @now >= s.arrival_date THEN 1.0
                ELSE CAST(DATEDIFF(SECOND, s.departure_date, @now) AS FLOAT) / NULLIF(CAST(DATEDIFF(SECOND, s.departure_date, s.arrival_date) AS FLOAT), 0)
            END)
        END AS DECIMAL(9,6)) AS CurrentLatitude,
        CAST(CASE
            WHEN sd.dest_latitude IS NULL OR sd.dest_longitude IS NULL THEN ISNULL(sd.origin_longitude, -77.042793)
            ELSE sd.origin_longitude + ((sd.dest_longitude - sd.origin_longitude) * CASE
                WHEN s.departure_date IS NULL OR s.arrival_date IS NULL THEN 0.0
                WHEN @now <= s.departure_date THEN 0.0
                WHEN @now >= s.arrival_date THEN 1.0
                ELSE CAST(DATEDIFF(SECOND, s.departure_date, @now) AS FLOAT) / NULLIF(CAST(DATEDIFF(SECOND, s.departure_date, s.arrival_date) AS FLOAT), 0)
            END)
        END AS DECIMAL(9,6)) AS CurrentLongitude
    FROM dbo.Shipments s
    INNER JOIN dbo.Vehicles v ON v.id_vehicle = s.id_vehicle
    INNER JOIN dbo.VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    INNER JOIN dbo.ShipmentStatuses ss ON ss.id_shipment_status = s.id_shipment_status
    LEFT JOIN dbo.ShipmentDetails sd ON sd.id_shipment = s.id_shipment
    WHERE s.deleted_at IS NULL
      AND (
            @in_transit_only = 0
         OR s.id_shipment_status = @status_transito
         OR (s.departure_date IS NOT NULL AND s.departure_date <= @now AND (s.arrival_date IS NULL OR s.arrival_date > @now))
      )
    ORDER BY s.departure_date DESC, s.id_shipment DESC;
END
GO

/* ---- Vehiculos: dimensiones y volumen calculado ---- */
IF OBJECT_ID('dbo.sp_vehicle_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_list_active;
GO
CREATE PROCEDURE dbo.sp_vehicle_list_active
    @search VARCHAR(100) = NULL, @page INT = 1, @page_size INT = 10, @id_vehicle_type INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.id_vehicle AS IdVehicle, v.id_vehicle_type AS IdVehicleType, vt.name AS VehicleTypeName,
           v.plate AS Plate, v.maximum_weight AS MaximumWeight, v.height AS Height, v.width AS Width, v.length AS Length,
           ISNULL(v.maximum_volume, ISNULL(v.height,0)*ISNULL(v.width,0)*ISNULL(v.length,0)) AS MaximumVolume,
           v.status AS Status, COUNT(*) OVER() AS TotalCount
    FROM Vehicles v INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    WHERE v.deleted_at IS NULL AND v.status = 1
      AND (@id_vehicle_type IS NULL OR v.id_vehicle_type = @id_vehicle_type)
      AND (@search IS NULL OR @search = '' OR v.plate LIKE '%'+@search+'%' OR vt.name LIKE '%'+@search+'%')
    ORDER BY v.id_vehicle DESC OFFSET (@page-1)*@page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_list_inactive;
GO
CREATE PROCEDURE dbo.sp_vehicle_list_inactive
    @search VARCHAR(100) = NULL, @page INT = 1, @page_size INT = 10, @id_vehicle_type INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.id_vehicle AS IdVehicle, v.id_vehicle_type AS IdVehicleType, vt.name AS VehicleTypeName,
           v.plate AS Plate, v.maximum_weight AS MaximumWeight, v.height AS Height, v.width AS Width, v.length AS Length,
           ISNULL(v.maximum_volume, ISNULL(v.height,0)*ISNULL(v.width,0)*ISNULL(v.length,0)) AS MaximumVolume,
           v.status AS Status, COUNT(*) OVER() AS TotalCount
    FROM Vehicles v INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    WHERE v.deleted_at IS NULL AND v.status = 0
      AND (@id_vehicle_type IS NULL OR v.id_vehicle_type = @id_vehicle_type)
      AND (@search IS NULL OR @search = '' OR v.plate LIKE '%'+@search+'%' OR vt.name LIKE '%'+@search+'%')
    ORDER BY v.id_vehicle DESC OFFSET (@page-1)*@page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_get_by_id;
GO
CREATE PROCEDURE dbo.sp_vehicle_get_by_id @id_vehicle INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.id_vehicle AS IdVehicle, v.id_vehicle_type AS IdVehicleType, vt.name AS VehicleTypeName, v.plate AS Plate,
           v.maximum_weight AS MaximumWeight, v.height AS Height, v.width AS Width, v.length AS Length,
           ISNULL(v.maximum_volume, ISNULL(v.height,0)*ISNULL(v.width,0)*ISNULL(v.length,0)) AS MaximumVolume,
           v.status AS Status, v.created_at AS CreatedAt, v.updated_at AS UpdatedAt
    FROM Vehicles v INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type
    WHERE v.id_vehicle = @id_vehicle AND v.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_create;
GO
CREATE PROCEDURE dbo.sp_vehicle_create
    @id_vehicle_type INT, @plate VARCHAR(20), @maximum_weight DECIMAL(10,2) = NULL,
    @height DECIMAL(10,2) = NULL, @width DECIMAL(10,2) = NULL, @length DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @plate = UPPER(LTRIM(RTRIM(@plate)));
    DECLARE @vol DECIMAL(10,2) = ISNULL(@height,0)*ISNULL(@width,0)*ISNULL(@length,0);
    IF NOT EXISTS (SELECT 1 FROM VehicleTypes WHERE id_vehicle_type=@id_vehicle_type AND deleted_at IS NULL AND status=1)
    BEGIN SELECT 0 AS Success, N'Seleccione un tipo de vehiculo valido.' AS Message, NULL AS IdVehicle; RETURN; END
    IF EXISTS (SELECT 1 FROM Vehicles WHERE UPPER(plate)=@plate AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, N'Ya existe un vehiculo con esa placa.' AS Message, NULL AS IdVehicle; RETURN; END
    INSERT INTO Vehicles (id_vehicle_type, plate, maximum_weight, height, width, length, maximum_volume)
    VALUES (@id_vehicle_type, @plate, @maximum_weight, @height, @width, @length, NULLIF(@vol,0));
    SELECT 1 AS Success, N'Vehiculo creado correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdVehicle;
END
GO

IF OBJECT_ID('dbo.sp_vehicle_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_vehicle_update;
GO
CREATE PROCEDURE dbo.sp_vehicle_update
    @id_vehicle INT, @id_vehicle_type INT, @plate VARCHAR(20), @maximum_weight DECIMAL(10,2) = NULL,
    @height DECIMAL(10,2) = NULL, @width DECIMAL(10,2) = NULL, @length DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @plate = UPPER(LTRIM(RTRIM(@plate)));
    DECLARE @vol DECIMAL(10,2) = ISNULL(@height,0)*ISNULL(@width,0)*ISNULL(@length,0);
    IF NOT EXISTS (SELECT 1 FROM Vehicles WHERE id_vehicle=@id_vehicle AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, N'Registro no encontrado.' AS Message; RETURN; END
    IF NOT EXISTS (SELECT 1 FROM VehicleTypes WHERE id_vehicle_type=@id_vehicle_type AND deleted_at IS NULL AND status=1)
    BEGIN SELECT 0 AS Success, N'Seleccione un tipo de vehiculo valido.' AS Message; RETURN; END
    IF EXISTS (SELECT 1 FROM Vehicles WHERE UPPER(plate)=@plate AND id_vehicle<>@id_vehicle AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, N'Ya existe otro vehiculo con esa placa.' AS Message; RETURN; END
    UPDATE Vehicles SET id_vehicle_type=@id_vehicle_type, plate=@plate, maximum_weight=@maximum_weight,
        height=@height, width=@width, length=@length, maximum_volume=NULLIF(@vol,0), updated_at=GETDATE()
    WHERE id_vehicle=@id_vehicle AND deleted_at IS NULL;
    SELECT 1 AS Success, N'Vehiculo actualizado correctamente.' AS Message;
END
GO

/* ---- Envios: validar disponibilidad y llegada automatica ---- */
IF OBJECT_ID('dbo.sp_shipment_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_shipment_create;
GO
CREATE PROCEDURE dbo.sp_shipment_create
    @id_vehicle INT, @id_employee INT, @id_shipment_status INT, @departure_date DATETIME = NULL, @arrival_date DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_logistics_sync_shipments;

    IF NOT EXISTS (SELECT 1 FROM Vehicles WHERE id_vehicle=@id_vehicle AND deleted_at IS NULL AND status=1)
    BEGIN SELECT 0 AS Success, N'Seleccione un vehiculo activo.' AS Message, NULL AS IdShipment; RETURN; END

    IF EXISTS (
        SELECT 1 FROM Shipments s WHERE s.deleted_at IS NULL AND s.id_vehicle=@id_vehicle
          AND s.departure_date IS NOT NULL AND s.departure_date <= GETDATE()
          AND (s.return_available_at IS NULL OR s.return_available_at > GETDATE())
    )
    BEGIN SELECT 0 AS Success, N'El vehiculo no esta disponible en ese horario.' AS Message, NULL AS IdShipment; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Employees WHERE id_employee=@id_employee AND deleted_at IS NULL AND status=1)
    BEGIN SELECT 0 AS Success, N'Seleccione un empleado activo.' AS Message, NULL AS IdShipment; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM ShipmentStatuses WHERE id_shipment_status=@id_shipment_status AND deleted_at IS NULL AND status=1)
    BEGIN SELECT 0 AS Success, N'Seleccione un estado de envio activo.' AS Message, NULL AS IdShipment; RETURN; END

    INSERT INTO Shipments (id_vehicle, id_employee, id_shipment_status, departure_date, arrival_date)
    VALUES (@id_vehicle, @id_employee, @id_shipment_status, @departure_date, @arrival_date);

    SELECT 1 AS Success, N'Envio creado correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdShipment;
END
GO

IF OBJECT_ID('dbo.sp_shipment_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_shipment_update;
GO
CREATE PROCEDURE dbo.sp_shipment_update
    @id_shipment INT, @id_vehicle INT, @id_employee INT, @id_shipment_status INT,
    @departure_date DATETIME = NULL, @arrival_date DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_logistics_sync_shipments;

    IF NOT EXISTS (SELECT 1 FROM Shipments WHERE id_shipment=@id_shipment AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, N'Registro no encontrado.' AS Message; RETURN; END

    IF EXISTS (
        SELECT 1 FROM Shipments s WHERE s.deleted_at IS NULL AND s.id_vehicle=@id_vehicle AND s.id_shipment<>@id_shipment
          AND s.departure_date IS NOT NULL AND s.departure_date <= GETDATE()
          AND (s.return_available_at IS NULL OR s.return_available_at > GETDATE())
    )
    BEGIN SELECT 0 AS Success, N'El vehiculo no esta disponible en ese horario.' AS Message; RETURN; END

    UPDATE Shipments SET id_vehicle=@id_vehicle, id_employee=@id_employee, id_shipment_status=@id_shipment_status,
        departure_date=@departure_date, arrival_date=@arrival_date, updated_at=GETDATE()
    WHERE id_shipment=@id_shipment AND deleted_at IS NULL;

    IF EXISTS (SELECT 1 FROM ShipmentDetails WHERE id_shipment=@id_shipment) AND @departure_date IS NOT NULL
    BEGIN
        UPDATE s SET arrival_date = DATEADD(MINUTE, sd.travel_minutes, @departure_date), updated_at = GETDATE()
        FROM Shipments s INNER JOIN ShipmentDetails sd ON sd.id_shipment = s.id_shipment
        WHERE s.id_shipment = @id_shipment;
    END

    SELECT 1 AS Success, N'Envio actualizado correctamente.' AS Message;
END
GO

IF OBJECT_ID('dbo.sp_shipment_sale_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_shipment_sale_create;
GO
CREATE PROCEDURE dbo.sp_shipment_sale_create @id_shipment INT, @id_sale INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Shipments WHERE id_shipment=@id_shipment AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, N'Seleccione un envio valido.' AS Message, NULL AS IdShipmentSale; RETURN; END
    IF NOT EXISTS (SELECT 1 FROM Sales WHERE id_sale=@id_sale AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, N'Seleccione una venta valida.' AS Message, NULL AS IdShipmentSale; RETURN; END
    IF EXISTS (SELECT 1 FROM ShipmentSales WHERE id_shipment=@id_shipment AND id_sale=@id_sale AND deleted_at IS NULL)
    BEGIN SELECT 0 AS Success, N'La venta ya esta asociada a este envio.' AS Message, NULL AS IdShipmentSale; RETURN; END

    INSERT INTO ShipmentSales (id_shipment, id_sale) VALUES (@id_shipment, @id_sale);
    EXEC dbo.sp_shipment_detail_sync @id_shipment;

    SELECT 1 AS Success, N'Venta asociada al envio correctamente.' AS Message, CAST(SCOPE_IDENTITY() AS INT) AS IdShipmentSale;
END
GO
