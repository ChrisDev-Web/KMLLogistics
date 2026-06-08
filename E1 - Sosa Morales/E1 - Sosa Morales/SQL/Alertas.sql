-- ============================================================
-- KMLLogistics - Alertas de stock
-- Base de datos: SQL Server 2017
-- Requiere: WarehouseDetails, Users (ya creadas)
-- Nomenclatura: sp_(tabla_singular)_(funcion)_(campo_opcional)
-- ============================================================

USE KMLLogistics;
GO

-- ============================================================
-- Actualizar WarehouseDetails (tabla ya existente)
-- Agrega stock mínimo por producto/almacén
-- (cada paso en su propio lote para que SQL Server reconozca la columna)
-- ============================================================
IF OBJECT_ID('dbo.WarehouseDetails', 'U') IS NULL
BEGIN
    RAISERROR('La tabla dbo.WarehouseDetails no existe en KMLLogistics.', 16, 1);
END
GO

IF COL_LENGTH('dbo.WarehouseDetails', 'min_stock') IS NULL
BEGIN
    ALTER TABLE dbo.WarehouseDetails
        ADD min_stock INT NOT NULL  
            CONSTRAINT df_warehouse_details_min_stock DEFAULT (0) WITH VALUES;
END
GO

IF COL_LENGTH('dbo.WarehouseDetails', 'min_stock') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.check_constraints
       WHERE name = 'chk_warehouse_min_stock'
         AND parent_object_id = OBJECT_ID('dbo.WarehouseDetails')
   )
BEGIN
    ALTER TABLE dbo.WarehouseDetails
        ADD CONSTRAINT chk_warehouse_min_stock CHECK (min_stock >= 0);
END
GO

-- ============================================================
-- StockAlerts
-- Una alerta activa por detalle de almacén (producto + almacén)
-- ============================================================
IF OBJECT_ID('dbo.StockAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockAlerts (
        id_stock_alert      INT IDENTITY(1,1) PRIMARY KEY,
        id_warehouse_detail INT NOT NULL,
        status              VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        first_triggered_at  DATETIME NOT NULL DEFAULT GETDATE(),
        last_notified_at    DATETIME NOT NULL DEFAULT GETDATE(),
        notification_count  INT NOT NULL DEFAULT 1,
        last_sent_by_user   INT NULL,
        resolved_at         DATETIME NULL,
        created_at          DATETIME NOT NULL DEFAULT GETDATE(),
        updated_at          DATETIME NULL,

        CONSTRAINT fk_stock_alert_warehouse_detail
            FOREIGN KEY (id_warehouse_detail)
            REFERENCES dbo.WarehouseDetails(id_warehouse_detail),

        CONSTRAINT fk_stock_alert_user
            FOREIGN KEY (last_sent_by_user)
            REFERENCES dbo.Users(id_user),

        CONSTRAINT uq_stock_alert_warehouse_detail
            UNIQUE (id_warehouse_detail),

        CONSTRAINT chk_stock_alert_status
            CHECK (status IN ('ACTIVE', 'RESOLVED')),

        CONSTRAINT chk_stock_alert_notification_count
            CHECK (notification_count > 0)
    );

    CREATE INDEX ix_stock_alerts_status
        ON StockAlerts (status)
        INCLUDE (id_warehouse_detail, last_notified_at);
END
GO

-- ============================================================
-- sp_stock_alert_check
-- Evalúa un detalle de almacén tras un cambio de stock.
-- Crea alerta si stock <= min_stock, o resuelve si se repuso.
-- ============================================================
IF COL_LENGTH('dbo.WarehouseDetails', 'min_stock') IS NULL
BEGIN
    RAISERROR('Ejecuta primero el bloque ALTER de WarehouseDetails (columna min_stock).', 16, 1);
END
GO

