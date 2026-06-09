# Tarjeta Inventario

Documentación del módulo **Inventario** del dashboard KMLLogistics: Almacenes, Detalle de almacén, Tipos de movimiento, Movimientos de inventario, estructura y base de datos.

---

## Resumen

La tarjeta **Inventario** agrupa cuatro módulos sobre las tablas `Warehouses`, `WarehouseDetails`, `MovementTypes` e `InventoryMovements`.

| Módulo                    | Ruta URL                 | Tabla SQL            | Estado          |
|---------------------------|--------------------------|----------------------|-----------------|
| Almacenes                 | `/Almacenes`             | `Warehouses`         | Completo        |
| Detalle de almacén        | `/DetalleAlmacen`        | `WarehouseDetails`   | Consulta only   |
| Tipos de movimiento       | `/TiposMovimiento`       | `MovementTypes`      | Completo        |
| Movimientos de inventario | `/MovimientosInventario` | `InventoryMovements` | Consulta only   |

Las pestañas se definen en `Config/ModuleRegistry.cs` bajo la clave `"Inventario"`. La tarjeta del dashboard abre por defecto `/Almacenes`.

---

## Funcionalidades implementadas

### Almacenes — CRUD completo

- Listar activos con paginación (10 / 20 / 50) y buscador AJAX.
- **Ver inactivos** en modal.
- Crear, editar, ver detalle.
- Eliminación lógica, restaurar y eliminación física.
- Campos: nombre, dirección, distrito (FK).

### Tipos de movimiento — CRUD completo

- Mismo patrón CRUD que Almacenes.
- Catálogo de tipos usados en el kardex (`Entrada por compra`, `Salida por transferencia`, etc.).
- Campos: `name`, `description`.

### Detalle de almacén — solo consulta

- Vista maestro-detalle: lista de almacenes y, al seleccionar uno, métricas y productos con stock.
- Modal de detalle por producto-almacén (precio, stock, fechas).
- **No** permite editar stock manualmente desde la UI.

### Movimientos de inventario — solo consulta

- Listado paginado del kardex (historial de entradas/salidas).
- Filtros por producto, almacén, tipo de movimiento y empleado.
- Modal de detalle de cada movimiento.

---

## Lógica de negocio — stock e inventario

El stock en `WarehouseDetails` **no se edita directamente** desde estos módulos. Se actualiza automáticamente desde:

| Origen              | Acción                         | Efecto en stock                          |
|---------------------|--------------------------------|------------------------------------------|
| Compras             | Completar compra               | Entrada por almacén                      |
| Compras             | Cancelar compra completada     | Reversión (salida)                       |
| Transferencias      | Crear transferencia            | Salida origen + entrada destino          |
| Transferencias      | Cancelar transferencia         | Reversión en ambos almacenes             |

Cada movimiento genera un registro en `InventoryMovements` con referencia (`COM-{id}`, `TRF-{id}`, etc.).

---

## Ubicación SQL

### Tablas (DDL)

| Archivo | Ubicación |
|---------|-----------|
| Definición de tablas | `SQL/Tablas Completas.sql` |

Secciones relevantes: `Warehouses`, `WarehouseDetails`, `MovementTypes`, `InventoryMovements`.

### Stored Procedures

| Archivo | Contenido |
|---------|-----------|
| `SQL/Sergio.sql` | SPs de Inventario |

Secciones dentro de `Sergio.sql`:

| Sección              | Línea aprox. |
|----------------------|--------------|
| WAREHOUSES           | ~10          |
| MOVEMENT TYPES       | ~216         |
| WAREHOUSE DETAILS    | ~358         |
| INVENTORY MOVEMENTS  | ~527         |

Los tipos de movimiento de compras y transferencias se insertan también en las secciones de seed de `Sergio.sql` y `Neil.sql`.

---

## Stored Procedures

### Almacenes (`warehouse`)

| SP | Descripción |
|----|-------------|
| `sp_warehouse_district_list_active` | Distritos para combo |
| `sp_warehouse_list_active` | Listado paginado de activos |
| `sp_warehouse_list_inactive` | Listado paginado de inactivos |
| `sp_warehouse_get_by_id` | Detalle por ID |
| `sp_warehouse_create` | Crear |
| `sp_warehouse_update` | Actualizar |
| `sp_warehouse_delete_logic` | Desactivar |
| `sp_warehouse_restore` | Restaurar |
| `sp_warehouse_delete_physical` | Eliminar permanente |

### Tipos de movimiento (`movement_type`)

| SP | Descripción |
|----|-------------|
| `sp_movement_type_list_active` | Listado paginado de activos |
| `sp_movement_type_list_inactive` | Listado paginado de inactivos |
| `sp_movement_type_get_by_id` | Detalle por ID |
| `sp_movement_type_create` | Crear |
| `sp_movement_type_update` | Actualizar |
| `sp_movement_type_delete_logic` | Desactivar |
| `sp_movement_type_restore` | Restaurar |
| `sp_movement_type_delete_physical` | Eliminar permanente |

