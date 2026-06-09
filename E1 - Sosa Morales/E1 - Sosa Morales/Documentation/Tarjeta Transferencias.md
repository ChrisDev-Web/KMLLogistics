# Tarjeta Transferencias

Documentación del módulo **Transferencias** del dashboard KMLLogistics: Estados de transferencia, Transferencias, Detalle de transferencia, estructura y base de datos.

---

## Resumen

La tarjeta **Transferencias** agrupa tres módulos sobre las tablas `StatusTransfers`, `Transfers` y `TransferDetails`.

| Módulo                   | Ruta URL               | Tabla SQL         | Estado        |
|--------------------------|------------------------|-------------------|---------------|
| Estados de transferencia | `/EstadosTransferencia`| `StatusTransfers` | Completo      |
| Transferencias           | `/ListaTransferencias` | `Transfers`       | Completo      |
| Detalle de transferencia | `/DetalleTransferencia`| `TransferDetails` | Consulta only |

Las pestañas se definen en `Config/ModuleRegistry.cs` bajo la clave `"Transferencias"`. La tarjeta del dashboard abre por defecto `/ListaTransferencias`.

---

## Funcionalidades implementadas

### Estados de transferencia — CRUD completo

- Mismo patrón CRUD que Configuración.
- Estados semilla: **Completada**, **Cancelada**.

### Transferencias — operaciones

- Listado paginado con filtros (almacén origen/destino, estado, empleado).
- **Crear transferencia** con almacén origen, almacén destino, empleado, fecha y líneas de producto/cantidad.
- **Cancelar transferencia** (solo si está Completada).
- Modal de detalle con cabecera y líneas de productos.

### Detalle de transferencia — solo consulta

- Listado paginado de líneas (`TransferDetails`).
- Filtros por transferencia, producto, almacenes y estado.
- Modal de detalle.
- Se refresca al cancelar una transferencia (`transfer:cancelled`).

---

## Lógica de negocio — ciclo de vida de la transferencia

A diferencia de Compras, las transferencias **no tienen estado Pendiente**: al crear quedan **Completadas** de inmediato.

```
Crear  →  Completada  →  Cancelar  →  Cancelada
```

### 1. Crear (`sp_transfer_create`)

- Valida que origen ≠ destino.
- Valida stock suficiente en almacén origen.
- Por cada línea:
  - Decrementa stock en origen (`WarehouseDetails`).
  - Incrementa o inserta stock en destino.
  - Crea 2 movimientos de inventario:
    - `Salida por transferencia` en origen.
    - `Entrada por transferencia` en destino.
  - Referencia: `TRF-{id}`.
- Estado inicial: **Completada**.

### 2. Cancelar (`sp_transfer_cancel`)

- Solo desde **Completada**.
- Valida stock suficiente en almacén destino para revertir.
- Revierte stock en origen y destino.
- Crea movimientos inversos de inventario.
- Cambia estado a **Cancelada**.

### Flag en el listado (UI)

| Flag        | Condición                |
|-------------|--------------------------|
| `canCancel` | Estado = Completada      |

---

## Ubicación SQL

### Tablas (DDL)

| Archivo | Ubicación |
|---------|-----------|
| Definición de tablas | `SQL/Tablas Completas.sql` |

Tablas: `StatusTransfers`, `Transfers`, `TransferDetails`, más `WarehouseDetails`, `InventoryMovements`.

### Stored Procedures

| Archivo | Contenido |
|---------|-----------|
| `SQL/Neil.sql` | SPs de Transferencias (desde ~1227) |

Secciones dentro de `Neil.sql`:

| Sección                    | Línea aprox. |
|----------------------------|--------------|
| Seed estados y tipos mov.  | ~1227        |
| STATUS TRANSFERS           | ~1238        |
| TRANSFERS - Lookups        | ~1376        |
| TRANSFERS - CRUD           | ~1431        |
| TRANSFER DETAILS - Consulta| ~1739        |

---

## Stored Procedures

### Estados de transferencia (`status_transfer`)

Patrón CRUD: `sp_status_transfer_list_active`, `_list_inactive`, `_get_by_id`, `_create`, `_update`, `_delete_logic`, `_restore`, `_delete_physical`.

