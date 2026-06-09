-- Productos nuevos - KMLLogistics
USE KMLLogistics;
GO
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @nuevo TABLE (name NVARCHAR(100) NOT NULL);

INSERT INTO @nuevo (name) VALUES
(N'Google Pixel 8 Pro 256GB'),
(N'Huawei P60 Pro 256GB'),
(N'Honor Magic6 Lite 256GB'),
(N'Vivo V30 5G 256GB'),
(N'ZTE Blade V50 Design'),
(N'Samsung Galaxy A15 4G 128GB'),
(N'iPhone 13 128GB'),
(N'Cargador Google 30W USB-C'),
(N'Funda Samsung Galaxy S24'),
(N'Protector Xiaomi 14 Ultra'),
(N'Cable USB-C a Lightning 1m'),
(N'Auricular Samsung Galaxy Buds FE');

INSERT INTO Products (id_category, id_brand, name, description, cost, profit_percentage, weight)
SELECT v.id_category, v.id_brand, v.name, v.description, v.cost, v.profit_percentage, v.weight
FROM (VALUES
    (4, 14, N'Google Pixel 8 Pro 256GB',       N'Celular gama alta Google',           3400.00, 28.00, 0.21),
    (4,  5, N'Huawei P60 Pro 256GB',           N'Celular gama alta Huawei',           2900.00, 27.00, 0.20),
    (5,  6, N'Honor Magic6 Lite 256GB',         N'Celular gama media Honor',            920.00, 25.00, 0.19),
    (5,  9, N'Vivo V30 5G 256GB',              N'Celular gama media Vivo',             880.00, 25.00, 0.19),
    (6, 11, N'ZTE Blade V50 Design',           N'Celular gama baja ZTE',               380.00, 22.00, 0.18),
    (6,  1, N'Samsung Galaxy A15 4G 128GB',    N'Celular gama baja Samsung',           420.00, 22.00, 0.18),
    (3,  2, N'iPhone 13 128GB',                N'iPhone modelo anterior',             2400.00, 25.00, 0.17),
    (8, 14, N'Cargador Google 30W USB-C',       N'Cargador rapido USB-C',                48.00, 35.00, 0.08),
    (9,  1, N'Funda Samsung Galaxy S24',       N'Funda silicona Galaxy S24',             22.00, 40.00, 0.05),
    (10, 3, N'Protector Xiaomi 14 Ultra',       N'Protector templado Xiaomi 14 Ultra',   14.00, 45.00, 0.02),
    (7,  2, N'Cable USB-C a Lightning 1m',     N'Cable original compatible Apple',        35.00, 38.00, 0.04),
    (7,  1, N'Auricular Samsung Galaxy Buds FE', N'Auriculares inalambricos Samsung',    180.00, 30.00, 0.06)
) AS v(id_category, id_brand, name, description, cost, profit_percentage, weight)
WHERE NOT EXISTS (
    SELECT 1 FROM Products p
    WHERE p.name = v.name AND p.deleted_at IS NULL
);

SELECT name FROM @nuevo n
WHERE EXISTS (
    SELECT 1 FROM Products p
    WHERE p.name = n.name AND p.deleted_at IS NULL
)
ORDER BY name;
GO