### Detalle de almacén (`warehouse_detail`) — consulta

| SP | Descripción |
|----|-------------|
| `sp_warehouse_detail_list` | Resumen por almacén |
| `sp_warehouse_detail_list_products` | Productos y stock de un almacén |
| `sp_warehouse_detail_metrics` | Métricas del almacén seleccionado |
| `sp_warehouse_detail_get_warehouse` | Cabecera del almacén |
| `sp_warehouse_detail_get_by_id` | Detalle producto-almacén |
| `sp_warehouse_detail_warehouse_list_active` | Almacenes para filtros |

### Movimientos de inventario (`inventory_movement`) — consulta

| SP | Descripción |
|----|-------------|
| `sp_inventory_movement_list` | Listado paginado |
| `sp_inventory_movement_get_by_id` | Detalle por ID |
| `sp_inventory_movement_filter_options` | Combos de filtros |

---

## Estructura de archivos

| Módulo                | Controller              | Service                         | CSS                         | JS                           |
|-----------------------|-------------------------|---------------------------------|-----------------------------|------------------------------|
| Almacenes             | `AlmacenesController`   | `Services/Almacenes/`           | `almacenes.css`             | `almacenes.js`               |
| DetalleAlmacen        | `DetalleAlmacenController` | `Services/DetalleAlmacen/`  | `detalle-almacen.css`       | `detalle-almacen.js`         |
| TiposMovimiento       | `TiposMovimientoController` | `Services/TiposMovimiento/` | `tipos-movimiento.css`      | `tipos-movimiento.js`        |
| MovimientosInventario | `MovimientosInventarioController` | `Services/MovimientosInventario/` | `movimientos-inventario.css` | `movimientos-inventario.js` |

### Registro en `Program.cs`

```csharp
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IWarehouseDetailService, WarehouseDetailService>();
builder.Services.AddScoped<IMovementTypeService, MovementTypeService>();
builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();
builder.Services.AddScoped<IStockAlertService, StockAlertService>();  // Alertas.sql
```

---

## Nomenclatura

### Prefijos CSS (BEM)

| Módulo                | Prefijo CSS | Variable global      |
|-----------------------|-------------|----------------------|
| Almacenes             | `alm-`      | `window.almUrls`     |
| DetalleAlmacen        | `dtalm-`    | `window.dtalmUrls`   |
| TiposMovimiento       | `tmov-`     | `window.tmovUrls`    |
| MovimientosInventario | `movinv-`   | `window.movinvUrls`  |

---

## Endpoints del controller

### AlmacenesController / TiposMovimientoController

Patrón CRUD completo: `List`, `ListInactive`, `Get`, `Create`, `Update`, `DeleteLogic`, `Restore`, `DeletePhysical`.

Almacenes incluye además `DistrictOptions`.

### DetalleAlmacenController

| Método | Acción            | Uso                              |
|--------|-------------------|----------------------------------|
| GET    | `List`            | Grid de almacenes                |
| GET    | `Metrics`         | Métricas del almacén seleccionado|
| GET    | `ListProducts`    | Productos con stock              |
| GET    | `GetWarehouse`    | Cabecera del almacén             |
| GET    | `Get`             | Detalle producto-almacén         |
| GET    | `WarehouseOptions`| Combo de almacenes               |

### MovimientosInventarioController

| Método | Acción          | Uso                    |
|--------|-----------------|------------------------|
| GET    | `List`          | Grid del kardex        |
| GET    | `Get`           | Detalle del movimiento |
| GET    | `FilterOptions` | Combos de filtros      |

---

## Módulo relacionado: Alertas de stock

Fuera de la tarjeta Inventario pero vinculado al stock:

- Ruta: `/AlertasStock`
- SQL: `SQL/Alertas.sql`
- Servicio: `IStockAlertService` / `StockAlertService`
- Badge en el header del dashboard vía `StockAlertCountViewComponent`

---

## Orden de ejecución SQL recomendado

```
1. SQL/Tablas Completas.sql
2. SQL/Login and Register - SP.sql
3. SQL/Espinoza.sql          ← distritos (FK de almacenes)
4. SQL/Sergio.sql            ← Inventario completo
5. SQL/Alertas.sql           ← alertas de stock
```

---

## Referencias

- Tarjeta Compras (entradas de stock): `Documentation/Tarjeta Compras.md`
- Tarjeta Transferencias (movimientos entre almacenes): `Documentation/Tarjeta Transferencias.md`
- Estructura general: `Documentation/Estructura.md`
- Convenciones de nombres: `Documentation/Nomenclatura.md`
- Conexión a base de datos: `Documentation/Database.md`
