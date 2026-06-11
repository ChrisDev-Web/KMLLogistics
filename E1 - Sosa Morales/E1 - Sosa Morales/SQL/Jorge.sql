USE KMLLogistics;
GO

-- ==========================================
-- Categorias
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_category_list_active
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

      SELECT
        id_category,
        name,
        description,
        photo,
        status,
        COUNT(*) OVER() AS total_count
    FROM dbo.Categories
    WHERE deleted_at IS NULL
      AND status = 1
      AND (
            @search IS NULL
            OR name LIKE '%' + @search + '%'
            OR description LIKE '%' + @search + '%'
          )
    ORDER BY name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_category_list_inactive
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        id_category,
        name,
        description,
        photo,
        status,
        COUNT(*) OVER() AS total_count
    FROM dbo.Categories
    WHERE (deleted_at IS NOT NULL OR status = 0)
      AND (
            @search IS NULL
            OR name LIKE '%' + @search + '%'
            OR description LIKE '%' + @search + '%'
          )
    ORDER BY name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_category_get_by_id
    @id_category INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_category,
        name,
        description,
        photo
    FROM dbo.Categories
    WHERE id_category = @id_category;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_category_create
    @name VARCHAR(100),
    @description VARCHAR(255) = NULL,
    @photo VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Categories WHERE name = @name)
    BEGIN
        SELECT 0 AS success, 'Ya existe una categoria con este nombre.' AS message, CAST(NULL AS INT) AS id_category;
        RETURN;
    END

    INSERT INTO dbo.Categories (name, description, photo, status, created_at)
    VALUES (@name, @description, @photo, 1, GETDATE());

    SELECT 1 AS success, 'Categoria registrada correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_category;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_category_update
    @id_category INT,
    @name VARCHAR(100),
    @description VARCHAR(255) = NULL,
    @photo VARCHAR(255) = NULL,
    @remove_photo BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE id_category = @id_category)
    BEGIN
        SELECT 0 AS success, 'La categoria no existe.' AS message, CAST(NULL AS INT) AS id_category;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Categories WHERE name = @name AND id_category <> @id_category)
    BEGIN
        SELECT 0 AS success, 'El nombre ya esta siendo usado por otra categoria.' AS message, CAST(NULL AS INT) AS id_category;
        RETURN;
    END

    UPDATE dbo.Categories
    SET name = @name,
        description = @description,
        photo = CASE
                    WHEN @remove_photo = 1 THEN NULL
                    WHEN @photo IS NOT NULL THEN @photo
                    ELSE photo
                END,
        updated_at = GETDATE()
    WHERE id_category = @id_category;

    SELECT 1 AS success, 'Categoria actualizada correctamente.' AS message, @id_category AS id_category;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_category_delete_logic
    @id_category INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE id_category = @id_category)
    BEGIN
        SELECT 0 AS success, 'La categoria no existe.' AS message, CAST(NULL AS INT) AS id_category;
        RETURN;
    END

    UPDATE dbo.Categories
    SET status = 0,
        deleted_at = GETDATE()
    WHERE id_category = @id_category;

    SELECT 1 AS success, 'Categoria desactivada correctamente.' AS message, @id_category AS id_category;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_category_restore
    @id_category INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE id_category = @id_category)
    BEGIN
        SELECT 0 AS success, 'La categoria no existe.' AS message, CAST(NULL AS INT) AS id_category;
        RETURN;
    END

    UPDATE dbo.Categories
    SET status = 1,
        deleted_at = NULL,
        updated_at = GETDATE()
    WHERE id_category = @id_category;

    SELECT 1 AS success, 'Categoria restaurada correctamente.' AS message, @id_category AS id_category;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_category_delete_physical
    @id_category INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Products WHERE id_category = @id_category)
    BEGIN
        SELECT 0 AS success, 'No se puede eliminar la categoria porque tiene productos asociados.' AS message, CAST(NULL AS INT) AS id_category;
        RETURN;
    END

    DELETE FROM dbo.Categories
    WHERE id_category = @id_category;

    SELECT 1 AS success, 'Categoria eliminada permanentemente.' AS message, @id_category AS id_category;