IF OBJECT_ID('dbo.sp_stock_alert_check', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_stock_alert_check;
GO

CREATE PROCEDURE dbo.sp_stock_alert_check
    @id_warehouse_detail INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @stock     INT;
    DECLARE @min_stock INT;

    SELECT
        @stock     = wd.stock,
        @min_stock = wd.min_stock
    FROM dbo.WarehouseDetails wd
    WHERE wd.id_warehouse_detail = @id_warehouse_detail;

    IF @stock IS NULL
        RETURN;

    IF @stock <= @min_stock
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM dbo.StockAlerts
            WHERE id_warehouse_detail = @id_warehouse_detail
              AND status = 'ACTIVE'
        )
        BEGIN
            UPDATE dbo.StockAlerts
            SET
                last_notified_at   = GETDATE(),
                notification_count = notification_count + 1,
                last_sent_by_user  = NULL,
                updated_at         = GETDATE()
            WHERE id_warehouse_detail = @id_warehouse_detail
              AND status = 'ACTIVE';
        END
        ELSE
        BEGIN
            INSERT INTO dbo.StockAlerts (
                id_warehouse_detail,
                status,
                first_triggered_at,
                last_notified_at,
                notification_count,
                last_sent_by_user
            )
            VALUES (
                @id_warehouse_detail,
                'ACTIVE',
                GETDATE(),
                GETDATE(),
                1,
                NULL
            );
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.StockAlerts
        SET
            status      = 'RESOLVED',
            resolved_at = GETDATE(),
            updated_at  = GETDATE()
        WHERE id_warehouse_detail = @id_warehouse_detail
          AND status = 'ACTIVE';
    END
END
GO

