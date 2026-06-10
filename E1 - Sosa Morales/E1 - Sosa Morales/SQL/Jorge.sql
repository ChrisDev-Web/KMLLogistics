USE KMLLogistics;
GO

---CATEGORIAS

-- ==========================================
-- 1. ARREGLANDO: ACTUALIZAR
-- ==========================================
ALTER PROCEDURE sp_category_update
    @id_category INT,
    @name VARCHAR(100),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Categories WHERE name = @name AND id_category <> @id_category AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'El nombre ya está siendo usado por otra categoría.' AS message, NULL AS id_category;
        RETURN;
    END

    UPDATE Categories
    SET name = @name,
        description = @description,
        updated_at = CURRENT_TIMESTAMP
    WHERE id_category = @id_category AND deleted_at IS NULL;

    -- Agregamos el id_category al final
    SELECT 1 AS success, 'Categoría actualizada correctamente.' AS message, @id_category AS id_category;
END
GO

-- ==========================================
-- 2. ARREGLANDO: ELIMINAR FÍSICO
-- ==========================================
ALTER PROCEDURE sp_category_delete_physical
    @id_category INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Products WHERE id_category = @id_category AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'No se puede eliminar la categoría porque tiene productos asociados.' AS message, NULL AS id_category;
        RETURN;
    END

    DELETE FROM Categories WHERE id_category = @id_category;

    -- Agregamos el id_category al final
    SELECT 1 AS success, 'Categoría eliminada permanentemente.' AS message, @id_category AS id_category;
END
GO

-- ==========================================
-- 3. ARREGLANDO: ELIMINAR LÓGICO (Desactivar)
-- ==========================================
ALTER PROCEDURE sp_category_delete_logic
    @id_category INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Categories
    SET status = 0,
        deleted_at = CURRENT_TIMESTAMP
    WHERE id_category = @id_category;

    -- Agregamos el id_category al final
    SELECT 1 AS success, 'Categoría desactivada correctamente.' AS message, @id_category AS id_category;
END
GO

-- ==========================================
-- 4. ARREGLANDO: RESTAURAR
-- ==========================================
ALTER PROCEDURE sp_category_restore
    @id_category INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Categories
    SET status = 1,
        deleted_at = NULL,
        updated_at = CURRENT_TIMESTAMP
    WHERE id_category = @id_category;

    -- Agregamos el id_category al final
    SELECT 1 AS success, 'Categoría restaurada correctamente.' AS message, @id_category AS id_category;
END
GO


-----MARCAS------

-- 1. LISTAR ACTIVOS
IF OBJECT_ID('sp_brand_list_active', 'P') IS NOT NULL DROP PROCEDURE sp_brand_list_active;
GO
CREATE PROCEDURE sp_brand_list_active @search VARCHAR(100) = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_brand, name, description, status FROM Brands
    WHERE deleted_at IS NULL AND status = 1
      AND (@search IS NULL OR name LIKE '%' + @search + '%' OR description LIKE '%' + @search + '%')
    ORDER BY name;
END
GO

-- 2. OBTENER POR ID
IF OBJECT_ID('sp_brand_get_by_id', 'P') IS NOT NULL DROP PROCEDURE sp_brand_get_by_id;
GO
CREATE PROCEDURE sp_brand_get_by_id @id_brand INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_brand, name, description, status, created_at, updated_at FROM Brands
    WHERE id_brand = @id_brand;
END
GO

-- 3. CREAR
IF OBJECT_ID('sp_brand_create', 'P') IS NOT NULL DROP PROCEDURE sp_brand_create;
GO
CREATE PROCEDURE sp_brand_create @name VARCHAR(100), @description VARCHAR(255) = NULL AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Brands WHERE name = @name AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'Ya existe una marca con este nombre.' AS message, NULL AS id_brand; RETURN; END

    INSERT INTO Brands (name, description, status, created_at) VALUES (@name, @description, 1, CURRENT_TIMESTAMP);
SELECT 1 AS success, 'Marca registrada.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_brand;
END
GO