END
GO

-- ==========================================
-- Marcas
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_brand_list_active
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        id_brand,
        name,
        description,
        status,
        COUNT(*) OVER() AS total_count
    FROM dbo.Brands
    WHERE deleted_at IS NULL
      AND status = 1
      AND (
            @search IS NULL
            OR name LIKE '%' + @search + '%'
            OR description LIKE '%' + @search + '%'
          )
    ORDER BY name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_brand_list_inactive
    @search VARCHAR(100) = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        id_brand,
        name,
        description,
        status,
        COUNT(*) OVER() AS total_count
    FROM dbo.Brands
    WHERE (deleted_at IS NOT NULL OR status = 0)
      AND (
            @search IS NULL
            OR name LIKE '%' + @search + '%'
            OR description LIKE '%' + @search + '%'
          )
    ORDER BY name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_brand_get_by_id
    @id_brand INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_brand,
        name,
        description,
        status,
        created_at,
        updated_at
    FROM dbo.Brands
    WHERE id_brand = @id_brand;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_brand_create
    @name VARCHAR(100),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Brands WHERE name = @name)
    BEGIN
        SELECT 0 AS success, 'Ya existe una marca con este nombre.' AS message, CAST(NULL AS INT) AS id_brand;
        RETURN;
    END

    INSERT INTO dbo.Brands (name, description, status, created_at)
    VALUES (@name, @description, 1, GETDATE());

    SELECT 1 AS success, 'Marca registrada correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_brand;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_brand_update
    @id_brand INT,
    @name VARCHAR(100),
    @description VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE id_brand = @id_brand)
    BEGIN
        SELECT 0 AS success, 'La marca no existe.' AS message, CAST(NULL AS INT) AS id_brand;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Brands WHERE name = @name AND id_brand <> @id_brand)
    BEGIN
        SELECT 0 AS success, 'El nombre ya esta siendo usado por otra marca.' AS message, CAST(NULL AS INT) AS id_brand;
        RETURN;
    END

    UPDATE dbo.Brands
    SET name = @name,
        description = @description,
        updated_at = GETDATE()
    WHERE id_brand = @id_brand;

    SELECT 1 AS success, 'Marca actualizada correctamente.' AS message, @id_brand AS id_brand;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_brand_delete_logic
    @id_brand INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE id_brand = @id_brand)
    BEGIN
        SELECT 0 AS success, 'La marca no existe.' AS message, CAST(NULL AS INT) AS id_brand;
        RETURN;
    END

    UPDATE dbo.Brands
    SET status = 0,
        deleted_at = GETDATE()
    WHERE id_brand = @id_brand;

    SELECT 1 AS success, 'Marca desactivada correctamente.' AS message, @id_brand AS id_brand;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_brand_restore
    @id_brand INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE id_brand = @id_brand)
    BEGIN
        SELECT 0 AS success, 'La marca no existe.' AS message, CAST(NULL AS INT) AS id_brand;
        RETURN;
    END

    UPDATE dbo.Brands
    SET status = 1,
        deleted_at = NULL,
        updated_at = GETDATE()
    WHERE id_brand = @id_brand;

    SELECT 1 AS success, 'Marca restaurada correctamente.' AS message, @id_brand AS id_brand;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_brand_delete_physical
    @id_brand INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Products WHERE id_brand = @id_brand)
       OR EXISTS (SELECT 1 FROM dbo.SupplierBrands WHERE id_brand = @id_brand)
    BEGIN
        SELECT 0 AS success, 'No se puede eliminar la marca porque tiene productos o proveedores asociados.' AS message, CAST(NULL AS INT) AS id_brand;
        RETURN;
    END

    DELETE FROM dbo.Brands
    WHERE id_brand = @id_brand;

    SELECT 1 AS success, 'Marca eliminada permanentemente.' AS message, @id_brand AS id_brand;
END
GO

