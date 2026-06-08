-- ============================================================
-- KMLLogistics - Stored Procedures: Personas y Terceros
-- Módulos: Clients, Suppliers
-- Nomenclatura: sp_{entidad_singular}_{funcion}_{campo_opcional}
-- ============================================================

USE KMLLogistics;
GO

-- ############################################################
-- CLIENTS
-- ############################################################

IF OBJECT_ID('dbo.sp_client_document_type_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_document_type_list_active;
GO
CREATE PROCEDURE dbo.sp_client_document_type_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_document_type, name
    FROM DocumentTypes
    WHERE deleted_at IS NULL AND status = 1
    ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_client_district_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_district_list_active;
GO
CREATE PROCEDURE dbo.sp_client_district_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.id_district,
        c.name + ' / ' + r.name + ' / ' + p.name + ' / ' + d.name AS name
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

IF OBJECT_ID('dbo.sp_client_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_list_active;
GO
CREATE PROCEDURE dbo.sp_client_list_active
    @search           VARCHAR(100) = NULL,
    @id_document_type INT = NULL,
    @id_district      INT = NULL,
    @page             INT = 1,
    @page_size        INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        cl.id_client,
        dt.name AS document_type_name,
        cl.document_number,
        cl.name,
        cl.last_name_paternal,
        cl.last_name_maternal,
        cl.phone,
        cl.email,
        ISNULL(d.name, '') AS district_name,
        COUNT(*) OVER() AS total_count
    FROM Clients cl
    INNER JOIN DocumentTypes dt ON dt.id_document_type = cl.id_document_type
    LEFT JOIN Districts d ON d.id_district = cl.id_district
    WHERE cl.deleted_at IS NULL AND cl.status = 1
      AND dt.deleted_at IS NULL
      AND (@id_document_type IS NULL OR cl.id_document_type = @id_document_type)
      AND (@id_district IS NULL OR cl.id_district = @id_district)
      AND (@search IS NULL OR @search = ''
           OR cl.document_number LIKE '%' + @search + '%'
           OR cl.name LIKE '%' + @search + '%'
           OR cl.last_name_paternal LIKE '%' + @search + '%'
           OR cl.last_name_maternal LIKE '%' + @search + '%'
           OR cl.email LIKE '%' + @search + '%'
           OR cl.phone LIKE '%' + @search + '%'
           OR dt.name LIKE '%' + @search + '%')
    ORDER BY cl.id_client DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_client_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_list_inactive;
GO
CREATE PROCEDURE dbo.sp_client_list_inactive
    @search           VARCHAR(100) = NULL,
    @id_document_type INT = NULL,
    @id_district      INT = NULL,
    @page             INT = 1,
    @page_size        INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        cl.id_client,
        dt.name AS document_type_name,
        cl.document_number,
        cl.name,
        cl.last_name_paternal,
        cl.last_name_maternal,
        cl.phone,
        cl.email,
        ISNULL(d.name, '') AS district_name,
        COUNT(*) OVER() AS total_count
    FROM Clients cl
    INNER JOIN DocumentTypes dt ON dt.id_document_type = cl.id_document_type
    LEFT JOIN Districts d ON d.id_district = cl.id_district
    WHERE cl.deleted_at IS NULL AND cl.status = 0
      AND dt.deleted_at IS NULL
      AND (@id_document_type IS NULL OR cl.id_document_type = @id_document_type)
      AND (@id_district IS NULL OR cl.id_district = @id_district)
      AND (@search IS NULL OR @search = ''
           OR cl.document_number LIKE '%' + @search + '%'
           OR cl.name LIKE '%' + @search + '%'
           OR cl.last_name_paternal LIKE '%' + @search + '%'
           OR cl.last_name_maternal LIKE '%' + @search + '%'
           OR cl.email LIKE '%' + @search + '%'
           OR cl.phone LIKE '%' + @search + '%'
           OR dt.name LIKE '%' + @search + '%')
    ORDER BY cl.id_client DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_client_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_get_by_id;
GO
CREATE PROCEDURE dbo.sp_client_get_by_id @id_client INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        cl.id_client,
        cl.id_document_type,
        dt.name AS document_type_name,
        cl.document_number,
        cl.name,
        cl.last_name_paternal,
        cl.last_name_maternal,
        cl.phone,
        cl.email,
        cl.address,
        cl.id_district,
        ISNULL(c.name, '') AS country_name,
        ISNULL(r.name, '') AS region_name,
        ISNULL(p.name, '') AS province_name,
        ISNULL(d.name, '') AS district_name,
        cl.status,
        cl.created_at,
        cl.updated_at
    FROM Clients cl
    INNER JOIN DocumentTypes dt ON dt.id_document_type = cl.id_document_type
    LEFT JOIN Districts d ON d.id_district = cl.id_district
    LEFT JOIN Provinces p ON p.id_province = d.id_province
    LEFT JOIN Regions r ON r.id_region = p.id_region
    LEFT JOIN Countries c ON c.id_country = r.id_country
    WHERE cl.id_client = @id_client AND cl.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_client_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_create;
GO
CREATE PROCEDURE dbo.sp_client_create
    @id_document_type    INT,
    @document_number     VARCHAR(20),
    @name                VARCHAR(80),
    @last_name_paternal  VARCHAR(80),
    @last_name_maternal  VARCHAR(80) = NULL,
    @phone               VARCHAR(20) = NULL,
    @email               VARCHAR(100) = NULL,
    @address             VARCHAR(255) = NULL,
    @id_district         INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El tipo de documento no es válido.' AS message, NULL AS id_client; RETURN; END

    IF EXISTS (SELECT 1 FROM Clients WHERE id_document_type = @id_document_type AND document_number = @document_number AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un cliente con ese tipo y número de documento.' AS message, NULL AS id_client; RETURN; END

    IF @email IS NOT NULL AND @email <> '' AND EXISTS (SELECT 1 FROM Clients WHERE email = @email AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un cliente con ese correo electrónico.' AS message, NULL AS id_client; RETURN; END

    IF @id_district IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El distrito seleccionado no es válido.' AS message, NULL AS id_client; RETURN; END

    INSERT INTO Clients (id_document_type, document_number, name, last_name_paternal, last_name_maternal, phone, email, address, id_district)
    VALUES (@id_document_type, @document_number, @name, @last_name_paternal, @last_name_maternal, @phone, @email, @address, @id_district);

    SELECT 1 AS success, 'Cliente creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_client;
END
GO

IF OBJECT_ID('dbo.sp_client_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_update;
GO
CREATE PROCEDURE dbo.sp_client_update
    @id_client           INT,
    @id_document_type    INT,
    @document_number     VARCHAR(20),
    @name                VARCHAR(80),
    @last_name_paternal  VARCHAR(80),
    @last_name_maternal  VARCHAR(80) = NULL,
    @phone               VARCHAR(20) = NULL,
    @email               VARCHAR(100) = NULL,
    @address             VARCHAR(255) = NULL,
    @id_district         INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Clients WHERE id_client = @id_client AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El tipo de documento no es válido.' AS message; RETURN; END

    IF EXISTS (SELECT 1 FROM Clients WHERE id_document_type = @id_document_type AND document_number = @document_number AND id_client <> @id_client AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro cliente con ese tipo y número de documento.' AS message; RETURN; END

    IF @email IS NOT NULL AND @email <> '' AND EXISTS (SELECT 1 FROM Clients WHERE email = @email AND id_client <> @id_client AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro cliente con ese correo electrónico.' AS message; RETURN; END

    IF @id_district IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El distrito seleccionado no es válido.' AS message; RETURN; END

    UPDATE Clients SET
        id_document_type = @id_document_type,
        document_number = @document_number,
        name = @name,
        last_name_paternal = @last_name_paternal,
        last_name_maternal = @last_name_maternal,
        phone = @phone,
        email = @email,
        address = @address,
        id_district = @id_district,
        updated_at = GETDATE()
    WHERE id_client = @id_client;

    SELECT 1 AS success, 'Cliente actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_client_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_delete_logic;
GO
CREATE PROCEDURE dbo.sp_client_delete_logic @id_client INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Clients WHERE id_client = @id_client AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message; RETURN; END
    UPDATE Clients SET status = 0, updated_at = GETDATE() WHERE id_client = @id_client;
    SELECT 1 AS success, 'Cliente desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_client_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_restore;
GO
CREATE PROCEDURE dbo.sp_client_restore @id_client INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Clients WHERE id_client = @id_client AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message; RETURN; END
    IF EXISTS (
        SELECT 1 FROM Clients c
        WHERE c.id_document_type = (SELECT id_document_type FROM Clients WHERE id_client = @id_client)
          AND c.document_number = (SELECT document_number FROM Clients WHERE id_client = @id_client)
          AND c.status = 1 AND c.deleted_at IS NULL
    )
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe un cliente activo con el mismo documento.' AS message; RETURN; END
    UPDATE Clients SET status = 1, updated_at = GETDATE() WHERE id_client = @id_client;
    SELECT 1 AS success, 'Cliente restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_client_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_client_delete_physical;
GO
CREATE PROCEDURE dbo.sp_client_delete_physical @id_client INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Clients WHERE id_client = @id_client AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Clients WHERE id_client = @id_client;
        SELECT 1 AS success, 'Cliente eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el cliente tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- SUPPLIERS
-- ############################################################

IF OBJECT_ID('dbo.sp_supplier_document_type_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_document_type_list_active;
GO
CREATE PROCEDURE dbo.sp_supplier_document_type_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_document_type, name
    FROM DocumentTypes
    WHERE deleted_at IS NULL AND status = 1
    ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_supplier_district_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_district_list_active;
GO
CREATE PROCEDURE dbo.sp_supplier_district_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.id_district,
        c.name + ' / ' + r.name + ' / ' + p.name + ' / ' + d.name AS name
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

IF OBJECT_ID('dbo.sp_supplier_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_list_active;
GO
CREATE PROCEDURE dbo.sp_supplier_list_active
    @search           VARCHAR(100) = NULL,
    @id_document_type INT = NULL,
    @id_district      INT = NULL,
    @page             INT = 1,
    @page_size        INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        s.id_supplier,
        dt.name AS document_type_name,
        s.document_number,
        s.name,
        s.phone,
        s.email,
        ISNULL(d.name, '') AS district_name,
        COUNT(*) OVER() AS total_count
    FROM Suppliers s
    INNER JOIN DocumentTypes dt ON dt.id_document_type = s.id_document_type
    LEFT JOIN Districts d ON d.id_district = s.id_district
    WHERE s.deleted_at IS NULL AND s.status = 1
      AND dt.deleted_at IS NULL
      AND (@id_document_type IS NULL OR s.id_document_type = @id_document_type)
      AND (@id_district IS NULL OR s.id_district = @id_district)
      AND (@search IS NULL OR @search = ''
           OR s.document_number LIKE '%' + @search + '%'
           OR s.name LIKE '%' + @search + '%'
           OR s.email LIKE '%' + @search + '%'
           OR s.phone LIKE '%' + @search + '%'
           OR dt.name LIKE '%' + @search + '%')
    ORDER BY s.id_supplier DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_supplier_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_list_inactive;
GO
CREATE PROCEDURE dbo.sp_supplier_list_inactive
    @search           VARCHAR(100) = NULL,
    @id_document_type INT = NULL,
    @id_district      INT = NULL,
    @page             INT = 1,
    @page_size        INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        s.id_supplier,
        dt.name AS document_type_name,
        s.document_number,
        s.name,
        s.phone,
        s.email,
        ISNULL(d.name, '') AS district_name,
        COUNT(*) OVER() AS total_count
    FROM Suppliers s
    INNER JOIN DocumentTypes dt ON dt.id_document_type = s.id_document_type
    LEFT JOIN Districts d ON d.id_district = s.id_district
    WHERE s.deleted_at IS NULL AND s.status = 0
      AND dt.deleted_at IS NULL
      AND (@id_document_type IS NULL OR s.id_document_type = @id_document_type)
      AND (@id_district IS NULL OR s.id_district = @id_district)
      AND (@search IS NULL OR @search = ''
           OR s.document_number LIKE '%' + @search + '%'
           OR s.name LIKE '%' + @search + '%'
           OR s.email LIKE '%' + @search + '%'
           OR s.phone LIKE '%' + @search + '%'
           OR dt.name LIKE '%' + @search + '%')
    ORDER BY s.id_supplier DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_supplier_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_get_by_id;
GO
CREATE PROCEDURE dbo.sp_supplier_get_by_id @id_supplier INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.id_supplier,
        s.id_document_type,
        dt.name AS document_type_name,
        s.document_number,
        s.name,
        s.phone,
        s.email,
        s.address,
        s.id_district,
        ISNULL(c.name, '') AS country_name,
        ISNULL(r.name, '') AS region_name,
        ISNULL(p.name, '') AS province_name,
        ISNULL(d.name, '') AS district_name,
        s.status,
        s.created_at,
        s.updated_at
    FROM Suppliers s
    INNER JOIN DocumentTypes dt ON dt.id_document_type = s.id_document_type
    LEFT JOIN Districts d ON d.id_district = s.id_district
    LEFT JOIN Provinces p ON p.id_province = d.id_province
    LEFT JOIN Regions r ON r.id_region = p.id_region
    LEFT JOIN Countries c ON c.id_country = r.id_country
    WHERE s.id_supplier = @id_supplier AND s.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_supplier_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_create;
GO
CREATE PROCEDURE dbo.sp_supplier_create
    @id_document_type INT,
    @document_number  VARCHAR(20),
    @name             VARCHAR(150),
    @phone            VARCHAR(20) = NULL,
    @email            VARCHAR(100) = NULL,
    @address          VARCHAR(255) = NULL,
    @id_district      INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El tipo de documento no es válido.' AS message, NULL AS id_supplier; RETURN; END

    IF EXISTS (SELECT 1 FROM Suppliers WHERE document_number = @document_number AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un proveedor con ese número de documento.' AS message, NULL AS id_supplier; RETURN; END

    IF @email IS NOT NULL AND @email <> '' AND EXISTS (SELECT 1 FROM Suppliers WHERE email = @email AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un proveedor con ese correo electrónico.' AS message, NULL AS id_supplier; RETURN; END

    IF @id_district IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El distrito seleccionado no es válido.' AS message, NULL AS id_supplier; RETURN; END

    INSERT INTO Suppliers (id_document_type, document_number, name, phone, email, address, id_district)
    VALUES (@id_document_type, @document_number, @name, @phone, @email, @address, @id_district);

    SELECT 1 AS success, 'Proveedor creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_supplier;
END
GO

IF OBJECT_ID('dbo.sp_supplier_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_update;
GO
CREATE PROCEDURE dbo.sp_supplier_update
    @id_supplier      INT,
    @id_document_type INT,
    @document_number  VARCHAR(20),
    @name             VARCHAR(150),
    @phone            VARCHAR(20) = NULL,
    @email            VARCHAR(100) = NULL,
    @address          VARCHAR(255) = NULL,
    @id_district      INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE id_supplier = @id_supplier AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El tipo de documento no es válido.' AS message; RETURN; END

    IF EXISTS (SELECT 1 FROM Suppliers WHERE document_number = @document_number AND id_supplier <> @id_supplier AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro proveedor con ese número de documento.' AS message; RETURN; END

    IF @email IS NOT NULL AND @email <> '' AND EXISTS (SELECT 1 FROM Suppliers WHERE email = @email AND id_supplier <> @id_supplier AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro proveedor con ese correo electrónico.' AS message; RETURN; END

    IF @id_district IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El distrito seleccionado no es válido.' AS message; RETURN; END

    UPDATE Suppliers SET
        id_document_type = @id_document_type,
        document_number = @document_number,
        name = @name,
        phone = @phone,
        email = @email,
        address = @address,
        id_district = @id_district,
        updated_at = GETDATE()
    WHERE id_supplier = @id_supplier;

    SELECT 1 AS success, 'Proveedor actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_supplier_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_delete_logic;
GO
CREATE PROCEDURE dbo.sp_supplier_delete_logic @id_supplier INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE id_supplier = @id_supplier AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message; RETURN; END
    UPDATE Suppliers SET status = 0, updated_at = GETDATE() WHERE id_supplier = @id_supplier;
    SELECT 1 AS success, 'Proveedor desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_supplier_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_restore;
GO
CREATE PROCEDURE dbo.sp_supplier_restore @id_supplier INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE id_supplier = @id_supplier AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message; RETURN; END
    IF EXISTS (
        SELECT 1 FROM Suppliers
        WHERE document_number = (SELECT document_number FROM Suppliers WHERE id_supplier = @id_supplier)
          AND status = 1 AND deleted_at IS NULL
    )
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe un proveedor activo con el mismo documento.' AS message; RETURN; END
    UPDATE Suppliers SET status = 1, updated_at = GETDATE() WHERE id_supplier = @id_supplier;
    SELECT 1 AS success, 'Proveedor restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_supplier_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_supplier_delete_physical;
GO
CREATE PROCEDURE dbo.sp_supplier_delete_physical @id_supplier INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE id_supplier = @id_supplier AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Suppliers WHERE id_supplier = @id_supplier;
        SELECT 1 AS success, 'Proveedor eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el proveedor tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- GEOGRAPHY (shared by Clients / Suppliers detail)
-- ############################################################

IF OBJECT_ID('dbo.sp_district_geography_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_geography_get_by_id;
GO
CREATE PROCEDURE dbo.sp_district_geography_get_by_id @id_district INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ISNULL(c.name, '') AS country_name,
        ISNULL(r.name, '') AS region_name,
        ISNULL(p.name, '') AS province_name,
        ISNULL(d.name, '') AS district_name
    FROM Districts d
    LEFT JOIN Provinces p ON p.id_province = d.id_province
    LEFT JOIN Regions r ON r.id_region = p.id_region
    LEFT JOIN Countries c ON c.id_country = r.id_country
    WHERE d.id_district = @id_district;
END
GO

-- ############################################################
-- RECURSOS HUMANOS
-- Módulos: JobPositions, Employees
-- Nomenclatura: sp_job_position_*, sp_employee_*
-- ############################################################

USE KMLLogistics;
GO

-- ############################################################
-- JOB POSITIONS
-- ############################################################

IF OBJECT_ID('dbo.sp_job_position_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_job_position_list_active;
GO
CREATE PROCEDURE dbo.sp_job_position_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        jp.id_job_position,
        jp.name,
        jp.description,
        COUNT(*) OVER() AS total_count
    FROM JobPositions jp
    WHERE jp.deleted_at IS NULL
      AND jp.status = 1
      AND (@search IS NULL OR @search = ''
           OR jp.name LIKE '%' + @search + '%'
           OR jp.description LIKE '%' + @search + '%')
    ORDER BY jp.id_job_position DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_job_position_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_job_position_list_inactive;
GO
CREATE PROCEDURE dbo.sp_job_position_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        jp.id_job_position,
        jp.name,
        jp.description,
        COUNT(*) OVER() AS total_count
    FROM JobPositions jp
    WHERE jp.deleted_at IS NULL
      AND jp.status = 0
      AND (@search IS NULL OR @search = ''
           OR jp.name LIKE '%' + @search + '%'
           OR jp.description LIKE '%' + @search + '%')
    ORDER BY jp.id_job_position DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_job_position_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_job_position_get_by_id;
GO
CREATE PROCEDURE dbo.sp_job_position_get_by_id
    @id_job_position INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_job_position, name, description, status, created_at, updated_at
    FROM JobPositions
    WHERE id_job_position = @id_job_position AND deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_job_position_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_job_position_create;
GO
CREATE PROCEDURE dbo.sp_job_position_create
    @name        VARCHAR(100),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM JobPositions WHERE name = @name AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Ya existe un puesto de trabajo con ese nombre.' AS message, NULL AS id_job_position;
        RETURN;
    END
    INSERT INTO JobPositions (name, description) VALUES (@name, @description);
    SELECT 1 AS success, 'Puesto de trabajo creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_job_position;
END
GO

IF OBJECT_ID('dbo.sp_job_position_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_job_position_update;
GO
CREATE PROCEDURE dbo.sp_job_position_update
    @id_job_position INT,
    @name             VARCHAR(100),
    @description      VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM JobPositions WHERE id_job_position = @id_job_position AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Registro no encontrado.' AS message;
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM JobPositions WHERE name = @name AND id_job_position <> @id_job_position AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Ya existe otro puesto de trabajo con ese nombre.' AS message;
        RETURN;
    END
    UPDATE JobPositions SET name = @name, description = @description, updated_at = GETDATE()
    WHERE id_job_position = @id_job_position;
    SELECT 1 AS success, 'Puesto de trabajo actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_job_position_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_job_position_delete_logic;
GO
CREATE PROCEDURE dbo.sp_job_position_delete_logic
    @id_job_position INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM JobPositions WHERE id_job_position = @id_job_position AND status = 1 AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message;
        RETURN;
    END
    UPDATE JobPositions SET status = 0, updated_at = GETDATE() WHERE id_job_position = @id_job_position;
    SELECT 1 AS success, 'Puesto de trabajo desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_job_position_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_job_position_restore;
GO
CREATE PROCEDURE dbo.sp_job_position_restore
    @id_job_position INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM JobPositions WHERE id_job_position = @id_job_position AND status = 0 AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message;
        RETURN;
    END
    IF EXISTS (
        SELECT 1 FROM JobPositions
        WHERE name = (SELECT name FROM JobPositions WHERE id_job_position = @id_job_position)
          AND status = 1 AND deleted_at IS NULL
    )
    BEGIN
        SELECT 0 AS success, 'No se puede restaurar: ya existe un puesto activo con el mismo nombre.' AS message;
        RETURN;
    END
    UPDATE JobPositions SET status = 1, updated_at = GETDATE() WHERE id_job_position = @id_job_position;
    SELECT 1 AS success, 'Puesto de trabajo restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_job_position_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_job_position_delete_physical;
GO
CREATE PROCEDURE dbo.sp_job_position_delete_physical
    @id_job_position INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM JobPositions WHERE id_job_position = @id_job_position AND status = 0 AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message;
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM Employees WHERE id_job_position = @id_job_position)
    BEGIN
        SELECT 0 AS success, 'No se puede eliminar: el puesto de trabajo tiene empleados asociados.' AS message;
        RETURN;
    END
    BEGIN TRY
        DELETE FROM JobPositions WHERE id_job_position = @id_job_position;
        SELECT 1 AS success, 'Puesto de trabajo eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el registro tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- EMPLOYEES
-- ############################################################

IF OBJECT_ID('dbo.sp_employee_document_type_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_document_type_list_active;
GO
CREATE PROCEDURE dbo.sp_employee_document_type_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_document_type, name
    FROM DocumentTypes
    WHERE deleted_at IS NULL AND status = 1
    ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_employee_district_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_district_list_active;
GO
CREATE PROCEDURE dbo.sp_employee_district_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.id_district,
        c.name + ' / ' + r.name + ' / ' + p.name + ' / ' + d.name AS name
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

IF OBJECT_ID('dbo.sp_employee_job_position_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_job_position_list_active;
GO
CREATE PROCEDURE dbo.sp_employee_job_position_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_job_position, name
    FROM JobPositions
    WHERE deleted_at IS NULL AND status = 1
    ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_employee_user_list_available', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_user_list_available;
GO
CREATE PROCEDURE dbo.sp_employee_user_list_available
    @exclude_employee_id INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.id_user, u.username
    FROM Users u
    INNER JOIN Roles r ON r.id_role = u.id_role
    WHERE u.deleted_at IS NULL
      AND r.deleted_at IS NULL
      AND NOT EXISTS (
          SELECT 1 FROM Employees e
          WHERE e.id_user = u.id_user
            AND e.deleted_at IS NULL
            AND (@exclude_employee_id IS NULL OR e.id_employee <> @exclude_employee_id)
      )
    ORDER BY u.username;
END
GO

IF OBJECT_ID('dbo.sp_employee_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_list_active;
GO
CREATE PROCEDURE dbo.sp_employee_list_active
    @search           VARCHAR(100) = NULL,
    @id_document_type INT = NULL,
    @id_district      INT = NULL,
    @id_job_position  INT = NULL,
    @page             INT = 1,
    @page_size        INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        e.id_employee,
        u.username AS user_name,
        dt.name AS document_type_name,
        e.document_number,
        e.name,
        e.last_name_paternal,
        e.last_name_maternal,
        jp.name AS job_position_name,
        e.phone,
        e.email,
        ISNULL(d.name, '') AS district_name,
        COUNT(*) OVER() AS total_count
    FROM Employees e
    INNER JOIN Users u ON u.id_user = e.id_user
    INNER JOIN DocumentTypes dt ON dt.id_document_type = e.id_document_type
    INNER JOIN JobPositions jp ON jp.id_job_position = e.id_job_position
    LEFT JOIN Districts d ON d.id_district = e.id_district
    WHERE e.deleted_at IS NULL AND e.status = 1
      AND u.deleted_at IS NULL
      AND dt.deleted_at IS NULL
      AND jp.deleted_at IS NULL
      AND (@id_document_type IS NULL OR e.id_document_type = @id_document_type)
      AND (@id_district IS NULL OR e.id_district = @id_district)
      AND (@id_job_position IS NULL OR e.id_job_position = @id_job_position)
      AND (@search IS NULL OR @search = ''
           OR u.username LIKE '%' + @search + '%'
           OR e.document_number LIKE '%' + @search + '%'
           OR e.name LIKE '%' + @search + '%'
           OR e.last_name_paternal LIKE '%' + @search + '%'
           OR e.last_name_maternal LIKE '%' + @search + '%'
           OR e.email LIKE '%' + @search + '%'
           OR e.phone LIKE '%' + @search + '%'
           OR dt.name LIKE '%' + @search + '%'
           OR jp.name LIKE '%' + @search + '%'
           OR d.name LIKE '%' + @search + '%')
    ORDER BY e.id_employee DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_employee_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_list_inactive;
GO
CREATE PROCEDURE dbo.sp_employee_list_inactive
    @search           VARCHAR(100) = NULL,
    @id_document_type INT = NULL,
    @id_district      INT = NULL,
    @id_job_position  INT = NULL,
    @page             INT = 1,
    @page_size        INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        e.id_employee,
        u.username AS user_name,
        dt.name AS document_type_name,
        e.document_number,
        e.name,
        e.last_name_paternal,
        e.last_name_maternal,
        jp.name AS job_position_name,
        e.phone,
        e.email,
        ISNULL(d.name, '') AS district_name,
        COUNT(*) OVER() AS total_count
    FROM Employees e
    INNER JOIN Users u ON u.id_user = e.id_user
    INNER JOIN DocumentTypes dt ON dt.id_document_type = e.id_document_type
    INNER JOIN JobPositions jp ON jp.id_job_position = e.id_job_position
    LEFT JOIN Districts d ON d.id_district = e.id_district
    WHERE e.deleted_at IS NULL AND e.status = 0
      AND u.deleted_at IS NULL
      AND dt.deleted_at IS NULL
      AND jp.deleted_at IS NULL
      AND (@id_document_type IS NULL OR e.id_document_type = @id_document_type)
      AND (@id_district IS NULL OR e.id_district = @id_district)
      AND (@id_job_position IS NULL OR e.id_job_position = @id_job_position)
      AND (@search IS NULL OR @search = ''
           OR u.username LIKE '%' + @search + '%'
           OR e.document_number LIKE '%' + @search + '%'
           OR e.name LIKE '%' + @search + '%'
           OR e.last_name_paternal LIKE '%' + @search + '%'
           OR e.last_name_maternal LIKE '%' + @search + '%'
           OR e.email LIKE '%' + @search + '%'
           OR e.phone LIKE '%' + @search + '%'
           OR dt.name LIKE '%' + @search + '%'
           OR jp.name LIKE '%' + @search + '%'
           OR d.name LIKE '%' + @search + '%')
    ORDER BY e.id_employee DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_employee_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_get_by_id;
GO
CREATE PROCEDURE dbo.sp_employee_get_by_id
    @id_employee INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        e.id_employee,
        e.id_user,
        u.username,
        r.name AS role_name,
        e.id_job_position,
        jp.name AS job_position_name,
        e.id_document_type,
        dt.name AS document_type_name,
        e.document_number,
        e.name,
        e.last_name_paternal,
        e.last_name_maternal,
        e.phone,
        e.email,
        e.id_district,
        ISNULL(d.name, '') AS district_name,
        e.status,
        e.created_at,
        e.updated_at
    FROM Employees e
    INNER JOIN Users u ON u.id_user = e.id_user
    INNER JOIN Roles r ON r.id_role = u.id_role
    INNER JOIN DocumentTypes dt ON dt.id_document_type = e.id_document_type
    INNER JOIN JobPositions jp ON jp.id_job_position = e.id_job_position
    LEFT JOIN Districts d ON d.id_district = e.id_district
    WHERE e.id_employee = @id_employee AND e.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_employee_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_create;
GO
CREATE PROCEDURE dbo.sp_employee_create
    @id_user            INT,
    @id_job_position    INT,
    @id_document_type   INT,
    @document_number    VARCHAR(20),
    @name               VARCHAR(80),
    @last_name_paternal VARCHAR(80),
    @last_name_maternal VARCHAR(80) = NULL,
    @phone              VARCHAR(20) = NULL,
    @email              VARCHAR(100) = NULL,
    @id_district        INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE id_user = @id_user AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El usuario seleccionado no es válido.' AS message, NULL AS id_employee; RETURN; END

    IF EXISTS (
        SELECT 1 FROM Employees e
        WHERE e.id_user = @id_user AND e.deleted_at IS NULL AND e.status = 1
    )
    BEGIN SELECT 0 AS success, 'El usuario ya está asignado a un empleado activo.' AS message, NULL AS id_employee; RETURN; END

    IF EXISTS (SELECT 1 FROM Employees WHERE id_user = @id_user AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El usuario ya está asignado a otro empleado.' AS message, NULL AS id_employee; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM JobPositions WHERE id_job_position = @id_job_position AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El puesto de trabajo no es válido.' AS message, NULL AS id_employee; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El tipo de documento no es válido.' AS message, NULL AS id_employee; RETURN; END

    IF EXISTS (SELECT 1 FROM Employees WHERE document_number = @document_number AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un empleado con ese número de documento.' AS message, NULL AS id_employee; RETURN; END

    IF @email IS NOT NULL AND @email <> '' AND EXISTS (SELECT 1 FROM Employees WHERE email = @email AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un empleado con ese correo electrónico.' AS message, NULL AS id_employee; RETURN; END

    IF @id_district IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El distrito seleccionado no es válido.' AS message, NULL AS id_employee; RETURN; END

    INSERT INTO Employees (id_user, id_job_position, id_document_type, document_number, name, last_name_paternal, last_name_maternal, phone, email, id_district)
    VALUES (@id_user, @id_job_position, @id_document_type, @document_number, @name, @last_name_paternal, @last_name_maternal, @phone, @email, @id_district);

    SELECT 1 AS success, 'Empleado creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_employee;
END
GO

IF OBJECT_ID('dbo.sp_employee_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_update;
GO
CREATE PROCEDURE dbo.sp_employee_update
    @id_employee        INT,
    @id_user            INT,
    @id_job_position    INT,
    @id_document_type   INT,
    @document_number    VARCHAR(20),
    @name               VARCHAR(80),
    @last_name_paternal VARCHAR(80),
    @last_name_maternal VARCHAR(80) = NULL,
    @phone              VARCHAR(20) = NULL,
    @email              VARCHAR(100) = NULL,
    @id_district        INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Employees WHERE id_employee = @id_employee AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE id_user = @id_user AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El usuario seleccionado no es válido.' AS message; RETURN; END

    IF EXISTS (
        SELECT 1 FROM Employees e
        WHERE e.id_user = @id_user AND e.deleted_at IS NULL AND e.status = 1 AND e.id_employee <> @id_employee
    )
    BEGIN SELECT 0 AS success, 'El usuario ya está asignado a otro empleado activo.' AS message; RETURN; END

    IF EXISTS (SELECT 1 FROM Employees WHERE id_user = @id_user AND id_employee <> @id_employee AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El usuario ya está asignado a otro empleado.' AS message; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM JobPositions WHERE id_job_position = @id_job_position AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El puesto de trabajo no es válido.' AS message; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El tipo de documento no es válido.' AS message; RETURN; END

    IF EXISTS (SELECT 1 FROM Employees WHERE document_number = @document_number AND id_employee <> @id_employee AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro empleado con ese número de documento.' AS message; RETURN; END

    IF @email IS NOT NULL AND @email <> '' AND EXISTS (SELECT 1 FROM Employees WHERE email = @email AND id_employee <> @id_employee AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro empleado con ese correo electrónico.' AS message; RETURN; END

    IF @id_district IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El distrito seleccionado no es válido.' AS message; RETURN; END

    UPDATE Employees SET
        id_user = @id_user,
        id_job_position = @id_job_position,
        id_document_type = @id_document_type,
        document_number = @document_number,
        name = @name,
        last_name_paternal = @last_name_paternal,
        last_name_maternal = @last_name_maternal,
        phone = @phone,
        email = @email,
        id_district = @id_district,
        updated_at = GETDATE()
    WHERE id_employee = @id_employee;

    SELECT 1 AS success, 'Empleado actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_employee_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_delete_logic;
GO
CREATE PROCEDURE dbo.sp_employee_delete_logic
    @id_employee INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Employees WHERE id_employee = @id_employee AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message; RETURN; END
    UPDATE Employees SET status = 0, updated_at = GETDATE() WHERE id_employee = @id_employee;
    SELECT 1 AS success, 'Empleado desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_employee_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_restore;
GO
CREATE PROCEDURE dbo.sp_employee_restore
    @id_employee INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Employees WHERE id_employee = @id_employee AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message; RETURN; END

    IF EXISTS (
        SELECT 1 FROM Employees
        WHERE document_number = (SELECT document_number FROM Employees WHERE id_employee = @id_employee)
          AND status = 1 AND deleted_at IS NULL
    )
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe un empleado activo con el mismo documento.' AS message; RETURN; END

    IF EXISTS (
        SELECT 1 FROM Employees
        WHERE id_user = (SELECT id_user FROM Employees WHERE id_employee = @id_employee)
          AND status = 1 AND deleted_at IS NULL
    )
    BEGIN SELECT 0 AS success, 'No se puede restaurar: el usuario ya está asignado a otro empleado activo.' AS message; RETURN; END

    IF EXISTS (
        SELECT 1 FROM Employees e
        INNER JOIN Employees cur ON cur.id_employee = @id_employee
        WHERE e.email = cur.email AND e.email IS NOT NULL AND e.email <> ''
          AND e.status = 1 AND e.deleted_at IS NULL
    )
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe un empleado activo con el mismo correo.' AS message; RETURN; END

    UPDATE Employees SET status = 1, updated_at = GETDATE() WHERE id_employee = @id_employee;
    SELECT 1 AS success, 'Empleado restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_employee_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_employee_delete_physical;
GO
CREATE PROCEDURE dbo.sp_employee_delete_physical
    @id_employee INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Employees WHERE id_employee = @id_employee AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Employees WHERE id_employee = @id_employee;
        SELECT 1 AS success, 'Empleado eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el empleado tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- TRANSFERENCIAS
-- Módulos: StatusTransfers, Transfers, TransferDetails
-- Nomenclatura: sp_status_transfer_*, sp_transfer_*, sp_transfer_detail_*
-- ############################################################

USE KMLLogistics;
GO

-- Datos base para movimientos y estados
IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE name = N'Salida por transferencia' AND deleted_at IS NULL)
    INSERT INTO MovementTypes (name) VALUES (N'Salida por transferencia');
IF NOT EXISTS (SELECT 1 FROM MovementTypes WHERE name = N'Entrada por transferencia' AND deleted_at IS NULL)
    INSERT INTO MovementTypes (name) VALUES (N'Entrada por transferencia');
IF NOT EXISTS (SELECT 1 FROM StatusTransfers WHERE name = N'Completada' AND deleted_at IS NULL)
    INSERT INTO StatusTransfers (name) VALUES (N'Completada');
IF NOT EXISTS (SELECT 1 FROM StatusTransfers WHERE name = N'Cancelada' AND deleted_at IS NULL)
    INSERT INTO StatusTransfers (name) VALUES (N'Cancelada');
GO

-- ############################################################
-- STATUS TRANSFERS
-- ############################################################

IF OBJECT_ID('dbo.sp_status_transfer_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_status_transfer_list_active;
GO
CREATE PROCEDURE dbo.sp_status_transfer_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT st.id_status_transfer, st.name, COUNT(*) OVER() AS total_count
    FROM StatusTransfers st
    WHERE st.deleted_at IS NULL AND st.status = 1
      AND (@search IS NULL OR @search = '' OR st.name LIKE '%' + @search + '%')
    ORDER BY st.id_status_transfer DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_status_transfer_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_status_transfer_list_inactive;
GO
CREATE PROCEDURE dbo.sp_status_transfer_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT st.id_status_transfer, st.name, COUNT(*) OVER() AS total_count
    FROM StatusTransfers st
    WHERE st.deleted_at IS NULL AND st.status = 0
      AND (@search IS NULL OR @search = '' OR st.name LIKE '%' + @search + '%')
    ORDER BY st.id_status_transfer DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_status_transfer_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_status_transfer_get_by_id;
GO
CREATE PROCEDURE dbo.sp_status_transfer_get_by_id @id_status_transfer INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_status_transfer, name, status, created_at, updated_at
    FROM StatusTransfers
    WHERE id_status_transfer = @id_status_transfer AND deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_status_transfer_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_status_transfer_create;
GO
CREATE PROCEDURE dbo.sp_status_transfer_create @name VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM StatusTransfers WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Ya existe un estado con ese nombre.' AS message, NULL AS id_status_transfer; RETURN; END
    INSERT INTO StatusTransfers (name) VALUES (@name);
    SELECT 1 AS success, N'Estado creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_status_transfer;
END
GO

IF OBJECT_ID('dbo.sp_status_transfer_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_status_transfer_update;
GO
CREATE PROCEDURE dbo.sp_status_transfer_update
    @id_status_transfer INT,
    @name               VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM StatusTransfers WHERE id_status_transfer = @id_status_transfer AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM StatusTransfers WHERE name = @name AND id_status_transfer <> @id_status_transfer AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Ya existe otro estado con ese nombre.' AS message; RETURN; END
    UPDATE StatusTransfers SET name = @name, updated_at = GETDATE() WHERE id_status_transfer = @id_status_transfer;
    SELECT 1 AS success, N'Estado actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_status_transfer_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_status_transfer_delete_logic;
GO
CREATE PROCEDURE dbo.sp_status_transfer_delete_logic @id_status_transfer INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM StatusTransfers WHERE id_status_transfer = @id_status_transfer AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado o ya inactivo.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Transfers WHERE id_status_transfer = @id_status_transfer AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'No se puede desactivar: el estado está en uso en transferencias.' AS message; RETURN; END
    UPDATE StatusTransfers SET status = 0, updated_at = GETDATE() WHERE id_status_transfer = @id_status_transfer;
    SELECT 1 AS success, N'Estado desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_status_transfer_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_status_transfer_restore;
GO
CREATE PROCEDURE dbo.sp_status_transfer_restore @id_status_transfer INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM StatusTransfers WHERE id_status_transfer = @id_status_transfer AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Registro no encontrado o ya activo.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM StatusTransfers WHERE name = (SELECT name FROM StatusTransfers WHERE id_status_transfer = @id_status_transfer) AND status = 1 AND deleted_at IS NULL AND id_status_transfer <> @id_status_transfer)
    BEGIN SELECT 0 AS success, N'Ya existe un estado activo con ese nombre.' AS message; RETURN; END
    UPDATE StatusTransfers SET status = 1, updated_at = GETDATE() WHERE id_status_transfer = @id_status_transfer;
    SELECT 1 AS success, N'Estado restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_status_transfer_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_status_transfer_delete_physical;
GO
CREATE PROCEDURE dbo.sp_status_transfer_delete_physical @id_status_transfer INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM StatusTransfers WHERE id_status_transfer = @id_status_transfer AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Transfers WHERE id_status_transfer = @id_status_transfer)
    BEGIN SELECT 0 AS success, N'No se puede eliminar: el estado tiene transferencias asociadas.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM StatusTransfers WHERE id_status_transfer = @id_status_transfer;
        SELECT 1 AS success, N'Estado eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, N'No se puede eliminar: el estado tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- TRANSFERS - Lookups
-- ############################################################

IF OBJECT_ID('dbo.sp_transfer_warehouse_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_warehouse_list_active;
GO
CREATE PROCEDURE dbo.sp_transfer_warehouse_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_warehouse, name FROM Warehouses WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_transfer_employee_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_employee_list_active;
GO
CREATE PROCEDURE dbo.sp_transfer_employee_list_active
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

IF OBJECT_ID('dbo.sp_transfer_status_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_status_list_active;
GO
CREATE PROCEDURE dbo.sp_transfer_status_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_status_transfer, name FROM StatusTransfers WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_transfer_product_list_by_warehouse', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_product_list_by_warehouse;
GO
CREATE PROCEDURE dbo.sp_transfer_product_list_by_warehouse @id_warehouse INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.id_product, p.name, wd.stock
    FROM WarehouseDetails wd
    INNER JOIN Products p ON p.id_product = wd.id_product
    WHERE wd.id_warehouse = @id_warehouse
      AND wd.stock > 0
      AND p.deleted_at IS NULL
      AND p.status = 1
    ORDER BY p.name;
END
GO

-- ############################################################
-- TRANSFERS - CRUD
-- ############################################################

IF OBJECT_ID('dbo.sp_transfer_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_list;
GO
CREATE PROCEDURE dbo.sp_transfer_list
    @search                  VARCHAR(100) = NULL,
    @id_warehouse_origin     INT = NULL,
    @id_warehouse_destination INT = NULL,
    @id_status_transfer      INT = NULL,
    @id_employee             INT = NULL,
    @page                    INT = 1,
    @page_size               INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        t.id_transfer,
        t.fec_transfer,
        wo.name AS warehouse_origin_name,
        wd.name AS warehouse_destination_name,
        st.name AS status_transfer_name,
        u.username AS employee_name,
        COUNT(*) OVER() AS total_count
    FROM Transfers t
    INNER JOIN Warehouses wo ON wo.id_warehouse = t.id_warehouse_origin
    INNER JOIN Warehouses wd ON wd.id_warehouse = t.id_warehouse_destination
    INNER JOIN StatusTransfers st ON st.id_status_transfer = t.id_status_transfer
    INNER JOIN Employees e ON e.id_employee = t.id_employee
    INNER JOIN Users u ON u.id_user = e.id_user
    WHERE t.deleted_at IS NULL
      AND (@id_warehouse_origin IS NULL OR t.id_warehouse_origin = @id_warehouse_origin)
      AND (@id_warehouse_destination IS NULL OR t.id_warehouse_destination = @id_warehouse_destination)
      AND (@id_status_transfer IS NULL OR t.id_status_transfer = @id_status_transfer)
      AND (@id_employee IS NULL OR t.id_employee = @id_employee)
      AND (@search IS NULL OR @search = ''
           OR CAST(t.id_transfer AS VARCHAR(20)) LIKE '%' + @search + '%'
           OR wo.name LIKE '%' + @search + '%'
           OR wd.name LIKE '%' + @search + '%'
           OR st.name LIKE '%' + @search + '%'
           OR u.username LIKE '%' + @search + '%')
    ORDER BY t.id_transfer DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_transfer_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_get_by_id;
GO
CREATE PROCEDURE dbo.sp_transfer_get_by_id @id_transfer INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.id_transfer,
        t.id_warehouse_origin,
        wo.name AS warehouse_origin_name,
        t.id_warehouse_destination,
        wd.name AS warehouse_destination_name,
        t.id_status_transfer,
        st.name AS status_transfer_name,
        t.fec_transfer,
        t.id_employee,
        u.username AS employee_username,
        e.name + N' ' + e.last_name_paternal AS employee_name,
        t.created_at,
        t.updated_at
    FROM Transfers t
    INNER JOIN Warehouses wo ON wo.id_warehouse = t.id_warehouse_origin
    INNER JOIN Warehouses wd ON wd.id_warehouse = t.id_warehouse_destination
    INNER JOIN StatusTransfers st ON st.id_status_transfer = t.id_status_transfer
    INNER JOIN Employees e ON e.id_employee = t.id_employee
    INNER JOIN Users u ON u.id_user = e.id_user
    WHERE t.id_transfer = @id_transfer AND t.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_transfer_detail_lines_by_transfer', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_detail_lines_by_transfer;
GO
CREATE PROCEDURE dbo.sp_transfer_detail_lines_by_transfer @id_transfer INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT td.id_transfer_detail, td.id_product, p.name AS product_name, td.quantity
    FROM TransferDetails td
    INNER JOIN Products p ON p.id_product = td.id_product
    WHERE td.id_transfer = @id_transfer
    ORDER BY td.id_transfer_detail;
END
GO

IF OBJECT_ID('dbo.sp_transfer_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_create;
GO
CREATE PROCEDURE dbo.sp_transfer_create
    @id_warehouse_origin      INT,
    @id_warehouse_destination INT,
    @id_employee              INT,
    @fec_transfer             DATETIME,
    @details_json             NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_status_transfer INT;
    DECLARE @id_movement_out INT;
    DECLARE @id_movement_in INT;
    DECLARE @id_transfer INT;
    DECLARE @id_product INT;
    DECLARE @quantity INT;
    DECLARE @stock INT;
    DECLARE @line_count INT;

    IF @id_warehouse_origin = @id_warehouse_destination
    BEGIN SELECT 0 AS success, N'El almacén de origen y destino deben ser diferentes.' AS message, NULL AS id_transfer; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE id_warehouse = @id_warehouse_origin AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS success, N'Almacén de origen no válido.' AS message, NULL AS id_transfer; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE id_warehouse = @id_warehouse_destination AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS success, N'Almacén de destino no válido.' AS message, NULL AS id_transfer; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Employees WHERE id_employee = @id_employee AND deleted_at IS NULL AND status = 1)
    BEGIN SELECT 0 AS success, N'Empleado no válido.' AS message, NULL AS id_transfer; RETURN; END

    IF @details_json IS NULL OR LTRIM(RTRIM(@details_json)) = '' OR @details_json = '[]'
    BEGIN SELECT 0 AS success, N'Debe agregar al menos un producto a la transferencia.' AS message, NULL AS id_transfer; RETURN; END

    SELECT @id_status_transfer = id_status_transfer FROM StatusTransfers WHERE name = N'Completada' AND deleted_at IS NULL AND status = 1;
    IF @id_status_transfer IS NULL
    BEGIN SELECT 0 AS success, N'No existe el estado Completada. Ejecute el script de transferencias.' AS message, NULL AS id_transfer; RETURN; END

    SELECT @id_movement_out = id_movement_type FROM MovementTypes WHERE name = N'Salida por transferencia' AND deleted_at IS NULL AND status = 1;
    SELECT @id_movement_in = id_movement_type FROM MovementTypes WHERE name = N'Entrada por transferencia' AND deleted_at IS NULL AND status = 1;
    IF @id_movement_out IS NULL OR @id_movement_in IS NULL
    BEGIN SELECT 0 AS success, N'No existen los tipos de movimiento de transferencia.' AS message, NULL AS id_transfer; RETURN; END

    IF EXISTS (
        SELECT id_product FROM OPENJSON(@details_json) WITH (id_product INT '$.idProduct', quantity INT '$.quantity')
        WHERE id_product IS NULL OR quantity IS NULL OR quantity <= 0
    )
    BEGIN SELECT 0 AS success, N'Todas las líneas deben tener producto y cantidad mayor a cero.' AS message, NULL AS id_transfer; RETURN; END

    IF EXISTS (
        SELECT id_product FROM OPENJSON(@details_json) WITH (id_product INT '$.idProduct', quantity INT '$.quantity')
        GROUP BY id_product HAVING COUNT(*) > 1
    )
    BEGIN SELECT 0 AS success, N'No repita el mismo producto en la transferencia.' AS message, NULL AS id_transfer; RETURN; END

    IF EXISTS (
        SELECT 1 FROM OPENJSON(@details_json) WITH (id_product INT '$.idProduct') j
        LEFT JOIN Products p ON p.id_product = j.id_product AND p.deleted_at IS NULL AND p.status = 1
        WHERE p.id_product IS NULL
    )
    BEGIN SELECT 0 AS success, N'Uno o más productos no son válidos.' AS message, NULL AS id_transfer; RETURN; END

    IF EXISTS (
        SELECT 1 FROM OPENJSON(@details_json) WITH (id_product INT '$.idProduct', quantity INT '$.quantity') j
        LEFT JOIN WarehouseDetails wd ON wd.id_warehouse = @id_warehouse_origin AND wd.id_product = j.id_product
        WHERE wd.id_warehouse_detail IS NULL OR wd.stock < j.quantity
    )
    BEGIN SELECT 0 AS success, N'Stock insuficiente en el almacén de origen para uno o más productos.' AS message, NULL AS id_transfer; RETURN; END

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Transfers (id_warehouse_origin, id_warehouse_destination, id_status_transfer, fec_transfer, id_employee)
        VALUES (@id_warehouse_origin, @id_warehouse_destination, @id_status_transfer, @fec_transfer, @id_employee);
        SET @id_transfer = SCOPE_IDENTITY();

        INSERT INTO TransferDetails (id_transfer, id_product, quantity)
        SELECT @id_transfer, j.id_product, j.quantity
        FROM OPENJSON(@details_json) WITH (id_product INT '$.idProduct', quantity INT '$.quantity') j;

        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
            SELECT id_product, quantity FROM OPENJSON(@details_json) WITH (id_product INT '$.idProduct', quantity INT '$.quantity');

        OPEN cur;
        FETCH NEXT FROM cur INTO @id_product, @quantity;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            UPDATE WarehouseDetails SET stock = stock - @quantity
            WHERE id_warehouse = @id_warehouse_origin AND id_product = @id_product;

            IF EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_warehouse = @id_warehouse_destination AND id_product = @id_product)
                UPDATE WarehouseDetails SET stock = stock + @quantity
                WHERE id_warehouse = @id_warehouse_destination AND id_product = @id_product;
            ELSE
                INSERT INTO WarehouseDetails (id_warehouse, id_product, stock) VALUES (@id_warehouse_destination, @id_product, @quantity);

            INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
            VALUES (@id_product, @id_warehouse_origin, @id_movement_out, @id_employee, @quantity, N'TRF-' + CAST(@id_transfer AS NVARCHAR(20)), @fec_transfer);

            INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
            VALUES (@id_product, @id_warehouse_destination, @id_movement_in, @id_employee, @quantity, N'TRF-' + CAST(@id_transfer AS NVARCHAR(20)), @fec_transfer);

            FETCH NEXT FROM cur INTO @id_product, @quantity;
        END
        CLOSE cur;
        DEALLOCATE cur;

        COMMIT TRANSACTION;
        SELECT 1 AS success, N'Transferencia registrada correctamente.' AS message, @id_transfer AS id_transfer;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        IF CURSOR_STATUS('local', 'cur') >= 0 BEGIN CLOSE cur; DEALLOCATE cur; END
        SELECT 0 AS success, N'Error al registrar la transferencia: ' + ERROR_MESSAGE() AS message, NULL AS id_transfer;
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_transfer_cancel', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_cancel;
GO
CREATE PROCEDURE dbo.sp_transfer_cancel @id_transfer INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_status_cancel INT;
    DECLARE @id_status_current INT;
    DECLARE @status_name NVARCHAR(50);
    DECLARE @id_warehouse_origin INT;
    DECLARE @id_warehouse_destination INT;
    DECLARE @id_employee INT;
    DECLARE @fec_transfer DATETIME;
    DECLARE @id_movement_out INT;
    DECLARE @id_movement_in INT;
    DECLARE @id_product INT;
    DECLARE @quantity INT;
    DECLARE @stock INT;

    IF NOT EXISTS (SELECT 1 FROM Transfers WHERE id_transfer = @id_transfer AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, N'Transferencia no encontrada.' AS message; RETURN; END

    SELECT @id_status_current = t.id_status_transfer, @status_name = st.name,
           @id_warehouse_origin = t.id_warehouse_origin, @id_warehouse_destination = t.id_warehouse_destination,
           @id_employee = t.id_employee, @fec_transfer = t.fec_transfer
    FROM Transfers t
    INNER JOIN StatusTransfers st ON st.id_status_transfer = t.id_status_transfer
    WHERE t.id_transfer = @id_transfer;

    IF @status_name = N'Cancelada'
    BEGIN SELECT 0 AS success, N'La transferencia ya está cancelada.' AS message; RETURN; END

    SELECT @id_status_cancel = id_status_transfer FROM StatusTransfers WHERE name = N'Cancelada' AND deleted_at IS NULL AND status = 1;
    IF @id_status_cancel IS NULL
    BEGIN SELECT 0 AS success, N'No existe el estado Cancelada.' AS message; RETURN; END

    SELECT @id_movement_out = id_movement_type FROM MovementTypes WHERE name = N'Salida por transferencia' AND deleted_at IS NULL AND status = 1;
    SELECT @id_movement_in = id_movement_type FROM MovementTypes WHERE name = N'Entrada por transferencia' AND deleted_at IS NULL AND status = 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @status_name = N'Completada'
        BEGIN
            DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
                SELECT id_product, quantity FROM TransferDetails WHERE id_transfer = @id_transfer;

            OPEN cur;
            FETCH NEXT FROM cur INTO @id_product, @quantity;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SELECT @stock = stock FROM WarehouseDetails WHERE id_warehouse = @id_warehouse_destination AND id_product = @id_product;
                IF @stock IS NULL OR @stock < @quantity
                BEGIN
                    ROLLBACK TRANSACTION;
                    CLOSE cur; DEALLOCATE cur;
                    SELECT 0 AS success, N'No hay stock suficiente en destino para revertir la transferencia.' AS message;
                    RETURN;
                END

                UPDATE WarehouseDetails SET stock = stock - @quantity
                WHERE id_warehouse = @id_warehouse_destination AND id_product = @id_product;

                IF EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_warehouse = @id_warehouse_origin AND id_product = @id_product)
                    UPDATE WarehouseDetails SET stock = stock + @quantity
                    WHERE id_warehouse = @id_warehouse_origin AND id_product = @id_product;
                ELSE
                    INSERT INTO WarehouseDetails (id_warehouse, id_product, stock) VALUES (@id_warehouse_origin, @id_product, @quantity);

                INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
                VALUES (@id_product, @id_warehouse_destination, @id_movement_out, @id_employee, @quantity, N'TRF-CAN-' + CAST(@id_transfer AS NVARCHAR(20)), GETDATE());

                INSERT INTO InventoryMovements (id_product, id_warehouse, id_movement_type, id_employee, quantity, reference, fec_movement)
                VALUES (@id_product, @id_warehouse_origin, @id_movement_in, @id_employee, @quantity, N'TRF-CAN-' + CAST(@id_transfer AS NVARCHAR(20)), GETDATE());

                FETCH NEXT FROM cur INTO @id_product, @quantity;
            END
            CLOSE cur;
            DEALLOCATE cur;
        END

        UPDATE Transfers SET id_status_transfer = @id_status_cancel, updated_at = GETDATE() WHERE id_transfer = @id_transfer;

        COMMIT TRANSACTION;
        SELECT 1 AS success, N'Transferencia cancelada correctamente. El stock fue revertido.' AS message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS success, N'Error al cancelar: ' + ERROR_MESSAGE() AS message;
    END CATCH
END
GO

-- ############################################################
-- TRANSFER DETAILS - Consulta
-- ############################################################

IF OBJECT_ID('dbo.sp_transfer_detail_list', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_detail_list;
GO
CREATE PROCEDURE dbo.sp_transfer_detail_list
    @search                  VARCHAR(100) = NULL,
    @id_transfer             INT = NULL,
    @id_product              INT = NULL,
    @id_warehouse_origin     INT = NULL,
    @id_warehouse_destination INT = NULL,
    @id_status_transfer      INT = NULL,
    @page                    INT = 1,
    @page_size               INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        td.id_transfer_detail,
        td.id_transfer,
        p.name AS product_name,
        td.quantity,
        wo.name AS warehouse_origin_name,
        wd.name AS warehouse_destination_name,
        st.name AS status_transfer_name,
        t.fec_transfer,
        COUNT(*) OVER() AS total_count
    FROM TransferDetails td
    INNER JOIN Transfers t ON t.id_transfer = td.id_transfer
    INNER JOIN Products p ON p.id_product = td.id_product
    INNER JOIN Warehouses wo ON wo.id_warehouse = t.id_warehouse_origin
    INNER JOIN Warehouses wd ON wd.id_warehouse = t.id_warehouse_destination
    INNER JOIN StatusTransfers st ON st.id_status_transfer = t.id_status_transfer
    WHERE t.deleted_at IS NULL
      AND (@id_transfer IS NULL OR td.id_transfer = @id_transfer)
      AND (@id_product IS NULL OR td.id_product = @id_product)
      AND (@id_warehouse_origin IS NULL OR t.id_warehouse_origin = @id_warehouse_origin)
      AND (@id_warehouse_destination IS NULL OR t.id_warehouse_destination = @id_warehouse_destination)
      AND (@id_status_transfer IS NULL OR t.id_status_transfer = @id_status_transfer)
      AND (@search IS NULL OR @search = ''
           OR CAST(td.id_transfer_detail AS VARCHAR(20)) LIKE '%' + @search + '%'
           OR p.name LIKE '%' + @search + '%'
           OR wo.name LIKE '%' + @search + '%'
           OR wd.name LIKE '%' + @search + '%'
           OR st.name LIKE '%' + @search + '%')
    ORDER BY td.id_transfer_detail DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_transfer_detail_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_detail_get_by_id;
GO
CREATE PROCEDURE dbo.sp_transfer_detail_get_by_id @id_transfer_detail INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        td.id_transfer_detail,
        td.id_transfer,
        td.id_product,
        p.name AS product_name,
        td.quantity,
        t.id_warehouse_origin,
        wo.name AS warehouse_origin_name,
        t.id_warehouse_destination,
        wd.name AS warehouse_destination_name,
        t.id_status_transfer,
        st.name AS status_transfer_name,
        t.fec_transfer,
        t.id_employee,
        u.username AS employee_username,
        e.name + N' ' + e.last_name_paternal AS employee_name,
        t.created_at AS transfer_created_at
    FROM TransferDetails td
    INNER JOIN Transfers t ON t.id_transfer = td.id_transfer
    INNER JOIN Products p ON p.id_product = td.id_product
    INNER JOIN Warehouses wo ON wo.id_warehouse = t.id_warehouse_origin
    INNER JOIN Warehouses wd ON wd.id_warehouse = t.id_warehouse_destination
    INNER JOIN StatusTransfers st ON st.id_status_transfer = t.id_status_transfer
    INNER JOIN Employees e ON e.id_employee = t.id_employee
    INNER JOIN Users u ON u.id_user = e.id_user
    WHERE td.id_transfer_detail = @id_transfer_detail AND t.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_transfer_detail_filter_options', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_transfer_detail_filter_options;
GO
CREATE PROCEDURE dbo.sp_transfer_detail_filter_options
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_product, name FROM Products WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
    SELECT id_warehouse, name FROM Warehouses WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
    SELECT id_status_transfer, name FROM StatusTransfers WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO
