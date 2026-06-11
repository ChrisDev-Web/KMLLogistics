-- ============================================================
-- KMLLogistics - Stored Procedures: Login y Register
-- Base de datos: SQL Server 2017
-- Nomenclatura: sp_(tabla_singular)_(funcion)_(campo_opcional)
-- ============================================================

USE KMLLogistics;
GO

-- ============================================================
-- sp_user_get_by_username
-- Obtiene un usuario activo por nombre de usuario (para Login)
-- ============================================================
IF OBJECT_ID('sp_user_get_by_username', 'P') IS NOT NULL
    DROP PROCEDURE sp_user_get_by_username;
GO

CREATE PROCEDURE sp_user_get_by_username
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

-- ============================================================
-- sp_user_create
-- Registra un nuevo usuario en el sistema
-- ============================================================
IF OBJECT_ID('sp_user_create', 'P') IS NOT NULL
    DROP PROCEDURE sp_user_create;
GO

CREATE PROCEDURE sp_user_create
    @username      VARCHAR(50),
    @password_hash VARCHAR(255),
    @id_role       INT = 2
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Users WHERE username = @username AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'El nombre de usuario ya existe.' AS message, CAST(NULL AS INT) AS id_user;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM Roles WHERE id_role = @id_role AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'El rol especificado no existe.' AS message, CAST(NULL AS INT) AS id_user;
        RETURN;
    END

    INSERT INTO Users (id_role, username, password_hash)
    VALUES (@id_role, @username, @password_hash);

    SELECT 1 AS success, 'Usuario registrado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_user;
END
GO

-- Roles para Register: usar dbo.sp_role_list_select_active (definido en Espinoza.sql).
-- No redefinir sp_role_list_active aquí; el módulo Seguridad usa la versión paginada.