-- ==========================================
-- Productos
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_product_list_active
    @search VARCHAR(100) = NULL,
    @id_category INT = NULL,
    @id_brand INT = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        p.id_product,
        p.photo,
        p.name,
        c.name AS category_name,
        b.name AS brand_name,
        p.cost,
        p.profit_percentage,
        p.sale_price,
        p.status,
        COUNT(*) OVER() AS total_count
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON c.id_category = p.id_category
    INNER JOIN dbo.Brands b ON b.id_brand = p.id_brand
    WHERE p.deleted_at IS NULL
      AND p.status = 1
      AND (@id_category IS NULL OR p.id_category = @id_category)
      AND (@id_brand IS NULL OR p.id_brand = @id_brand)
      AND (
            @search IS NULL
            OR p.name LIKE '%' + @search + '%'
            OR c.name LIKE '%' + @search + '%'
            OR b.name LIKE '%' + @search + '%'
          )
    ORDER BY p.name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_list_inactive
    @search VARCHAR(100) = NULL,
    @id_category INT = NULL,
    @id_brand INT = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        p.id_product,
        p.photo,
        p.name,
        c.name AS category_name,
        b.name AS brand_name,
        p.cost,
        p.profit_percentage,
        p.sale_price,
        p.status,
        COUNT(*) OVER() AS total_count
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON c.id_category = p.id_category
    INNER JOIN dbo.Brands b ON b.id_brand = p.id_brand
    WHERE (p.deleted_at IS NOT NULL OR p.status = 0)
      AND (@id_category IS NULL OR p.id_category = @id_category)
      AND (@id_brand IS NULL OR p.id_brand = @id_brand)
      AND (
            @search IS NULL
            OR p.name LIKE '%' + @search + '%'
            OR c.name LIKE '%' + @search + '%'
            OR b.name LIKE '%' + @search + '%'
          )
    ORDER BY p.name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_get_by_id
    @id_product INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_product,
        id_category,
        id_brand,
        name,
        description,
        photo,
        cost,
        profit_percentage,
        sale_price,
        weight,
        height,
        width,
        length,
        volume,
        status,
        created_at,
        updated_at
    FROM dbo.Products
    WHERE id_product = @id_product;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_create
    @id_category INT,
    @id_brand INT,
    @name VARCHAR(100),
    @description VARCHAR(255) = NULL,
    @photo VARCHAR(255) = NULL,
    @cost DECIMAL(10,2),
    @profit_percentage DECIMAL(5,2),
    @weight DECIMAL(10,2) = NULL,
    @height DECIMAL(10,2) = NULL,
    @width DECIMAL(10,2) = NULL,
    @length DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE id_category = @id_category AND deleted_at IS NULL AND status = 1)
    BEGIN
        SELECT 0 AS success, 'Seleccione una categoria activa.' AS message, CAST(NULL AS INT) AS id_product;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE id_brand = @id_brand AND deleted_at IS NULL AND status = 1)
    BEGIN
        SELECT 0 AS success, 'Seleccione una marca activa.' AS message, CAST(NULL AS INT) AS id_product;
        RETURN;
    END

    INSERT INTO dbo.Products
        (id_category, id_brand, name, description, photo, cost, profit_percentage, weight, height, width, length, status, created_at)
    VALUES
        (@id_category, @id_brand, @name, @description, @photo, @cost, @profit_percentage, @weight, @height, @width, @length, 1, GETDATE());

    SELECT 1 AS success, 'Producto registrado correctamente.' AS message, CAST(SCOPE_IDENTITY() AS INT) AS id_product;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_update
    @id_product INT,
    @id_category INT,
    @id_brand INT,
    @name VARCHAR(100),
    @description VARCHAR(255) = NULL,
    @photo VARCHAR(255) = NULL,
    @remove_photo BIT = 0,
    @cost DECIMAL(10,2),
    @profit_percentage DECIMAL(5,2),
    @weight DECIMAL(10,2) = NULL,
    @height DECIMAL(10,2) = NULL,
    @width DECIMAL(10,2) = NULL,
    @length DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE id_product = @id_product AND deleted_at IS NULL)
    BEGIN
        SELECT 0 AS success, 'El producto no existe o esta inactivo.' AS message, CAST(NULL AS INT) AS id_product;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE id_category = @id_category AND deleted_at IS NULL AND status = 1)
    BEGIN
        SELECT 0 AS success, 'Seleccione una categoria activa.' AS message, CAST(NULL AS INT) AS id_product;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE id_brand = @id_brand AND deleted_at IS NULL AND status = 1)
    BEGIN
        SELECT 0 AS success, 'Seleccione una marca activa.' AS message, CAST(NULL AS INT) AS id_product;
        RETURN;
    END

    UPDATE dbo.Products
    SET id_category = @id_category,
        id_brand = @id_brand,
        name = @name,
        description = @description,
        photo = CASE
                    WHEN @remove_photo = 1 THEN NULL
                    WHEN @photo IS NOT NULL THEN @photo
                    ELSE photo
                END,
        cost = @cost,
        profit_percentage = @profit_percentage,
        weight = @weight,
        height = @height,
        width = @width,
        length = @length,
        updated_at = GETDATE()
    WHERE id_product = @id_product;

    SELECT 1 AS success, 'Producto actualizado correctamente.' AS message, @id_product AS id_product;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_delete_logic
    @id_product INT
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE id_product = @id_product)
    BEGIN
        SELECT 0 AS success, 'El producto no existe.' AS message, CAST(NULL AS INT) AS id_product;
        RETURN;
    END

    UPDATE dbo.Products
    SET status = 0,
        deleted_at = GETDATE()
    WHERE id_product = @id_product;

    SELECT 1 AS success, 'Producto desactivado correctamente.' AS message, @id_product AS id_product;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_restore
    @id_product INT
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE id_product = @id_product)
    BEGIN
        SELECT 0 AS success, 'El producto no existe.' AS message, CAST(NULL AS INT) AS id_product;
        RETURN;
    END

    UPDATE dbo.Products
    SET status = 1,
        deleted_at = NULL,
        updated_at = GETDATE()
    WHERE id_product = @id_product;

    SELECT 1 AS success, 'Producto restaurado correctamente.' AS message, @id_product AS id_product;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_delete_physical
    @id_product INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ProductSuppliers WHERE id_product = @id_product)
       OR EXISTS (SELECT 1 FROM dbo.WarehouseDetails WHERE id_product = @id_product)
       OR EXISTS (SELECT 1 FROM dbo.SaleDetails WHERE id_product = @id_product)
       OR EXISTS (SELECT 1 FROM dbo.TransferDetails WHERE id_product = @id_product)
       OR EXISTS (SELECT 1 FROM dbo.InventoryMovements WHERE id_product = @id_product)
    BEGIN
        SELECT 0 AS success, 'No se puede eliminar. El producto tiene movimientos, ventas o almacenes asociados.' AS message, CAST(NULL AS INT) AS id_product;
        RETURN;
    END

    DELETE FROM dbo.Products
    WHERE id_product = @id_product;

    SELECT 1 AS success, 'Producto eliminado permanentemente.' AS message, @id_product AS id_product;
