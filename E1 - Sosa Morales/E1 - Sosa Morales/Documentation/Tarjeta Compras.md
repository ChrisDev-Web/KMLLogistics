# Tarjeta Compras

Documentación del módulo **Compras** del dashboard KMLLogistics: Estados de compra, Órdenes de compra, Detalle de compra, Detalle almacén compra, estructura y base de datos.

---

## Resumen

La tarjeta **Compras** agrupa cuatro módulos sobre las tablas `PurchaseStatuses`, `Purchases`, `PurchaseDetails` y `PurchaseWarehouseDetails`.

| Módulo                 | Ruta URL                | Tabla SQL                  | Estado        |
|------------------------|-------------------------|----------------------------|---------------|
| Estados de compra      | `/EstadosCompra`        | `PurchaseStatuses`         | Completo      |
| Compras                | `/OrdenesCompra`        | `Purchases`                | Completo      |
| Detalle de compra      | `/DetalleCompra`        | `PurchaseDetails`          | Consulta only |
| Detalle almacén compra | `/DetalleAlmacenCompra` | `PurchaseWarehouseDetails` | Consulta only |

Las pestañas se definen en `Config/ModuleRegistry.cs` bajo la clave `"Compras"`. La tarjeta del dashboard abre por defecto `/OrdenesCompra`.

---

## Funcionalidades implementadas

### Estados de compra — CRUD completo

- Mismo patrón CRUD que Configuración (activos, inactivos, soft delete, restore, purge).
- Estados semilla: **Pendiente**, **Completada**, **Cancelada**.

### Compras (Órdenes de compra) — operaciones

- Listado paginado con filtros (proveedor, empleado, estado, ID compra).
- **Crear compra** con líneas de producto-proveedor, cantidad, costo unitario y almacén destino.
- **Completar compra** (Pendiente → Completada): distribuye stock a almacenes.
- **Cancelar compra** (Pendiente o Completada).
- Modal de detalle con productos y distribución planificada/ejecutada.

### Detalle de compra — solo consulta

- Listado paginado de líneas de compra (`PurchaseDetails`).
- Filtros por compra, producto, proveedor y estado de la compra.
- Modal de detalle.

### Detalle almacén compra — solo consulta

- Listado paginado de distribución por almacén (`PurchaseWarehouseDetails`).
- **Solo muestra compras en estado Completada** (stock ya distribuido).
- Se refresca al completar o cancelar una compra (`purchase:completed`, `purchase:cancelled`).

---

## Lógica de negocio — ciclo de vida de la compra

```
Crear  →  Pendiente  →  Completar  →  Completada
              │                          │
              └──── Cancelar ────────────┴── Cancelar → Cancelada
```

### 1. Crear (`sp_purchase_create`)

- Estado inicial: **Pendiente**.
- Inserta `Purchases`, `PurchaseDetails` y `PurchaseWarehouseDetails` (plan de distribución).
- Calcula subtotal, IGV (18 %) y total.
- **No** modifica `WarehouseDetails` ni crea movimientos de inventario.

### 2. Completar (`sp_purchase_complete`)

- Solo desde estado **Pendiente**.
- Por cada línea en `PurchaseWarehouseDetails`:
  - Incrementa stock en `WarehouseDetails`.
  - Inserta movimiento `Entrada por compra` (ref. `COM-{id}`).
  - Actualiza `ProductSuppliers.last_purchase_cost`.
- Cambia estado a **Completada**.

### 3. Cancelar (`sp_purchase_cancel`)

- Desde **Pendiente**: solo cambia estado a Cancelada (sin afectar inventario).
- Desde **Completada**: revierte stock (valida stock suficiente), inserta movimientos `Salida por anulación de compra` (ref. `COM-CAN-{id}`), cambia a Cancelada.

### Flags en el listado (UI)

| Flag          | Condición                          |
|---------------|------------------------------------|
| `canComplete` | Estado = Pendiente                 |
| `canCancel`   | Estado = Pendiente o Completada    |

---

## Ubicación SQL

### Tablas (DDL)

| Archivo | Ubicación |
|---------|-----------|
| Definición de tablas | `SQL/Tablas Completas.sql` |

Tablas: `PurchaseStatuses`, `Purchases`, `PurchaseDetails`, `PurchaseWarehouseDetails`, más `ProductSuppliers`, `WarehouseDetails`, `InventoryMovements`.

### Stored Procedures

| Archivo | Contenido |
|---------|-----------|
| `SQL/Sergio.sql` | SPs de Compras (desde ~646) |

Secciones dentro de `Sergio.sql`:

| Sección                        | Línea aprox. |
|--------------------------------|--------------|
| Seed estados y tipos movimiento| ~646         |
| PURCHASE STATUSES              | ~659         |
| PURCHASES - Lookups            | ~797         |
| PURCHASES - CRUD               | ~854         |
| PURCHASE DETAILS - Consulta    | ~1289        |
| PURCHASE WAREHOUSE DETAILS     | ~1377        |

---

## Stored Procedures

### Estados de compra (`purchase_status`)

Patrón CRUD: `sp_purchase_status_list_active`, `_list_inactive`, `_get_by_id`, `_create`, `_update`, `_delete_logic`, `_restore`, `_delete_physical`.