-- 4. ACTUALIZAR
IF OBJECT_ID('sp_brand_update', 'P') IS NOT NULL DROP PROCEDURE sp_brand_update;
GO
CREATE PROCEDURE sp_brand_update @id_brand INT, @name VARCHAR(100), @description VARCHAR(255) = NULL AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Brands WHERE name = @name AND id_brand <> @id_brand AND deleted_at IS NULL)
    BEGIN SELECT 0 AS success, 'El nombre ya está siendo usado por otra marca.' AS message, NULL AS id_brand; RETURN; END

    UPDATE Brands SET name = @name, description = @description, updated_at = CURRENT_TIMESTAMP WHERE id_brand = @id_brand AND deleted_at IS NULL;
    SELECT 1 AS success, 'Marca actualizada.' AS message, CAST(@id_brand AS INT) AS id_brand;
END
GO

-- 5. ELIMINAR FÍSICO (Con validación doble por tus relaciones)
IF OBJECT_ID('sp_brand_delete_physical', 'P') IS NOT NULL DROP PROCEDURE sp_brand_delete_physical;
GO
CREATE PROCEDURE sp_brand_delete_physical @id_brand INT AS
BEGIN
    SET NOCOUNT ON;
    -- Validar si tiene productos o proveedores asociados
    IF EXISTS (SELECT 1 FROM Products WHERE id_brand = @id_brand AND deleted_at IS NULL) OR
       EXISTS (SELECT 1 FROM SupplierBrands WHERE id_brand = @id_brand)
    BEGIN SELECT 0 AS success, 'No se puede eliminar la marca porque tiene productos o proveedores asociados.' AS message, NULL AS id_brand; RETURN; END

    DELETE FROM Brands WHERE id_brand = @id_brand;
    SELECT 1 AS success, 'Acción realizada.' AS message, CAST(@id_brand AS INT) AS id_brand;
END
GO

-- 6. LISTAR INACTIVOS
IF OBJECT_ID('sp_brand_list_inactive', 'P') IS NOT NULL DROP PROCEDURE sp_brand_list_inactive;
GO
CREATE PROCEDURE sp_brand_list_inactive @search VARCHAR(100) = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_brand, name, description, status FROM Brands
    WHERE (deleted_at IS NOT NULL OR status = 0)
      AND (@search IS NULL OR name LIKE '%' + @search + '%' OR description LIKE '%' + @search + '%')
    ORDER BY name;
END
GO

-- 7. ELIMINAR LÓGICO
IF OBJECT_ID('sp_brand_delete_logic', 'P') IS NOT NULL DROP PROCEDURE sp_brand_delete_logic;
GO
CREATE PROCEDURE sp_brand_delete_logic @id_brand INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Brands SET status = 0, deleted_at = CURRENT_TIMESTAMP WHERE id_brand = @id_brand;
    SELECT 1 AS success, 'Marca desactivada correctamente.' AS message, @id_brand AS id_brand;
END
GO

-- 8. RESTAURAR
IF OBJECT_ID('sp_brand_restore', 'P') IS NOT NULL DROP PROCEDURE sp_brand_restore;
GO
CREATE PROCEDURE sp_brand_restore @id_brand INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Brands SET status = 1, deleted_at = NULL, updated_at = CURRENT_TIMESTAMP WHERE id_brand = @id_brand;
    SELECT 1 AS success, 'Marca restaurada correctamente.' AS message, @id_brand AS id_brand;
END
GO


----------PRODUCTOS----------



-- 1. LISTAR ACTIVOS
IF OBJECT_ID('sp_product_list_active', 'P') IS NOT NULL DROP PROCEDURE sp_product_list_active;
GO
CREATE PROCEDURE sp_product_list_active @search VARCHAR(100) = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        p.id_product, p.name, c.name AS category_name, b.name AS brand_name,
        p.cost, p.profit_percentage, p.sale_price, p.status
    FROM Products p
    INNER JOIN Categories c ON p.id_category = c.id_category
    INNER JOIN Brands b ON p.id_brand = b.id_brand
    WHERE p.deleted_at IS NULL AND p.status = 1
      AND (@search IS NULL OR p.name LIKE '%' + @search + '%' OR c.name LIKE '%' + @search + '%' OR b.name LIKE '%' + @search + '%')
    ORDER BY p.name;
END
GO