END
GO

-- ==========================================
-- Producto proveedores
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_product_supplier_list_active
    @search VARCHAR(100) = NULL,
    @id_product INT = NULL,
    @id_supplier INT = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        ps.id_product_supplier AS IdProductSupplier,
        p.name AS ProductName,
        s.name AS SupplierName,
        ps.supplier_cost AS SupplierCost,
        ps.is_main_supplier AS IsMainSupplier,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.ProductSuppliers ps
    INNER JOIN dbo.Products p ON p.id_product = ps.id_product
    INNER JOIN dbo.Suppliers s ON s.id_supplier = ps.id_supplier
    WHERE ps.status = 1
      AND p.deleted_at IS NULL
      AND p.status = 1
      AND s.deleted_at IS NULL
      AND s.status = 1
      AND (@id_product IS NULL OR ps.id_product = @id_product)
      AND (@id_supplier IS NULL OR ps.id_supplier = @id_supplier)
      AND (
            @search IS NULL
            OR p.name LIKE '%' + @search + '%'
            OR s.name LIKE '%' + @search + '%'
          )
    ORDER BY p.name, s.name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_supplier_create
    @id_product INT,
    @id_supplier INT,
    @supplier_cost DECIMAL(10,2),
    @is_main_supplier BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE id_product = @id_product AND deleted_at IS NULL AND status = 1)
    BEGIN
        SELECT 0 AS success, 'Seleccione un producto activo.' AS message, CAST(NULL AS INT) AS id_supplier;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers WHERE id_supplier = @id_supplier AND deleted_at IS NULL AND status = 1)
    BEGIN
        SELECT 0 AS success, 'Seleccione un proveedor activo.' AS message, CAST(NULL AS INT) AS id_supplier;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.ProductSuppliers WHERE id_product = @id_product AND id_supplier = @id_supplier)
    BEGIN
        SELECT 0 AS success, 'Este producto ya esta asignado al proveedor.' AS message, CAST(NULL AS INT) AS id_supplier;
        RETURN;
    END

    INSERT INTO dbo.ProductSuppliers
        (id_product, id_supplier, supplier_cost, is_main_supplier, status, created_at)
    VALUES
        (@id_product, @id_supplier, @supplier_cost, @is_main_supplier, 1, GETDATE());

    INSERT INTO dbo.SupplierBrands (id_supplier, id_brand)
    SELECT @id_supplier, p.id_brand
    FROM dbo.Products p
    WHERE p.id_product = @id_product
      AND NOT EXISTS (
            SELECT 1
            FROM dbo.SupplierBrands sb
            WHERE sb.id_supplier = @id_supplier
              AND sb.id_brand = p.id_brand
          );

    SELECT 1 AS success, 'Producto asignado al proveedor correctamente.' AS message, @id_supplier AS id_supplier;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_supplier_delete
    @id_product_supplier INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.PurchaseDetails WHERE id_product_supplier = @id_product_supplier)
    BEGIN
        SELECT 0 AS success, 'No se puede eliminar porque esta asignacion tiene compras asociadas.' AS message, CAST(NULL AS INT) AS id_supplier;
        RETURN;
    END

    DELETE FROM dbo.ProductSuppliers
    WHERE id_product_supplier = @id_product_supplier;

    SELECT 1 AS success, 'Asignacion eliminada correctamente.' AS message, CAST(NULL AS INT) AS id_supplier;
