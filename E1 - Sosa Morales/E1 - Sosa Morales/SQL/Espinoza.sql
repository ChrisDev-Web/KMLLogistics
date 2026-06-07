-- ============================================================
-- KMLLogistics - Stored Procedures: Configuración (CRUD)
-- Módulos: DocumentTypes, Countries, Regions, Provinces, Districts
-- Nomenclatura: sp_{tabla_singular}_{funcion}_{campo_opcional}
-- ============================================================

USE KMLLogistics;
GO

-- ############################################################
-- DOCUMENT TYPES
-- ############################################################

IF OBJECT_ID('dbo.sp_document_type_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_document_type_list_active;
GO
CREATE PROCEDURE dbo.sp_document_type_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        dt.id_document_type,
        dt.name,
        dt.description,
        dt.created_at,
        COUNT(*) OVER() AS total_count
    FROM DocumentTypes dt
    WHERE dt.deleted_at IS NULL
      AND dt.status = 1
      AND (@search IS NULL OR @search = ''
           OR dt.name LIKE '%' + @search + '%'
           OR dt.description LIKE '%' + @search + '%')
    ORDER BY dt.id_document_type DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_document_type_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_document_type_list_inactive;
GO
CREATE PROCEDURE dbo.sp_document_type_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        dt.id_document_type,
        dt.name,
        dt.description,
        dt.updated_at,
        COUNT(*) OVER() AS total_count
    FROM DocumentTypes dt
    WHERE dt.deleted_at IS NULL
      AND dt.status = 0
      AND (@search IS NULL OR @search = ''
           OR dt.name LIKE '%' + @search + '%'
           OR dt.description LIKE '%' + @search + '%')
    ORDER BY dt.id_document_type DESC
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_document_type_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_document_type_get_by_id;
GO
CREATE PROCEDURE dbo.sp_document_type_get_by_id
    @id_document_type INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_document_type, name, description, status, created_at, updated_at
    FROM DocumentTypes
    WHERE id_document_type = @id_document_type AND deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_document_type_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_document_type_create;
GO
CREATE PROCEDURE dbo.sp_document_type_create
    @name        VARCHAR(50),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM DocumentTypes WHERE name = @name AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Ya existe un tipo de documento con ese nombre.' AS message, NULL AS id_document_type;
        RETURN;
    END
    INSERT INTO DocumentTypes (name, description) VALUES (@name, @description);
    SELECT 1 AS success, 'Tipo de documento creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_document_type;
END
GO

IF OBJECT_ID('dbo.sp_document_type_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_document_type_update;
GO
CREATE PROCEDURE dbo.sp_document_type_update
    @id_document_type INT,
    @name             VARCHAR(50),
    @description      VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Registro no encontrado.' AS message;
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM DocumentTypes WHERE name = @name AND id_document_type <> @id_document_type AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Ya existe otro tipo de documento con ese nombre.' AS message;
        RETURN;
    END
    UPDATE DocumentTypes SET name = @name, description = @description, updated_at = GETDATE()
    WHERE id_document_type = @id_document_type;
    SELECT 1 AS success, 'Tipo de documento actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_document_type_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_document_type_delete_logic;
GO
CREATE PROCEDURE dbo.sp_document_type_delete_logic
    @id_document_type INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 1 AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message;
        RETURN;
    END
    UPDATE DocumentTypes SET status = 0, updated_at = GETDATE() WHERE id_document_type = @id_document_type;
    SELECT 1 AS success, 'Tipo de documento desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_document_type_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_document_type_restore;
GO
CREATE PROCEDURE dbo.sp_document_type_restore
    @id_document_type INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 0 AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message;
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM DocumentTypes WHERE name = (SELECT name FROM DocumentTypes WHERE id_document_type = @id_document_type) AND status = 1 AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'No se puede restaurar: ya existe un registro activo con el mismo nombre.' AS message;
        RETURN;
    END
    UPDATE DocumentTypes SET status = 1, updated_at = GETDATE() WHERE id_document_type = @id_document_type;
    SELECT 1 AS success, 'Tipo de documento restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_document_type_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_document_type_delete_physical;
GO
CREATE PROCEDURE dbo.sp_document_type_delete_physical
    @id_document_type INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE id_document_type = @id_document_type AND status = 0 AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message;
        RETURN;
    END
    BEGIN TRY
        DELETE FROM DocumentTypes WHERE id_document_type = @id_document_type;
        SELECT 1 AS success, 'Tipo de documento eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el registro tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- COUNTRIES
-- ############################################################

IF OBJECT_ID('dbo.sp_country_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_country_list_active;
GO
CREATE PROCEDURE dbo.sp_country_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT c.id_country, c.name, c.created_at, COUNT(*) OVER() AS total_count
    FROM Countries c
    WHERE c.deleted_at IS NULL AND c.status = 1
      AND (@search IS NULL OR @search = '' OR c.name LIKE '%' + @search + '%')
    ORDER BY c.id_country DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_country_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_country_list_inactive;
GO
CREATE PROCEDURE dbo.sp_country_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT c.id_country, c.name, c.updated_at, COUNT(*) OVER() AS total_count
    FROM Countries c
    WHERE c.deleted_at IS NULL AND c.status = 0
      AND (@search IS NULL OR @search = '' OR c.name LIKE '%' + @search + '%')
    ORDER BY c.id_country DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_country_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_country_get_by_id;
GO
CREATE PROCEDURE dbo.sp_country_get_by_id @id_country INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_country, name, status, created_at, updated_at FROM Countries
    WHERE id_country = @id_country AND deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_country_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_country_create;
GO
CREATE PROCEDURE dbo.sp_country_create @name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Countries WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un país con ese nombre.' AS message, NULL AS id_country; RETURN; END
    INSERT INTO Countries (name) VALUES (@name);
    SELECT 1 AS success, 'País creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_country;
END
GO

IF OBJECT_ID('dbo.sp_country_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_country_update;
GO
CREATE PROCEDURE dbo.sp_country_update @id_country INT, @name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Countries WHERE id_country = @id_country AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Countries WHERE name = @name AND id_country <> @id_country AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro país con ese nombre.' AS message; RETURN; END
    UPDATE Countries SET name = @name, updated_at = GETDATE() WHERE id_country = @id_country;
    SELECT 1 AS success, 'País actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_country_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_country_delete_logic;
GO
CREATE PROCEDURE dbo.sp_country_delete_logic @id_country INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Countries WHERE id_country = @id_country AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message; RETURN; END
    UPDATE Countries SET status = 0, updated_at = GETDATE() WHERE id_country = @id_country;
    SELECT 1 AS success, 'País desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_country_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_country_restore;
GO
CREATE PROCEDURE dbo.sp_country_restore @id_country INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Countries WHERE id_country = @id_country AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Countries WHERE name = (SELECT name FROM Countries WHERE id_country = @id_country) AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe un país activo con el mismo nombre.' AS message; RETURN; END
    UPDATE Countries SET status = 1, updated_at = GETDATE() WHERE id_country = @id_country;
    SELECT 1 AS success, 'País restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_country_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_country_delete_physical;
GO
CREATE PROCEDURE dbo.sp_country_delete_physical @id_country INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Countries WHERE id_country = @id_country AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Countries WHERE id_country = @id_country;
        SELECT 1 AS success, 'País eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el país tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- REGIONS
-- ############################################################

IF OBJECT_ID('dbo.sp_region_country_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_country_list_active;
GO
CREATE PROCEDURE dbo.sp_region_country_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_country, name FROM Countries WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_region_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_list_active;
GO
CREATE PROCEDURE dbo.sp_region_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT r.id_region, r.id_country, c.name AS country_name, r.name, r.created_at, COUNT(*) OVER() AS total_count
    FROM Regions r INNER JOIN Countries c ON c.id_country = r.id_country
    WHERE r.deleted_at IS NULL AND r.status = 1 AND c.deleted_at IS NULL
      AND (@search IS NULL OR @search = '' OR r.name LIKE '%' + @search + '%' OR c.name LIKE '%' + @search + '%')
    ORDER BY r.id_region DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_region_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_list_inactive;
GO
CREATE PROCEDURE dbo.sp_region_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT r.id_region, r.id_country, c.name AS country_name, r.name, r.updated_at, COUNT(*) OVER() AS total_count
    FROM Regions r INNER JOIN Countries c ON c.id_country = r.id_country
    WHERE r.deleted_at IS NULL AND r.status = 0
      AND (@search IS NULL OR @search = '' OR r.name LIKE '%' + @search + '%' OR c.name LIKE '%' + @search + '%')
    ORDER BY r.id_region DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_region_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_get_by_id;
GO
CREATE PROCEDURE dbo.sp_region_get_by_id @id_region INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.id_region, r.id_country, c.name AS country_name, r.name, r.status, r.created_at, r.updated_at
    FROM Regions r INNER JOIN Countries c ON c.id_country = r.id_country
    WHERE r.id_region = @id_region AND r.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_region_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_create;
GO
CREATE PROCEDURE dbo.sp_region_create @id_country INT, @name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Countries WHERE id_country = @id_country AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El país seleccionado no es válido.' AS message, NULL AS id_region; RETURN; END
    IF EXISTS (SELECT 1 FROM Regions WHERE id_country = @id_country AND name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe una región con ese nombre en el país.' AS message, NULL AS id_region; RETURN; END
    INSERT INTO Regions (id_country, name) VALUES (@id_country, @name);
    SELECT 1 AS success, 'Región creada correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_region;
END
GO

IF OBJECT_ID('dbo.sp_region_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_update;
GO
CREATE PROCEDURE dbo.sp_region_update @id_region INT, @id_country INT, @name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Regions WHERE id_region = @id_region AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Regions WHERE id_country = @id_country AND name = @name AND id_region <> @id_region AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otra región con ese nombre en el país.' AS message; RETURN; END
    UPDATE Regions SET id_country = @id_country, name = @name, updated_at = GETDATE() WHERE id_region = @id_region;
    SELECT 1 AS success, 'Región actualizada correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_region_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_delete_logic;
GO
CREATE PROCEDURE dbo.sp_region_delete_logic @id_region INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Regions WHERE id_region = @id_region AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message; RETURN; END
    UPDATE Regions SET status = 0, updated_at = GETDATE() WHERE id_region = @id_region;
    SELECT 1 AS success, 'Región desactivada correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_region_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_restore;
GO
CREATE PROCEDURE dbo.sp_region_restore @id_region INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Regions WHERE id_region = @id_region AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message; RETURN; END
    DECLARE @cid INT, @n VARCHAR(100);
    SELECT @cid = id_country, @n = name FROM Regions WHERE id_region = @id_region;
    IF EXISTS (SELECT 1 FROM Regions WHERE id_country = @cid AND name = @n AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe una región activa con el mismo nombre.' AS message; RETURN; END
    UPDATE Regions SET status = 1, updated_at = GETDATE() WHERE id_region = @id_region;
    SELECT 1 AS success, 'Región restaurada correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_region_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_region_delete_physical;
GO
CREATE PROCEDURE dbo.sp_region_delete_physical @id_region INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Regions WHERE id_region = @id_region AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Regions WHERE id_region = @id_region;
        SELECT 1 AS success, 'Región eliminada permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: la región tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- PROVINCES
-- ############################################################

IF OBJECT_ID('dbo.sp_province_region_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_region_list_active;
GO
CREATE PROCEDURE dbo.sp_province_region_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.id_region, c.name + ' - ' + r.name AS name
    FROM Regions r INNER JOIN Countries c ON c.id_country = r.id_country
    WHERE r.deleted_at IS NULL AND r.status = 1 AND c.deleted_at IS NULL AND c.status = 1
    ORDER BY c.name, r.name;
END
GO

IF OBJECT_ID('dbo.sp_province_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_list_active;
GO
CREATE PROCEDURE dbo.sp_province_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT p.id_province, p.id_region, r.name AS region_name, p.name, p.created_at, COUNT(*) OVER() AS total_count
    FROM Provinces p INNER JOIN Regions r ON r.id_region = p.id_region
    WHERE p.deleted_at IS NULL AND p.status = 1 AND r.deleted_at IS NULL
      AND (@search IS NULL OR @search = '' OR p.name LIKE '%' + @search + '%' OR r.name LIKE '%' + @search + '%')
    ORDER BY p.id_province DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_province_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_list_inactive;
GO
CREATE PROCEDURE dbo.sp_province_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT p.id_province, p.id_region, r.name AS region_name, p.name, p.updated_at, COUNT(*) OVER() AS total_count
    FROM Provinces p INNER JOIN Regions r ON r.id_region = p.id_region
    WHERE p.deleted_at IS NULL AND p.status = 0
      AND (@search IS NULL OR @search = '' OR p.name LIKE '%' + @search + '%' OR r.name LIKE '%' + @search + '%')
    ORDER BY p.id_province DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_province_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_get_by_id;
GO
CREATE PROCEDURE dbo.sp_province_get_by_id @id_province INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.id_province, p.id_region, r.name AS region_name, p.name, p.status, p.created_at, p.updated_at
    FROM Provinces p INNER JOIN Regions r ON r.id_region = p.id_region
    WHERE p.id_province = @id_province AND p.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_province_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_create;
GO
CREATE PROCEDURE dbo.sp_province_create @id_region INT, @name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Regions WHERE id_region = @id_region AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'La región seleccionada no es válida.' AS message, NULL AS id_province; RETURN; END
    IF EXISTS (SELECT 1 FROM Provinces WHERE id_region = @id_region AND name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe una provincia con ese nombre en la región.' AS message, NULL AS id_province; RETURN; END
    INSERT INTO Provinces (id_region, name) VALUES (@id_region, @name);
    SELECT 1 AS success, 'Provincia creada correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_province;
END
GO

IF OBJECT_ID('dbo.sp_province_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_update;
GO
CREATE PROCEDURE dbo.sp_province_update @id_province INT, @id_region INT, @name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Provinces WHERE id_province = @id_province AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Provinces WHERE id_region = @id_region AND name = @name AND id_province <> @id_province AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otra provincia con ese nombre en la región.' AS message; RETURN; END
    UPDATE Provinces SET id_region = @id_region, name = @name, updated_at = GETDATE() WHERE id_province = @id_province;
    SELECT 1 AS success, 'Provincia actualizada correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_province_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_delete_logic;
GO
CREATE PROCEDURE dbo.sp_province_delete_logic @id_province INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Provinces WHERE id_province = @id_province AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message; RETURN; END
    UPDATE Provinces SET status = 0, updated_at = GETDATE() WHERE id_province = @id_province;
    SELECT 1 AS success, 'Provincia desactivada correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_province_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_restore;
GO
CREATE PROCEDURE dbo.sp_province_restore @id_province INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Provinces WHERE id_province = @id_province AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message; RETURN; END
    DECLARE @rid INT, @n VARCHAR(100);
    SELECT @rid = id_region, @n = name FROM Provinces WHERE id_province = @id_province;
    IF EXISTS (SELECT 1 FROM Provinces WHERE id_region = @rid AND name = @n AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe una provincia activa con el mismo nombre.' AS message; RETURN; END
    UPDATE Provinces SET status = 1, updated_at = GETDATE() WHERE id_province = @id_province;
    SELECT 1 AS success, 'Provincia restaurada correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_province_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_province_delete_physical;
GO
CREATE PROCEDURE dbo.sp_province_delete_physical @id_province INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Provinces WHERE id_province = @id_province AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Provinces WHERE id_province = @id_province;
        SELECT 1 AS success, 'Provincia eliminada permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: la provincia tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- DISTRICTS
-- ############################################################

IF OBJECT_ID('dbo.sp_district_province_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_province_list_active;
GO
CREATE PROCEDURE dbo.sp_district_province_list_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.id_province, r.name + ' - ' + p.name AS name
    FROM Provinces p INNER JOIN Regions r ON r.id_region = p.id_region
    WHERE p.deleted_at IS NULL AND p.status = 1 AND r.deleted_at IS NULL AND r.status = 1
    ORDER BY r.name, p.name;
END
GO

IF OBJECT_ID('dbo.sp_district_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_list_active;
GO
CREATE PROCEDURE dbo.sp_district_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT d.id_district, d.id_province, p.name AS province_name, d.name, d.created_at, COUNT(*) OVER() AS total_count
    FROM Districts d INNER JOIN Provinces p ON p.id_province = d.id_province
    WHERE d.deleted_at IS NULL AND d.status = 1 AND p.deleted_at IS NULL
      AND (@search IS NULL OR @search = '' OR d.name LIKE '%' + @search + '%' OR p.name LIKE '%' + @search + '%')
    ORDER BY d.id_district DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_district_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_list_inactive;
GO
CREATE PROCEDURE dbo.sp_district_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT d.id_district, d.id_province, p.name AS province_name, d.name, d.updated_at, COUNT(*) OVER() AS total_count
    FROM Districts d INNER JOIN Provinces p ON p.id_province = d.id_province
    WHERE d.deleted_at IS NULL AND d.status = 0
      AND (@search IS NULL OR @search = '' OR d.name LIKE '%' + @search + '%' OR p.name LIKE '%' + @search + '%')
    ORDER BY d.id_district DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_district_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_get_by_id;
GO
CREATE PROCEDURE dbo.sp_district_get_by_id @id_district INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.id_district, d.id_province, p.name AS province_name, d.name, d.status, d.created_at, d.updated_at
    FROM Districts d INNER JOIN Provinces p ON p.id_province = d.id_province
    WHERE d.id_district = @id_district AND d.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_district_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_create;
GO
CREATE PROCEDURE dbo.sp_district_create @id_province INT, @name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Provinces WHERE id_province = @id_province AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'La provincia seleccionada no es válida.' AS message, NULL AS id_district; RETURN; END
    IF EXISTS (SELECT 1 FROM Districts WHERE id_province = @id_province AND name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un distrito con ese nombre en la provincia.' AS message, NULL AS id_district; RETURN; END
    INSERT INTO Districts (id_province, name) VALUES (@id_province, @name);
    SELECT 1 AS success, 'Distrito creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_district;
END
GO

IF OBJECT_ID('dbo.sp_district_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_update;
GO
CREATE PROCEDURE dbo.sp_district_update @id_district INT, @id_province INT, @name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Districts WHERE id_province = @id_province AND name = @name AND id_district <> @id_district AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro distrito con ese nombre en la provincia.' AS message; RETURN; END
    UPDATE Districts SET id_province = @id_province, name = @name, updated_at = GETDATE() WHERE id_district = @id_district;
    SELECT 1 AS success, 'Distrito actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_district_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_delete_logic;
GO
CREATE PROCEDURE dbo.sp_district_delete_logic @id_district INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message; RETURN; END
    UPDATE Districts SET status = 0, updated_at = GETDATE() WHERE id_district = @id_district;
    SELECT 1 AS success, 'Distrito desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_district_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_restore;
GO
CREATE PROCEDURE dbo.sp_district_restore @id_district INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message; RETURN; END
    DECLARE @pid INT, @n VARCHAR(100);
    SELECT @pid = id_province, @n = name FROM Districts WHERE id_district = @id_district;
    IF EXISTS (SELECT 1 FROM Districts WHERE id_province = @pid AND name = @n AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe un distrito activo con el mismo nombre.' AS message; RETURN; END
    UPDATE Districts SET status = 1, updated_at = GETDATE() WHERE id_district = @id_district;
    SELECT 1 AS success, 'Distrito restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_district_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_district_delete_physical;
GO
CREATE PROCEDURE dbo.sp_district_delete_physical @id_district INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Districts WHERE id_district = @id_district AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Districts WHERE id_district = @id_district;
        SELECT 1 AS success, 'Distrito eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el distrito tiene dependencias.' AS message;
    END CATCH
END
GO

-- ############################################################
-- ROLES (Seguridad)
-- ############################################################

IF OBJECT_ID('dbo.sp_role_list_select_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_list_select_active;
GO
CREATE PROCEDURE dbo.sp_role_list_select_active
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_role, name FROM Roles WHERE deleted_at IS NULL AND status = 1 ORDER BY name;
END
GO

IF OBJECT_ID('dbo.sp_role_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_list_active;
GO
CREATE PROCEDURE dbo.sp_role_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT r.id_role, r.name, r.description, COUNT(*) OVER() AS total_count
    FROM Roles r
    WHERE r.deleted_at IS NULL AND r.status = 1
      AND (@search IS NULL OR @search = '' OR r.name LIKE '%' + @search + '%' OR r.description LIKE '%' + @search + '%')
    ORDER BY r.id_role DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_role_list_inactive', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_list_inactive;
GO
CREATE PROCEDURE dbo.sp_role_list_inactive
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT r.id_role, r.name, r.description, COUNT(*) OVER() AS total_count
    FROM Roles r
    WHERE r.deleted_at IS NULL AND r.status = 0
      AND (@search IS NULL OR @search = '' OR r.name LIKE '%' + @search + '%' OR r.description LIKE '%' + @search + '%')
    ORDER BY r.id_role DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_role_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_get_by_id;
GO
CREATE PROCEDURE dbo.sp_role_get_by_id @id_role INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_role, name, description, status, created_at, updated_at
    FROM Roles WHERE id_role = @id_role AND deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_role_create', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_create;
GO
CREATE PROCEDURE dbo.sp_role_create @name VARCHAR(50), @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Roles WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe un rol con ese nombre.' AS message, NULL AS id_role; RETURN; END
    INSERT INTO Roles (name, description) VALUES (@name, @description);
    SELECT 1 AS success, 'Rol creado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_role;
END
GO

IF OBJECT_ID('dbo.sp_role_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_update;
GO
CREATE PROCEDURE dbo.sp_role_update @id_role INT, @name VARCHAR(50), @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Roles WHERE id_role = @id_role AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Roles WHERE name = @name AND id_role <> @id_role AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro rol con ese nombre.' AS message; RETURN; END
    UPDATE Roles SET name = @name, description = @description, updated_at = GETDATE() WHERE id_role = @id_role;
    SELECT 1 AS success, 'Rol actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_role_delete_logic', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_delete_logic;
GO
CREATE PROCEDURE dbo.sp_role_delete_logic @id_role INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Roles WHERE id_role = @id_role AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está inactivo.' AS message; RETURN; END
    UPDATE Roles SET status = 0, updated_at = GETDATE() WHERE id_role = @id_role;
    SELECT 1 AS success, 'Rol desactivado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_role_restore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_restore;
GO
CREATE PROCEDURE dbo.sp_role_restore @id_role INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Roles WHERE id_role = @id_role AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado o ya está activo.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Roles WHERE name = (SELECT name FROM Roles WHERE id_role = @id_role) AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'No se puede restaurar: ya existe un rol activo con el mismo nombre.' AS message; RETURN; END
    UPDATE Roles SET status = 1, updated_at = GETDATE() WHERE id_role = @id_role;
    SELECT 1 AS success, 'Rol restaurado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_role_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_role_delete_physical;
GO
CREATE PROCEDURE dbo.sp_role_delete_physical @id_role INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Roles WHERE id_role = @id_role AND status = 0 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Solo se pueden eliminar registros inactivos.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Roles WHERE id_role = @id_role;
        SELECT 1 AS success, 'Rol eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el rol tiene usuarios asociados.' AS message;
    END CATCH
END
GO

-- ############################################################
-- USERS (Seguridad) — sin eliminación lógica
-- ############################################################

IF OBJECT_ID('dbo.sp_user_role_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_user_role_list_active;
GO
CREATE PROCEDURE dbo.sp_user_role_list_active
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_role_list_select_active;
END
GO

IF OBJECT_ID('dbo.sp_user_list_active', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_user_list_active;
GO
CREATE PROCEDURE dbo.sp_user_list_active
    @search    VARCHAR(100) = NULL,
    @page      INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT u.id_user, u.username, r.name AS role_name, COUNT(*) OVER() AS total_count
    FROM Users u INNER JOIN Roles r ON r.id_role = u.id_role
    WHERE u.deleted_at IS NULL AND r.deleted_at IS NULL
      AND (@search IS NULL OR @search = '' OR u.username LIKE '%' + @search + '%' OR r.name LIKE '%' + @search + '%')
    ORDER BY u.id_user DESC
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.sp_user_get_by_id', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_user_get_by_id;
GO
CREATE PROCEDURE dbo.sp_user_get_by_id @id_user INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.id_user, u.id_role, u.username, r.name AS role_name, u.created_at, u.updated_at
    FROM Users u INNER JOIN Roles r ON r.id_role = u.id_role
    WHERE u.id_user = @id_user AND u.deleted_at IS NULL;
END
GO

IF OBJECT_ID('dbo.sp_user_update', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_user_update;
GO
CREATE PROCEDURE dbo.sp_user_update
    @id_user       INT,
    @username      VARCHAR(50),
    @id_role       INT,
    @password_hash VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Users WHERE id_user = @id_user AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END
    IF EXISTS (SELECT 1 FROM Users WHERE username = @username AND id_user <> @id_user AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe otro usuario con ese nombre.' AS message; RETURN; END
    IF NOT EXISTS (SELECT 1 FROM Roles WHERE id_role = @id_role AND status = 1 AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El rol seleccionado no es válido.' AS message; RETURN; END
    IF @password_hash IS NULL OR @password_hash = ''
        UPDATE Users SET username = @username, id_role = @id_role, updated_at = GETDATE() WHERE id_user = @id_user;
    ELSE
        UPDATE Users SET username = @username, id_role = @id_role, password_hash = @password_hash, updated_at = GETDATE() WHERE id_user = @id_user;
    SELECT 1 AS success, 'Usuario actualizado correctamente.' AS message;
END
GO

IF OBJECT_ID('dbo.sp_user_delete_physical', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_user_delete_physical;
GO
CREATE PROCEDURE dbo.sp_user_delete_physical @id_user INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Users WHERE id_user = @id_user AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Registro no encontrado.' AS message; RETURN; END
    BEGIN TRY
        DELETE FROM Users WHERE id_user = @id_user;
        SELECT 1 AS success, 'Usuario eliminado permanentemente.' AS message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS success, 'No se puede eliminar: el usuario tiene dependencias.' AS message;
    END CATCH
END
GO
