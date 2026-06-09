# Tarjeta Personas y Terceros

Documentación del módulo **Personas y Terceros** del dashboard KMLLogistics: Clientes, Proveedores, estructura, nomenclatura y base de datos.

---

## Resumen

La tarjeta **Personas y Terceros** agrupa dos módulos CRUD sobre las tablas `Clients` y `Suppliers`. Cada uno tiene **archivos propios**.

| Módulo     | Ruta URL      | Tabla SQL   | Estado   |
|------------|---------------|-------------|----------|
| Clientes   | `/Clientes`   | `Clients`   | Completo |
| Proveedores| `/Proveedores`| `Suppliers` | Completo |

Las pestañas se definen en `Config/ModuleRegistry.cs` bajo la clave `"PersonasTerceros"`. La tarjeta del dashboard abre por defecto `/Clientes`.

---

## Funcionalidades implementadas

Ambos módulos comparten el mismo comportamiento CRUD:

- Listar registros **activos** con paginación (10 / 20 / 50).
- Buscador AJAX en tiempo real.
- Filtros por **tipo de documento** y **distrito**.
- **Ver inactivos** en modal con paginación y búsqueda propias.
- Crear, editar y ver detalle (modal).
- **Eliminación lógica**, **restaurar** y **eliminación física** (solo inactivos).
- En detalle se muestra la **jerarquía geográfica** (país → región → provincia → distrito) vía `sp_district_geography_get_by_id`.

### Campos principales

**Clientes:** tipo y número de documento, nombres, apellidos, teléfono, email, dirección, distrito.

**Proveedores:** tipo y número de documento (RUC), razón social, contacto, teléfono, email, dirección, distrito.

---

## Ubicación SQL

### Tablas (DDL)

| Archivo | Ubicación |
|---------|-----------|
| Definición de tablas | `SQL/Tablas Completas.sql` |

Secciones relevantes: `Clients`, `Suppliers` (con FK a `DocumentTypes` y `Districts`).

### Stored Procedures

| Archivo | Contenido |
|---------|-----------|
| `SQL/Neil.sql` | SPs CRUD de Clientes y Proveedores |

Secciones dentro de `Neil.sql`:

| Sección   | Línea aprox. |
|-----------|--------------|
| CLIENTS   | ~10          |
| SUPPLIERS | ~308         |
| GEOGRAPHY | ~589         |

---

## Stored Procedures

Patrón: `sp_{entidad_singular}_{funcion}`

### Clientes (`client`)

| SP | Descripción |
|----|-------------|
| `sp_client_document_type_list_active` | Tipos de documento para combo |
| `sp_client_district_list_active` | Distritos para combo |
| `sp_client_list_active` | Listado paginado de activos |
| `sp_client_list_inactive` | Listado paginado de inactivos |
| `sp_client_get_by_id` | Detalle por ID (incluye geografía) |
| `sp_client_create` | Crear |
| `sp_client_update` | Actualizar |
| `sp_client_delete_logic` | Desactivar |
| `sp_client_restore` | Restaurar |
| `sp_client_delete_physical` | Eliminar permanente |

### Proveedores (`supplier`)

| SP | Descripción |
|----|-------------|
| `sp_supplier_document_type_list_active` | Tipos de documento para combo |
| `sp_supplier_district_list_active` | Distritos para combo |
| `sp_supplier_list_active` | Listado paginado de activos |
| `sp_supplier_list_inactive` | Listado paginado de inactivos |
| `sp_supplier_get_by_id` | Detalle por ID |
| `sp_supplier_create` | Crear |
| `sp_supplier_update` | Actualizar |
| `sp_supplier_delete_logic` | Desactivar |
| `sp_supplier_restore` | Restaurar |
| `sp_supplier_delete_physical` | Eliminar permanente |

### Geografía compartida

| SP | Descripción |
|----|-------------|
| `sp_district_geography_get_by_id` | Devuelve país, región, provincia y distrito para mostrar en detalle |

---

## Estructura de archivos

### Clientes

```
Controllers/Clientes/ClientesController.cs
Models/Clientes/
├── ClientDtos.cs
└── ClientesViewModel.cs
Services/Clientes/
├── IClientService.cs
└── ClientService.cs
Views/Clientes/Index.cshtml
wwwroot/Public/CSS/Clientes/clientes.css
wwwroot/Public/JS/Clientes/clientes.js
```

### Proveedores

```
Controllers/Proveedores/ProveedoresController.cs
Models/Proveedores/
├── SupplierDtos.cs
└── ProveedoresViewModel.cs
Services/Proveedores/
├── ISupplierService.cs
└── SupplierService.cs
Views/Proveedores/Index.cshtml
wwwroot/Public/CSS/Proveedores/proveedores.css
wwwroot/Public/JS/Proveedores/proveedores.js
```

### Registro en `Program.cs`

```csharp
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
```

---

## Nomenclatura

### Prefijos CSS (BEM)

| Módulo     | Prefijo CSS | Clase raíz |
|------------|-------------|------------|
| Clientes   | `clt-`      | `.clt`     |
| Proveedores| `sup-`      | `.sup`     |

### Variables JavaScript globales

| Módulo     | Variable global   |
|------------|-------------------|
| Clientes   | `window.cltUrls`  |
| Proveedores| `window.supUrls`  |

---

## Endpoints del controller

Patrón común en ambos controllers:

| Método | Acción           | Uso                          |
|--------|------------------|------------------------------|
| GET    | `List`           | Grid de activos              |
| GET    | `ListInactive`   | Modal de inactivos           |
| GET    | `Get`            | Detalle / cargar formulario  |
| GET    | `FilterOptions`  | Combos de documento y distrito |
| POST   | `Create`         | Crear registro               |
| POST   | `Update`         | Editar registro              |
| POST   | `DeleteLogic`    | Desactivar                   |
| POST   | `Restore`        | Restaurar inactivo           |
| POST   | `DeletePhysical` | Eliminar permanente          |

---

## Orden de ejecución SQL recomendado

```
1. SQL/Tablas Completas.sql
2. SQL/Login and Register - SP.sql
3. SQL/Espinoza.sql          ← geografía y tipos de documento
4. SQL/Neil.sql              ← Clientes y Proveedores
```

---

## Referencias

- Tarjeta Configuración (geografía padre): `Documentation/Tarjeta Configuracion.md`
- Estructura general: `Documentation/Estructura.md`
- Convenciones de nombres: `Documentation/Nomenclatura.md`
- Conexión a base de datos: `Documentation/Database.md`
