# Nomenclatura del proyecto KMLLogistics

Convenciones de nombres para mantener el código uniforme entre el equipo.

---

## Stored Procedures (SQL Server)

### Patrón general

```
sp_{entidad}_{accion}_{detalle_opcional}
```

| Parte              | Descripción                                      | Ejemplo        |
|--------------------|--------------------------------------------------|----------------|
| `sp_`              | Prefijo fijo                                     | `sp_`          |
| `{entidad}`        | Tabla en singular, snake_case                    | `user`, `stock_alert`, `role` |
| `{accion}`         | Verbo: get, create, list, check, resend, count   | `list_active`  |
| `{detalle}`        | Campo o contexto opcional                        | `by_username`, `products_filter` |

### Ejemplos actuales

#### Usuarios y roles (`SQL/Login and Register - SP.sql`)

| SP                         | Descripción                              |
|----------------------------|------------------------------------------|
| `sp_user_get_by_username`  | Obtiene usuario por nombre (login)       |
| `sp_user_create`           | Registra un nuevo usuario                |
| `sp_role_list_active`      | Lista roles activos para el combo Register |

#### Alertas de stock (`SQL/Alertas.sql`)

| SP                                      | Descripción                                      |
|-----------------------------------------|--------------------------------------------------|
| `sp_stock_alert_check`                  | Evalúa stock tras un movimiento (crea/resuelve)  |
| `sp_stock_alert_list_active`            | Lista alertas con buscador y filtros             |
| `sp_stock_alert_list_products_filter`   | Productos para combo de filtro                   |
| `sp_stock_alert_list_warehouses_filter` | Almacenes para combo de filtro                   |
| `sp_stock_alert_resend`                 | Reenvío manual de alerta por admin               |
| `sp_stock_alert_count_active`           | Contador para la campana del header              |

### Reglas para nuevos SP

1. Usar **snake_case** en minúsculas.
2. Entidad en **singular** (`user`, no `users`; `stock_alert`, no `stock_alerts`).
3. Incluir `list_active` cuando la consulta devuelva solo registros activos/no eliminados.
4. Parámetros con `@` y snake_case: `@id_user`, `@password_hash`, `@id_stock_alert`.
5. Siempre anteponer `dbo.` al ejecutar desde C#: `EXEC dbo.sp_user_get_by_username`.
6. Documentar cada SP con comentario de cabecera en el archivo `.sql`.

### Plantilla de nuevo SP

```sql
-- ============================================================
-- sp_producto_list_active
-- Lista productos activos para el módulo Productos
-- ============================================================
IF OBJECT_ID('dbo.sp_producto_list_active', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_producto_list_active;
GO

CREATE PROCEDURE dbo.sp_producto_list_active
AS
BEGIN
    SET NOCOUNT ON;
    -- ...
END
GO
```

---

## Tablas SQL

| Regla              | Ejemplo                                      |
|--------------------|----------------------------------------------|
| PascalCase plural  | `Users`, `WarehouseDetails`, `StockAlerts`   |
| PK                 | `id_{tabla_singular}` → `id_user`, `id_product` |
| FK                 | `id_{tabla_referenciada}`                    |
| Fechas auditoría   | `created_at`, `updated_at`, `deleted_at`     |
| Soft delete        | `deleted_at IS NULL` en consultas            |
| Checks             | `chk_{tabla}_{campo}`                        |
| FK constraints     | `fk_{tabla_origen}_{tabla_destino}`          |

---

## C# — Namespaces y clases

### Namespaces

```
E1___Sosa_Morales.Controllers.{Modulo}
E1___Sosa_Morales.Models.{Modulo}
E1___Sosa_Morales.Services.{Modulo}
E1___Sosa_Morales.Config
E1___Sosa_Morales.Data
```

### Clases

| Tipo            | Patrón                    | Ejemplo                    |
|-----------------|---------------------------|----------------------------|
| Controller      | `{Modulo}Controller`      | `CountriesController`      |
| ViewModel       | `{Modulo}ViewModel`       | `CountriesViewModel`       |
| Entidad / DTO SP| Nombre descriptivo        | `StockAlertItem`, `User`   |
| Service         | `{Dominio}Service`        | `StockAlertService`        |
| Interface       | `I{Dominio}Service`       | `IStockAlertService`       |
| ViewComponent   | `{Nombre}ViewComponent`   | `StockAlertCountViewComponent` |

### Propiedades C# ↔ columnas SQL

Las propiedades usan **PascalCase** y se mapean a columnas SQL con `[Column]`:

```csharp
[Column("id_user")]
public int IdUser { get; set; }

[Column("warehouse_name")]
public string WarehouseName { get; set; }
```

---

## Vistas Razor

| Elemento     | Convención                          |
|--------------|-------------------------------------|
| Carpeta      | Igual al Controller sin sufijo      |
| Vista principal | Siempre `Index.cshtml`           |
| Layout propio   | `_Layout.cshtml` dentro de la carpeta (Login, Register) |
| Partials compartidos | `_NombrePartial.cshtml` en `Views/Shared/` |

---

## Archivos estáticos (CSS / JS)

| Elemento   | Convención                                      |
|------------|-------------------------------------------------|
| Carpeta CSS/JS | PascalCase, igual que el módulo (`AlertasStock`) |
| Archivo    | kebab-case en minúsculas                        |
| Ruta web   | `~/Public/CSS/{Modulo}/{archivo}.css`           |

**Ejemplos:**

| Módulo              | CSS                              | JS                               |
|---------------------|----------------------------------|----------------------------------|
| TiposDocumento      | `tipos-documento.css`            | `tipos-documento.js`             |
| ListaTransferencias | `lista-transferencias.css`       | `lista-transferencias.js`        |
| AlertasStock        | `alertas-stock.css`              | `alertas-stock.js`               |
| Countries           | `countries.css`                  | `countries.js`                   |

---

## Rutas URL

| Pantalla        | Ruta                  |
|-----------------|-----------------------|
| Landing         | `/` o `/LandingPage`  |
| Login           | `/Login`              |
| Logout          | `/Login/Logout` (POST)|
| Register        | `/Register`           |
| Dashboard       | `/Dashboard`          |
| Alertas stock   | `/AlertasStock`       |
| Módulo genérico | `/{NombreModulo}`     |

ASP.NET Core usa el nombre del controller sin el sufijo `Controller`.

---

## Archivos SQL del repositorio

| Archivo                      | Contenido                              |
|------------------------------|----------------------------------------|
| `SQL/Tablas Completas.sql`   | CREATE TABLE de todo el esquema        |
| `SQL/Login and Register - SP.sql` | SPs de autenticación              |
| `SQL/Alertas.sql`            | Tabla StockAlerts, ALTER, SPs alertas  |
| `SQL/{Modulo} - SP.sql`      | Patrón sugerido para nuevos módulos    |

---

## Git y commits (sugerido)

```
feat: descripción corta en imperativo

- Detalle de cambios si aplica
- Módulo o área afectada
```

Prefijos: `feat`, `fix`, `docs`, `refactor`, `sql`.