-- 2. LISTAR INACTIVOS
IF OBJECT_ID('sp_product_list_inactive', 'P') IS NOT NULL DROP PROCEDURE sp_product_list_inactive;
GO
CREATE PROCEDURE sp_product_list_inactive @search VARCHAR(100) = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        p.id_product, p.name, c.name AS category_name, b.name AS brand_name,
        p.cost, p.profit_percentage, p.sale_price, p.status
    FROM Products p
    INNER JOIN Categories c ON p.id_category = c.id_category
    INNER JOIN Brands b ON p.id_brand = b.id_brand
    WHERE (p.deleted_at IS NOT NULL OR p.status = 0)
      AND (@search IS NULL OR p.name LIKE '%' + @search + '%' OR c.name LIKE '%' + @search + '%' OR b.name LIKE '%' + @search + '%')
    ORDER BY p.name;
END
GO

-- 3. OBTENER POR ID
IF OBJECT_ID('sp_product_get_by_id', 'P') IS NOT NULL DROP PROCEDURE sp_product_get_by_id;
GO
CREATE PROCEDURE sp_product_get_by_id @id_product INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        id_product, id_category, id_brand, name, description, 
        cost, profit_percentage, sale_price, 
        weight, height, width, length, volume, status, created_at, updated_at
    FROM Products
    WHERE id_product = @id_product;
END
GO

-- 4. CREAR
IF OBJECT_ID('sp_product_create', 'P') IS NOT NULL DROP PROCEDURE sp_product_create;
GO
CREATE PROCEDURE sp_product_create 
    @id_category INT, @id_brand INT, @name VARCHAR(100), @description VARCHAR(255) = NULL,
    @cost DECIMAL(10,2), @profit_percentage DECIMAL(5,2),
    @weight DECIMAL(10,2) = NULL, @height DECIMAL(10,2) = NULL, @width DECIMAL(10,2) = NULL, @length DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Products (id_category, id_brand, name, description, cost, profit_percentage, weight, height, width, length, status, created_at) 
    VALUES (@id_category, @id_brand, @name, @description, @cost, @profit_percentage, @weight, @height, @width, @length, 1, CURRENT_TIMESTAMP);
    
    SELECT 1 AS success, 'Producto registrado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_product;
END
GO

-- 5. ACTUALIZAR
IF OBJECT_ID('sp_product_update', 'P') IS NOT NULL DROP PROCEDURE sp_product_update;
GO
CREATE PROCEDURE sp_product_update 
    @id_product INT, @id_category INT, @id_brand INT, @name VARCHAR(100), @description VARCHAR(255) = NULL,
    @cost DECIMAL(10,2), @profit_percentage DECIMAL(5,2),
    @weight DECIMAL(10,2) = NULL, @height DECIMAL(10,2) = NULL, @width DECIMAL(10,2) = NULL, @length DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Products 
    SET id_category = @id_category, id_brand = @id_brand, name = @name, description = @description, 
        cost = @cost, profit_percentage = @profit_percentage, 
        weight = @weight, height = @height, width = @width, length = @length, updated_at = CURRENT_TIMESTAMP 
    WHERE id_product = @id_product AND deleted_at IS NULL;
    
    SELECT 1 AS success, 'Producto actualizado correctamente.' AS message, CAST(@id_product AS INT) AS id_product;
END
GO

-- 6. ELIMINAR LÓGICO
IF OBJECT_ID('sp_product_delete_logic', 'P') IS NOT NULL DROP PROCEDURE sp_product_delete_logic;
GO
CREATE PROCEDURE sp_product_delete_logic @id_product INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Products SET status = 0, deleted_at = CURRENT_TIMESTAMP WHERE id_product = @id_product;
    SELECT 1 AS success, 'Producto desactivado correctamente.' AS message, CAST(@id_product AS INT) AS id_product;
END
GO

-- 7. RESTAURAR
IF OBJECT_ID('sp_product_restore', 'P') IS NOT NULL DROP PROCEDURE sp_product_restore;
GO
CREATE PROCEDURE sp_product_restore @id_product INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Products SET status = 1, deleted_at = NULL, updated_at = CURRENT_TIMESTAMP WHERE id_product = @id_product;
    SELECT 1 AS success, 'Producto restaurado correctamente.' AS message, CAST(@id_product AS INT) AS id_product;
END
GO