-- ============================================================
-- sp_stock_alert_list_active
-- Lista alertas con buscador y filtros (vista AlertasStock / campana)
-- @status: ACTIVE (default), RESOLVED, ALL
-- Sin parámetros = solo alertas activas (compatibilidad campana)
-- ============================================================
IF OBJECT_ID('dbo.sp_stock_alert_list', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_stock_alert_list;
GO

IF OBJECT_ID('dbo.sp_stock_alert_list_active', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_stock_alert_list_active;
GO

CREATE PROCEDURE dbo.sp_stock_alert_list_active
    @search        VARCHAR(100) = NULL,
    @id_product    INT = NULL,
    @id_warehouse  INT = NULL,
    @status        VARCHAR(20) = 'ACTIVE'
AS
BEGIN
    SET NOCOUNT ON;

    SET @search = NULLIF(LTRIM(RTRIM(@search)), '');
    SET @status = UPPER(NULLIF(LTRIM(RTRIM(@status)), ''));

    IF @status IS NULL OR @status NOT IN ('ACTIVE', 'RESOLVED', 'ALL')
        SET @status = 'ACTIVE';

    SELECT
        sa.id_stock_alert,
        sa.id_warehouse_detail,
        sa.status,
        sa.first_triggered_at,
        sa.last_notified_at,
        sa.notification_count,
        sa.last_sent_by_user,
        sa.resolved_at,
        wd.id_warehouse,
        w.name AS warehouse_name,
        wd.id_product,
        p.name AS product_name,
        wd.stock,
        wd.min_stock,
        wd.location,
        CASE
            WHEN wd.stock < wd.min_stock THEN wd.min_stock - wd.stock
            ELSE 0
        END AS stock_deficit,
        u.username AS last_sent_by_username
    FROM dbo.StockAlerts sa
    INNER JOIN dbo.WarehouseDetails wd ON wd.id_warehouse_detail = sa.id_warehouse_detail
    INNER JOIN dbo.Warehouses w        ON w.id_warehouse = wd.id_warehouse
    INNER JOIN dbo.Products p          ON p.id_product = wd.id_product
    LEFT JOIN dbo.Users u              ON u.id_user = sa.last_sent_by_user
    WHERE w.deleted_at IS NULL
      AND p.deleted_at IS NULL
      AND (
            (@status = 'ALL' AND sa.status IN ('ACTIVE', 'RESOLVED'))
         OR (@status = 'RESOLVED' AND sa.status = 'RESOLVED')
         OR (@status = 'ACTIVE' AND sa.status = 'ACTIVE' AND wd.stock <= wd.min_stock)
      )
      AND (@id_product IS NULL OR wd.id_product = @id_product)
      AND (@id_warehouse IS NULL OR wd.id_warehouse = @id_warehouse)
      AND (
            @search IS NULL
         OR p.name LIKE '%' + @search + '%'
         OR w.name LIKE '%' + @search + '%'
         OR wd.location LIKE '%' + @search + '%'
         OR u.username LIKE '%' + @search + '%'
      )
    ORDER BY sa.last_notified_at DESC, p.name ASC;
END
GO

-- ============================================================
-- sp_stock_alert_list_products_filter
-- Productos con alertas para combo de filtro
-- ============================================================
IF OBJECT_ID('dbo.sp_stock_alert_list_products_filter', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_stock_alert_list_products_filter;
GO

CREATE PROCEDURE dbo.sp_stock_alert_list_products_filter
    @status VARCHAR(20) = 'ALL'
AS
BEGIN
    SET NOCOUNT ON;

    SET @status = UPPER(NULLIF(LTRIM(RTRIM(@status)), ''));
    IF @status IS NULL OR @status NOT IN ('ACTIVE', 'RESOLVED', 'ALL')
        SET @status = 'ALL';

    SELECT DISTINCT
        p.id_product AS id,
        p.name
    FROM dbo.StockAlerts sa
    INNER JOIN dbo.WarehouseDetails wd ON wd.id_warehouse_detail = sa.id_warehouse_detail
    INNER JOIN dbo.Products p          ON p.id_product = wd.id_product
    INNER JOIN dbo.Warehouses w        ON w.id_warehouse = wd.id_warehouse
    WHERE p.deleted_at IS NULL
      AND w.deleted_at IS NULL
      AND (
            (@status = 'ALL' AND sa.status IN ('ACTIVE', 'RESOLVED'))
         OR (@status = 'RESOLVED' AND sa.status = 'RESOLVED')
         OR (@status = 'ACTIVE' AND sa.status = 'ACTIVE' AND wd.stock <= wd.min_stock)
      )
    ORDER BY p.name ASC;
END
GO

-- ============================================================
-- sp_stock_alert_list_warehouses_filter
-- Almacenes con alertas para combo de filtro
-- ============================================================
IF OBJECT_ID('dbo.sp_stock_alert_list_warehouses_filter', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_stock_alert_list_warehouses_filter;
GO

CREATE PROCEDURE dbo.sp_stock_alert_list_warehouses_filter
    @status VARCHAR(20) = 'ALL'
AS
BEGIN
    SET NOCOUNT ON;

    SET @status = UPPER(NULLIF(LTRIM(RTRIM(@status)), ''));
    IF @status IS NULL OR @status NOT IN ('ACTIVE', 'RESOLVED', 'ALL')
        SET @status = 'ALL';

    SELECT DISTINCT
        w.id_warehouse AS id,
        w.name
    FROM dbo.StockAlerts sa
    INNER JOIN dbo.WarehouseDetails wd ON wd.id_warehouse_detail = sa.id_warehouse_detail
    INNER JOIN dbo.Warehouses w        ON w.id_warehouse = wd.id_warehouse
    INNER JOIN dbo.Products p          ON p.id_product = wd.id_product
    WHERE w.deleted_at IS NULL
      AND p.deleted_at IS NULL
      AND (
            (@status = 'ALL' AND sa.status IN ('ACTIVE', 'RESOLVED'))
         OR (@status = 'RESOLVED' AND sa.status = 'RESOLVED')
         OR (@status = 'ACTIVE' AND sa.status = 'ACTIVE' AND wd.stock <= wd.min_stock)
      )
    ORDER BY w.name ASC;
END
GO

-- ============================================================
-- sp_stock_alert_resend
-- Reenvío manual por admin (stock sigue bajo)
-- ============================================================
IF OBJECT_ID('dbo.sp_stock_alert_resend', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_stock_alert_resend;
GO

CREATE PROCEDURE dbo.sp_stock_alert_resend
    @id_stock_alert    INT,
    @id_user           INT,
    @message           VARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_warehouse_detail INT;
    DECLARE @stock               INT;
    DECLARE @min_stock           INT;

    SELECT
        @id_warehouse_detail = sa.id_warehouse_detail
    FROM dbo.StockAlerts sa
    WHERE sa.id_stock_alert = @id_stock_alert
      AND sa.status = 'ACTIVE';

    IF @id_warehouse_detail IS NULL
    BEGIN
        SET @message = 'La alerta no existe o ya fue resuelta.';
        RETURN;
    END

    SELECT
        @stock     = wd.stock,
        @min_stock = wd.min_stock
    FROM dbo.WarehouseDetails wd
    WHERE wd.id_warehouse_detail = @id_warehouse_detail;

    IF @stock > @min_stock
    BEGIN
        UPDATE dbo.StockAlerts
        SET
            status      = 'RESOLVED',
            resolved_at = GETDATE(),
            updated_at  = GETDATE()
        WHERE id_stock_alert = @id_stock_alert;

        SET @message = 'El stock ya fue repuesto. La alerta se cerró automáticamente.';
        RETURN;
    END

    UPDATE dbo.StockAlerts
    SET
        last_notified_at   = GETDATE(),
        notification_count = notification_count + 1,
        last_sent_by_user  = @id_user,
        updated_at         = GETDATE()
    WHERE id_stock_alert = @id_stock_alert
      AND status = 'ACTIVE';

    SET @message = 'Alerta reenviada correctamente.';
END
GO

-- ============================================================
-- sp_stock_alert_count_active
-- Contador para el ícono de campana en el header
-- ============================================================
IF OBJECT_ID('dbo.sp_stock_alert_count_active', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_stock_alert_count_active;
GO

CREATE PROCEDURE dbo.sp_stock_alert_count_active
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS active_alerts
    FROM dbo.StockAlerts sa
    INNER JOIN dbo.WarehouseDetails wd ON wd.id_warehouse_detail = sa.id_warehouse_detail
    WHERE sa.status = 'ACTIVE'
      AND wd.stock <= wd.min_stock;
END
GO

-- Verificación rápida
SELECT
    COL_LENGTH('dbo.WarehouseDetails', 'min_stock') AS min_stock_column_exists,
    OBJECT_ID('dbo.StockAlerts', 'U')              AS stock_alerts_table_exists;
GO

-- ############################################################
-- PERFIL DE USUARIO (Users.photo)
-- ############################################################

IF OBJECT_ID('dbo.sp_user_get_by_username', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_user_get_by_username;
GO
CREATE PROCEDURE dbo.sp_user_get_by_username
    @username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.id_user,
        u.id_role,
        u.username,
        u.password_hash,
        u.photo,
        u.created_at,
        u.updated_at,
        r.name AS role_name
    FROM Users u
    INNER JOIN Roles r ON r.id_role = u.id_role
    WHERE u.username = @username
      AND u.deleted_at IS NULL
      AND r.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_user_profile_get_by_id', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_user_profile_get_by_id;
GO
CREATE PROCEDURE dbo.sp_user_profile_get_by_id
    @id_user INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.id_user,
        u.id_role,
        u.username,
        u.photo,
        r.name AS role_name,
        u.created_at,
        u.updated_at
    FROM Users u
    INNER JOIN Roles r ON r.id_role = u.id_role
    WHERE u.id_user = @id_user
      AND u.deleted_at IS NULL
      AND r.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_user_profile_update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_user_profile_update;
GO
CREATE PROCEDURE dbo.sp_user_profile_update
    @id_user       INT,
    @username      VARCHAR(50),
    @password_hash VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE id_user = @id_user AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Registro no encontrado.' AS message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Users WHERE username = @username AND id_user <> @id_user AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Ya existe otro usuario con ese nombre.' AS message;
        RETURN;
    END

    IF @password_hash IS NULL OR @password_hash = ''
        UPDATE Users SET username = @username, updated_at = GETDATE() WHERE id_user = @id_user;
    ELSE
        UPDATE Users SET username = @username, password_hash = @password_hash, updated_at = GETDATE() WHERE id_user = @id_user;

    SELECT 1 AS success, 'Perfil actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_user_profile_update_photo', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_user_profile_update_photo;
GO
CREATE PROCEDURE dbo.sp_user_profile_update_photo
    @id_user INT,
    @photo   VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE id_user = @id_user AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Registro no encontrado.' AS message;
        RETURN;
    END

    UPDATE Users SET photo = @photo, updated_at = GETDATE() WHERE id_user = @id_user;
    SELECT 1 AS success, 'Foto de perfil actualizada correctamente.' AS message;
END
GO