### Transferencias (`transfer`)

| SP | Descripción |
|----|-------------|
| `sp_transfer_list` | Listado paginado con filtros |
| `sp_transfer_get_by_id` | Cabecera de la transferencia |
| `sp_transfer_create` | Crear y mover stock (estado Completada) |
| `sp_transfer_cancel` | Cancelar y revertir stock |
| `sp_transfer_warehouse_list_active` | Almacenes para combos |
| `sp_transfer_employee_list_active` | Empleados para combo |
| `sp_transfer_status_list_active` | Estados para filtro |
| `sp_transfer_product_list_by_warehouse` | Productos con stock en origen |
| `sp_transfer_detail_lines_by_transfer` | Líneas para modal detalle |

### Detalle de transferencia (`transfer_detail`) — consulta

| SP | Descripción |
|----|-------------|
| `sp_transfer_detail_list` | Listado paginado |
| `sp_transfer_detail_get_by_id` | Detalle por ID |
| `sp_transfer_detail_filter_options` | Combos de filtros |

---

## Estructura de archivos

| Módulo              | Prefijo CSS | Variable global     | Carpeta Service                    |
|---------------------|-------------|---------------------|------------------------------------|
| EstadosTransferencia| `estr-`     | `window.estrUrls`   | `Services/EstadosTransferencia/`   |
| ListaTransferencias | `ltrf-`     | `window.ltrfUrls`   | `Services/ListaTransferencias/`    |
| DetalleTransferencia| `dtrf-`     | `window.dtrfUrls`   | `Services/DetalleTransferencia/`   |

```
Controllers/{EstadosTransferencia,ListaTransferencias,DetalleTransferencia}/
Models/{EstadosTransferencia,ListaTransferencias,DetalleTransferencia}/
Views/{EstadosTransferencia,ListaTransferencias,DetalleTransferencia}/Index.cshtml
wwwroot/Public/CSS/{estados-transferencia,lista-transferencias,detalle-transferencia}.css
wwwroot/Public/JS/{estados-transferencia,lista-transferencias,detalle-transferencia}.js
```

### Registro en `Program.cs`

```csharp
builder.Services.AddScoped<IStatusTransferService, StatusTransferService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<ITransferDetailService, TransferDetailService>();
```

---

## Endpoints del controller

### EstadosTransferenciaController

Patrón CRUD completo (igual que Configuración).

### ListaTransferenciasController

| Método | Acción           | Uso                                    |
|--------|------------------|----------------------------------------|
| GET    | `List`           | Grid de transferencias                 |
| GET    | `Get`            | Detalle con líneas                     |
| GET    | `FilterOptions`  | Combos de filtros                      |
| GET    | `ProductOptions` | Productos con stock en almacén origen  |
| POST   | `Create`         | Crear transferencia (JSON de líneas)   |
| POST   | `Cancel`         | Cancelar transferencia                 |

### DetalleTransferenciaController

| Método | Acción          | Uso                    |
|--------|-----------------|------------------------|
| GET    | `List`          | Grid paginado          |
| GET    | `Get`           | Detalle por ID         |
| GET    | `FilterOptions` | Combos de filtros      |

---

## Validaciones al crear transferencia

- Almacén origen y destino deben ser distintos.
- Al menos una línea con producto y cantidad > 0.
- Stock suficiente en origen para cada producto.
- Empleado y almacenes deben estar activos.

---

## Orden de ejecución SQL recomendado

```
1. SQL/Tablas Completas.sql
2. SQL/Sergio.sql            ← almacenes, tipos de movimiento, stock
3. SQL/Neil.sql              ← Transferencias (requiere inventario base)
```

---

## Referencias

- Tarjeta Inventario: `Documentation/Tarjeta Inventario.md`
- Tarjeta Compras (patrón similar de movimientos): `Documentation/Tarjeta Compras.md`
- Estructura general: `Documentation/Estructura.md`
- Convenciones de nombres: `Documentation/Nomenclatura.md`
- Conexión a base de datos: `Documentation/Database.md`