END
GO

-- ==========================================
-- Marcas de proveedor
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_supplier_brand_list
    @search VARCHAR(100) = NULL,
    @id_brand INT = NULL,
    @id_supplier INT = NULL,
    @page INT = 1,
    @page_size INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @page < 1 SET @page = 1;
    IF @page_size NOT IN (10, 20, 50) SET @page_size = 10;

    SELECT
        sb.id_supplier AS IdSupplier,
        sb.id_brand AS IdBrand,
        s.name AS SupplierName,
        b.name AS BrandName,
        COUNT(*) OVER() AS TotalCount
    FROM dbo.SupplierBrands sb
    INNER JOIN dbo.Suppliers s ON s.id_supplier = sb.id_supplier
    INNER JOIN dbo.Brands b ON b.id_brand = sb.id_brand
    WHERE s.deleted_at IS NULL
      AND s.status = 1
      AND b.deleted_at IS NULL
      AND b.status = 1
      AND (@id_brand IS NULL OR sb.id_brand = @id_brand)
      AND (@id_supplier IS NULL OR sb.id_supplier = @id_supplier)
      AND (
            @search IS NULL
            OR s.name LIKE '%' + @search + '%'
            OR b.name LIKE '%' + @search + '%'
          )
    ORDER BY s.name, b.name
    OFFSET (@page - 1) * @page_size ROWS FETCH NEXT @page_size ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_supplier_brand_create
    @id_supplier INT,
    @id_brand INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers WHERE id_supplier = @id_supplier AND deleted_at IS NULL AND status = 1)
    BEGIN
        SELECT 0 AS success, 'Seleccione un proveedor activo.' AS message, CAST(NULL AS INT) AS id_supplier;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE id_brand = @id_brand AND deleted_at IS NULL AND status = 1)
    BEGIN
        SELECT 0 AS success, 'Seleccione una marca activa.' AS message, CAST(NULL AS INT) AS id_supplier;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.SupplierBrands WHERE id_supplier = @id_supplier AND id_brand = @id_brand)
    BEGIN
        SELECT 0 AS success, 'Esta marca ya esta asignada al proveedor.' AS message, CAST(NULL AS INT) AS id_supplier;
        RETURN;
    END

    INSERT INTO dbo.SupplierBrands (id_supplier, id_brand)
    VALUES (@id_supplier, @id_brand);

    SELECT 1 AS success, 'Marca asignada al proveedor correctamente.' AS message, @id_supplier AS id_supplier;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_supplier_brand_delete
    @id_supplier INT,
    @id_brand INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.SupplierBrands
    WHERE id_supplier = @id_supplier
      AND id_brand = @id_brand;

    SELECT 1 AS success, 'Asignacion eliminada correctamente.' AS message, @id_supplier AS id_supplier;