-- 8. ELIMINAR FÍSICO (Validación múltiple)
IF OBJECT_ID('sp_product_delete_physical', 'P') IS NOT NULL DROP PROCEDURE sp_product_delete_physical;
GO
CREATE PROCEDURE sp_product_delete_physical @id_product INT AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM ProductSuppliers WHERE id_product = @id_product) OR
       EXISTS (SELECT 1 FROM WarehouseDetails WHERE id_product = @id_product) OR
       EXISTS (SELECT 1 FROM SaleDetails WHERE id_product = @id_product) OR
       EXISTS (SELECT 1 FROM TransferDetails WHERE id_product = @id_product) OR
       EXISTS (SELECT 1 FROM InventoryMovements WHERE id_product = @id_product)
    BEGIN 
        SELECT 0 AS success, 'No se puede eliminar. El producto tiene movimientos, ventas o almacenes asociados.' AS message, CAST(NULL AS INT) AS id_product; 
        RETURN; 
    END

    DELETE FROM Products WHERE id_product = @id_product;
    SELECT 1 AS success, 'Producto eliminado permanentemente.' AS message, CAST(@id_product AS INT) AS id_product;
END
GO

------------PRODUTOS PROVEEDORESZZZZZZZZZZZZZZZZ------------

-- 1. LISTAR (Join para mostrar nombres claros)
CREATE OR ALTER PROCEDURE sp_product_supplier_list_active @search VARCHAR(100) = NULL AS
BEGIN
    SELECT ps.id_product_supplier, p.name AS product_name, s.name AS supplier_name, ps.supplier_cost, ps.is_main_supplier
    FROM ProductSuppliers ps
    JOIN Products p ON ps.id_product = p.id_product
    JOIN Suppliers s ON ps.id_supplier = s.id_supplier
    WHERE ps.status = 1 AND (@search IS NULL OR p.name LIKE '%' + @search + '%');
END
GO

-- 2. CREAR
CREATE OR ALTER PROCEDURE sp_product_supplier_create 
    @id_product INT, @id_supplier INT, @supplier_cost DECIMAL(10,2), @is_main_supplier BIT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM ProductSuppliers WHERE id_product = @id_product AND id_supplier = @id_supplier)
    BEGIN SELECT 0 AS success, 'Esta relación ya existe.' AS message; RETURN; END
    
    INSERT INTO ProductSuppliers (id_product, id_supplier, supplier_cost, is_main_supplier, status)
    VALUES (@id_product, @id_supplier, @supplier_cost, @is_main_supplier, 1);
    SELECT 1 AS success, 'Registrado correctamente.' AS message;
END
GO

-- 3. ELIMINAR (Físico porque es una tabla de relación)
CREATE OR ALTER PROCEDURE sp_product_supplier_delete @id_product_supplier INT AS
BEGIN
    DELETE FROM ProductSuppliers WHERE id_product_supplier = @id_product_supplier;
    SELECT 1 AS success, 'Relación eliminada.' AS message;
END
GO


----------MARCAS DE PROVEEDOREZZZZZ-------


USE KMLLogistics;
GO

-- 1. LISTAR (Join para mostrar nombres claros)
CREATE OR ALTER PROCEDURE sp_supplier_brand_list @search VARCHAR(100) = NULL AS
BEGIN
    SELECT sb.id_supplier, sb.id_brand, s.name AS supplier_name, b.name AS brand_name
    FROM SupplierBrands sb
    JOIN Suppliers s ON sb.id_supplier = s.id_supplier
    JOIN Brands b ON sb.id_brand = b.id_brand
    WHERE (@search IS NULL OR s.name LIKE '%' + @search + '%' OR b.name LIKE '%' + @search + '%');
END
GO

-- 2. CREAR
CREATE OR ALTER PROCEDURE sp_supplier_brand_create @id_supplier INT, @id_brand INT AS
BEGIN
    IF EXISTS (SELECT 1 FROM SupplierBrands WHERE id_supplier = @id_supplier AND id_brand = @id_brand)
    BEGIN SELECT 0 AS success, 'Esta asignación ya existe.' AS message; RETURN; END
    
    INSERT INTO SupplierBrands (id_supplier, id_brand) VALUES (@id_supplier, @id_brand);
    SELECT 1 AS success, 'Marca asignada al proveedor.' AS message;
END
GO

-- 3. ELIMINAR
CREATE OR ALTER PROCEDURE sp_supplier_brand_delete @id_supplier INT, @id_brand INT AS
BEGIN
    DELETE FROM SupplierBrands WHERE id_supplier = @id_supplier AND id_brand = @id_brand;
    SELECT 1 AS success, 'Relación eliminada.' AS message;
END
GO