### Compras (`purchase`)

| SP | Descripción |
|----|-------------|
| `sp_purchase_list` | Listado paginado con filtros |
| `sp_purchase_get_by_id` | Cabecera de la compra |
| `sp_purchase_create` | Crear compra (estado Pendiente) |
| `sp_purchase_complete` | Completar y distribuir stock |
| `sp_purchase_cancel` | Cancelar (con o sin reversión de stock) |
| `sp_purchase_supplier_list_active` | Proveedores para combo |
| `sp_purchase_employee_list_active` | Empleados para combo |
| `sp_purchase_warehouse_list_active` | Almacenes para líneas |
| `sp_purchase_product_supplier_list_by_supplier` | Productos del proveedor |
| `sp_purchase_detail_lines_by_purchase` | Líneas para modal detalle |
| `sp_purchase_warehouse_lines_by_purchase` | Distribución para modal detalle |

### Detalle de compra (`purchase_detail`) — consulta

| SP | Descripción |
|----|-------------|
| `sp_purchase_detail_list` | Listado paginado |
| `sp_purchase_detail_get_by_id` | Detalle por ID |

### Detalle almacén compra (`purchase_warehouse_detail`) — consulta

| SP | Descripción |
|----|-------------|
| `sp_purchase_warehouse_detail_list` | Listado (solo compras Completadas) |
| `sp_purchase_warehouse_detail_get_by_id` | Detalle por ID |

---

## Estructura de archivos

| Módulo              | Prefijo CSS | Variable global     | Carpeta Service              |
|---------------------|-------------|---------------------|------------------------------|
| EstadosCompra       | `ecmp-`     | `window.ecmpUrls`   | `Services/EstadosCompra/`    |
| OrdenesCompra       | `ocmp-`     | `window.ocmpUrls`   | `Services/OrdenesCompra/`    |
| DetalleCompra       | `dcmp-`     | `window.dcmpUrls`   | `Services/DetalleCompra/`    |
| DetalleAlmacenCompra| `dacmp-`    | `window.dacmpUrls`  | `Services/DetalleAlmacenCompra/` |

```
Controllers/{EstadosCompra,OrdenesCompra,DetalleCompra,DetalleAlmacenCompra}/
Models/{EstadosCompra,OrdenesCompra,DetalleCompra,DetalleAlmacenCompra}/
Views/{EstadosCompra,OrdenesCompra,DetalleCompra,DetalleAlmacenCompra}/Index.cshtml
wwwroot/Public/CSS/{estados-compra,ordenes-compra,detalle-compra,detalle-almacen-compra}.css
wwwroot/Public/JS/{estados-compra,ordenes-compra,detalle-compra,detalle-almacen-compra}.js
```

### Registro en `Program.cs`

```csharp
builder.Services.AddScoped<IPurchaseStatusService, PurchaseStatusService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IPurchaseDetailService, PurchaseDetailService>();
builder.Services.AddScoped<IPurchaseWarehouseDetailService, PurchaseWarehouseDetailService>();
```

---

## Endpoints del controller

### EstadosCompraController

Patrón CRUD completo (igual que Configuración).

### OrdenesCompraController

| Método | Acción                  | Uso                              |
|--------|-------------------------|----------------------------------|
| GET    | `List`                  | Grid de compras                  |
| GET    | `Get`                   | Detalle con líneas y almacenes   |
| GET    | `FilterOptions`         | Combos de filtros                |
| GET    | `ProductSupplierOptions`| Productos por proveedor          |
| POST   | `Create`                | Crear compra (JSON de líneas)    |
| POST   | `Complete`              | Completar compra                 |
| POST   | `Cancel`                | Cancelar compra                  |

### DetalleCompraController / DetalleAlmacenCompraController

| Método | Acción          | Uso                    |
|--------|-----------------|------------------------|
| GET    | `List`          | Grid paginado          |
| GET    | `Get`           | Detalle por ID         |
| GET    | `FilterOptions` | Combos de filtros      |

---

## Validaciones al crear compra

- Al menos una línea con producto-proveedor, almacén, cantidad > 0 y costo válido.
- Un mismo producto-proveedor no puede tener distinto costo unitario en la misma compra.
- No repetir producto + almacén en la misma compra.
- Productos deben pertenecer al proveedor seleccionado (`ProductSuppliers`).
- Almacenes deben estar activos.

---

## Orden de ejecución SQL recomendado

```
1. SQL/Tablas Completas.sql
2. SQL/Neil.sql              ← proveedores, empleados, productos (ProductSuppliers)
3. SQL/Sergio.sql            ← Inventario + Compras
```

> Ejecutar la sección de Compras de `Sergio.sql` después de tener tablas de inventario y catálogo base.

---

## Referencias

- Tarjeta Inventario: `Documentation/Tarjeta Inventario.md`
- Tarjeta Personas y Terceros (proveedores): `Documentation/Tarjeta Personas y Terceros.md`
- Estructura general: `Documentation/Estructura.md`
- Convenciones de nombres: `Documentation/Nomenclatura.md`
- Conexión a base de datos: `Documentation/Database.md`