END
GO

INSERT INTO dbo.SupplierBrands (id_supplier, id_brand)
SELECT DISTINCT
    ps.id_supplier,
    p.id_brand
FROM dbo.ProductSuppliers ps
INNER JOIN dbo.Products p ON p.id_product = ps.id_product
INNER JOIN dbo.Suppliers s ON s.id_supplier = ps.id_supplier
INNER JOIN dbo.Brands b ON b.id_brand = p.id_brand
WHERE ps.status = 1
  AND p.deleted_at IS NULL
  AND p.status = 1
  AND s.deleted_at IS NULL
  AND s.status = 1
  AND b.deleted_at IS NULL
  AND b.status = 1
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.SupplierBrands sb
        WHERE sb.id_supplier = ps.id_supplier
          AND sb.id_brand = p.id_brand
      );
GO

-- ==========================================
-- Opciones de filtros - Productos y tags
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_product_filter_category_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        c.id_category AS id,
        c.name AS name
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON c.id_category = p.id_category
    WHERE p.deleted_at IS NULL
      AND p.status = 1
      AND c.deleted_at IS NULL
      AND c.status = 1
    ORDER BY c.name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_filter_brand_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        b.id_brand AS id,
        b.name AS name
    FROM dbo.Products p
    INNER JOIN dbo.Brands b ON b.id_brand = p.id_brand
    WHERE p.deleted_at IS NULL
      AND p.status = 1
      AND b.deleted_at IS NULL
      AND b.status = 1
    ORDER BY b.name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_supplier_filter_product_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        p.id_product AS id,
        p.name AS name
    FROM dbo.ProductSuppliers ps
    INNER JOIN dbo.Products p ON p.id_product = ps.id_product
    INNER JOIN dbo.Suppliers s ON s.id_supplier = ps.id_supplier
    WHERE ps.status = 1
      AND p.deleted_at IS NULL
      AND p.status = 1
      AND s.deleted_at IS NULL
      AND s.status = 1
    ORDER BY p.name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_product_supplier_filter_supplier_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        s.id_supplier AS id,
        s.name AS name
    FROM dbo.ProductSuppliers ps
    INNER JOIN dbo.Products p ON p.id_product = ps.id_product
    INNER JOIN dbo.Suppliers s ON s.id_supplier = ps.id_supplier
    WHERE ps.status = 1
      AND p.deleted_at IS NULL
      AND p.status = 1
      AND s.deleted_at IS NULL
      AND s.status = 1
    ORDER BY s.name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_supplier_brand_filter_brand_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        b.id_brand AS id,
        b.name AS name
    FROM dbo.SupplierBrands sb
    INNER JOIN dbo.Suppliers s ON s.id_supplier = sb.id_supplier
    INNER JOIN dbo.Brands b ON b.id_brand = sb.id_brand
    WHERE s.deleted_at IS NULL
      AND s.status = 1
      AND b.deleted_at IS NULL
      AND b.status = 1
    ORDER BY b.name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_supplier_brand_filter_supplier_options
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        s.id_supplier AS id,
        s.name AS name
    FROM dbo.SupplierBrands sb
    INNER JOIN dbo.Suppliers s ON s.id_supplier = sb.id_supplier
    INNER JOIN dbo.Brands b ON b.id_brand = sb.id_brand
    WHERE s.deleted_at IS NULL
      AND s.status = 1
      AND b.deleted_at IS NULL
      AND b.status = 1
    ORDER BY s.name;
END
GO
