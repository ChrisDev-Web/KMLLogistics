# Tarjeta Recursos Humanos

Documentación del módulo **Recursos Humanos** (RRHH) del dashboard KMLLogistics: Cargos, Empleados, estructura, nomenclatura y base de datos.

---

## Resumen

La tarjeta **Recursos Humanos** agrupa dos módulos CRUD sobre las tablas `JobPositions` y `Employees`. Cada uno tiene **archivos propios**.

| Módulo    | Ruta URL    | Tabla SQL       | Estado   |
|-----------|-------------|-----------------|----------|
| Cargos    | `/Cargos`   | `JobPositions`  | Completo |
| Empleados | `/Empleados`| `Employees`     | Completo |

Las pestañas se definen en `Config/ModuleRegistry.cs` bajo la clave `"Organizacion"`. La tarjeta del dashboard abre por defecto `/Empleados`.

---

## Funcionalidades implementadas

### Cargos — CRUD completo

- Listar activos con paginación (10 / 20 / 50) y buscador AJAX.
- **Ver inactivos** en modal.
- Crear, editar, ver detalle.
- Eliminación lógica, restaurar y eliminación física (solo inactivos).
- Campos: `name`, `description`.

### Empleados — CRUD completo

- Listar activos con paginación y buscador AJAX.
- Filtros por **cargo**, **tipo de documento** y **distrito**.
- **Ver inactivos** en modal.
- Crear, editar, ver detalle.
- Eliminación lógica, restaurar y eliminación física.
- Vinculación obligatoria con un **usuario** del sistema (`Users`) y un **cargo** (`JobPositions`).
- Combo de usuarios disponibles vía `sp_employee_user_list_available` (usuarios sin empleado asignado).

Campos principales: datos personales, documento, contacto, dirección, distrito, cargo, usuario vinculado.

---

## Ubicación SQL

### Tablas (DDL)

| Archivo | Ubicación |
|---------|-----------|
| Definición de tablas | `SQL/Tablas Completas.sql` |

Secciones relevantes: `JobPositions`, `Employees` (FK a `Users`, `JobPositions`, `DocumentTypes`, `Districts`).

### Stored Procedures

| Archivo | Contenido |
|---------|-----------|
| `SQL/Neil.sql` | SPs CRUD de Cargos y Empleados |

Secciones dentro de `Neil.sql`:

| Sección       | Línea aprox. |
|---------------|--------------|
| JOB POSITIONS | ~621         |
| EMPLOYEES     | ~809         |

---

## Stored Procedures

### Cargos (`job_position`)

| SP | Descripción |
|----|-------------|
| `sp_job_position_list_active` | Listado paginado de activos |
| `sp_job_position_list_inactive` | Listado paginado de inactivos |
| `sp_job_position_get_by_id` | Detalle por ID |
| `sp_job_position_create` | Crear |
| `sp_job_position_update` | Actualizar |
| `sp_job_position_delete_logic` | Desactivar |
| `sp_job_position_restore` | Restaurar |
| `sp_job_position_delete_physical` | Eliminar permanente |

### Empleados (`employee`)

| SP | Descripción |
|----|-------------|
| `sp_employee_job_position_list_active` | Cargos para combo |
| `sp_employee_document_type_list_active` | Tipos de documento para combo |
| `sp_employee_district_list_active` | Distritos para combo |
| `sp_employee_user_list_available` | Usuarios sin empleado asignado |
| `sp_employee_list_active` | Listado paginado de activos |
| `sp_employee_list_inactive` | Listado paginado de inactivos |
| `sp_employee_get_by_id` | Detalle por ID |
| `sp_employee_create` | Crear |
| `sp_employee_update` | Actualizar |
| `sp_employee_delete_logic` | Desactivar |
| `sp_employee_restore` | Restaurar |
| `sp_employee_delete_physical` | Eliminar permanente |

---

## Estructura de archivos

### Cargos

```
Controllers/Cargos/CargosController.cs
Models/Cargos/
├── JobPositionDtos.cs
└── CargosViewModel.cs
Services/Cargos/
├── IJobPositionService.cs
└── JobPositionService.cs
Views/Cargos/Index.cshtml
wwwroot/Public/CSS/Cargos/cargos.css
wwwroot/Public/JS/Cargos/cargos.js
```

### Empleados

```
Controllers/Empleados/EmpleadosController.cs
Models/Empleados/
├── EmployeeDtos.cs
└── EmpleadosViewModel.cs
Services/Empleados/
├── IEmployeeService.cs
└── EmployeeService.cs
Views/Empleados/Index.cshtml
wwwroot/Public/CSS/Empleados/empleados.css
wwwroot/Public/JS/Empleados/empleados.js
```

### Registro en `Program.cs`

```csharp
builder.Services.AddScoped<IJobPositionService, JobPositionService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
```

---

## Nomenclatura

### Prefijos CSS (BEM)

| Módulo    | Prefijo CSS | Clase raíz |
|-----------|-------------|------------|
| Cargos    | `cgo-`      | `.cgo`     |
| Empleados | `emp-`      | `.emp`     |

### Variables JavaScript globales

| Módulo    | Variable global   |
|-----------|-------------------|
| Cargos    | `window.cgoUrls`  |
| Empleados | `window.empUrls`  |

---

## Endpoints del controller

### CargosController

| Método | Acción           | Uso                 |
|--------|------------------|---------------------|
| GET    | `Index`          | Vista principal     |
| GET    | `List`           | Grid activos        |
| GET    | `ListInactive`   | Modal inactivos     |
| GET    | `Get`            | Detalle / editar    |
| POST   | `Create`         | Crear               |
| POST   | `Update`         | Editar              |
| POST   | `DeleteLogic`    | Desactivar          |
| POST   | `Restore`        | Restaurar           |
| POST   | `DeletePhysical` | Eliminar permanente |

### EmpleadosController

Incluye los endpoints anteriores más:

| Método | Acción        | Uso                                    |
|--------|---------------|----------------------------------------|
| GET    | `FilterOptions` | Combos de cargo, documento y distrito |
| GET    | `UserOptions`   | Usuarios disponibles para vincular   |

---

## Relación con otros módulos

- **Seguridad / Usuarios:** cada empleado debe estar vinculado a un registro en `Users`.
- **Compras y Transferencias:** el empleado registrado en la operación se toma de `Employees` (FK `id_employee`).
- **Configuración:** tipos de documento y distritos provienen de los módulos geográficos.

---

## Orden de ejecución SQL recomendado

```
1. SQL/Tablas Completas.sql
2. SQL/Login and Register - SP.sql
3. SQL/Espinoza.sql          ← geografía, roles, usuarios
4. SQL/Neil.sql              ← Cargos y Empleados
```

---

## Referencias

- Tarjeta Seguridad (usuarios): `Documentation/Tarjeta Seguridad.md`
- Tarjeta Configuración (geografía): `Documentation/Tarjeta Configuracion.md`
- Estructura general: `Documentation/Estructura.md`
- Convenciones de nombres: `Documentation/Nomenclatura.md`
- Conexión a base de datos: `Documentation/Database.md`
