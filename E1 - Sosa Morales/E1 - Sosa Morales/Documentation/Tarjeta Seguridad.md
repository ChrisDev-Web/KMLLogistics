# Tarjeta Seguridad

Documentación del módulo **Seguridad** del dashboard KMLLogistics: Roles, Usuarios, estructura, nomenclatura, base de datos y diferencias respecto a Configuración.

---

## Resumen

La tarjeta **Seguridad** agrupa dos módulos CRUD sobre las tablas `Roles` y `Users`. Cada uno tiene **archivos propios**.

| Módulo   | Ruta URL     | Tabla SQL | Estado   |
|----------|--------------|-----------|----------|
| Roles    | `/Roles`     | `Roles`   | Completo |
| Usuarios | `/Usuarios`  | `Users`   | Completo |

Las pestañas se definen en `Config/ModuleRegistry.cs` bajo la clave `"Seguridad"`.

---

## Funcionalidades implementadas

### Roles — CRUD completo (igual que Configuración)

- Listar activos con paginación (10 / 20 / 50) y buscador AJAX.
- **Ver inactivos** en modal.
- Crear, editar, ver detalle.
- Eliminación lógica, restaurar y eliminación física (solo inactivos).
- Campos: `name`, `description`.

### Usuarios — CRUD con restricciones

| Función                    | ¿Implementada? |
|----------------------------|----------------|
| Listar activos + paginación| Sí             |
| Buscador AJAX              | Sí             |
| Crear                      | Sí             |
| Editar                     | Sí             |
| Ver detalle                | Sí             |
| Eliminar **físico**        | Sí             |
| Ver / listar inactivos     | **No**         |
| Eliminación lógica         | **No**         |
| Restaurar                  | **No**         |

Campos del formulario:

- **Usuario** (`username`) — obligatorio.
- **Contraseña** — obligatoria al crear; opcional al editar (si se deja vacía, no se cambia).
- **Rol** — select cargado desde roles activos.

La contraseña se hashea en C# con `PasswordHasher<User>` antes de enviarse al SP (nunca se guarda en texto plano).

---

## Servicios: diferencia importante

Existen **dos servicios** relacionados con usuarios; no confundirlos:

| Servicio | Namespace | Uso |
|----------|-----------|-----|
| `UserService` | `Services/Users/` | **Login y Register** (`/Login`, `/Register`) |
| `UsuarioService` | `Services/Usuarios/` | **CRUD del módulo Usuarios** en el dashboard |

| Interfaz | Registro en `Program.cs` |
|----------|--------------------------|
| `IUserService` → `UserService` | Autenticación y registro público |
| `IUsuarioService` → `UsuarioService` | Mantenimiento de usuarios (Seguridad) |
| `IRoleService` → `RoleService` | Mantenimiento de roles (Seguridad) |

---

## Ubicación SQL

### Tablas (DDL)

| Archivo | Ubicación |
|---------|-----------|
| Definición de tablas | `SQL/Tablas Completas.sql` |

Secciones relevantes:

**Roles** (~líneas 199–207):

```sql
CREATE TABLE Roles (
    id_role INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    status TINYINT NOT NULL DEFAULT (1)
);
```

**Users** (~líneas 212–221):

```sql
CREATE TABLE Users (
    id_user INT IDENTITY(1,1) PRIMARY KEY,
    id_role INT NOT NULL,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_user_role FOREIGN KEY (id_role) REFERENCES Roles(id_role)
);
```

> La tabla `Users` tiene `deleted_at` en el esquema, pero el módulo **no usa eliminación lógica**: la única eliminación expuesta es física (`DELETE`).

### Stored Procedures

| Archivo | Contenido |
|---------|-----------|
| `SQL/Espinoza.sql` | SPs CRUD de Roles y Usuarios (Secciones ~766 y ~907) |
| `SQL/Login and Register - SP.sql` | SPs de autenticación reutilizados por Usuarios |

Secciones en `Espinoza.sql`:

| Sección | Línea aprox. |
|---------|--------------|
| Roles   | ~766         |
| Users   | ~907         |

---

## Stored Procedures

Patrón: `sp_{entidad_singular}_{funcion}`

### Roles

| SP | Descripción |
|----|-------------|
| `sp_role_list_select_active` | Roles activos para **combos** (Register + formulario Usuarios). Devuelve solo `id_role`, `name` |
| `sp_role_list_active` | Listado **paginado** de roles activos (grid del módulo Roles) |
| `sp_role_list_inactive` | Listado paginado de roles inactivos |
| `sp_role_get_by_id` | Detalle por ID |
| `sp_role_create` | Crear |
| `sp_role_update` | Actualizar |
| `sp_role_delete_logic` | Desactivar |
| `sp_role_restore` | Restaurar |
| `sp_role_delete_physical` | Eliminar permanente (solo inactivos) |

> **Nota:** `sp_role_list_select_active` es para selects; `sp_role_list_active` es para el grid paginado. El Register usa el primero vía `UserService.GetActiveRolesAsync()`.

### Users (módulo Seguridad)

| SP | Archivo | Descripción |
|----|---------|-------------|
| `sp_user_create` | `Login and Register - SP.sql` | Crear usuario (Register + módulo Usuarios) |
| `sp_user_get_by_username` | `Login and Register - SP.sql` | Login |
| `sp_user_role_list_active` | `Espinoza.sql` | Roles activos para combo del formulario Usuarios |
| `sp_user_list_active` | `Espinoza.sql` | Listado paginado de usuarios |
| `sp_user_get_by_id` | `Espinoza.sql` | Detalle por ID |
| `sp_user_update` | `Espinoza.sql` | Actualizar (`@password_hash` opcional) |
| `sp_user_delete_physical` | `Espinoza.sql` | Eliminar permanentemente |

No existen `sp_user_list_inactive`, `sp_user_delete_logic` ni `sp_user_restore` por diseño del módulo.

---

## Estructura de archivos

### Roles

```
Controllers/Roles/RolesController.cs
Models/Roles/
├── Role.cs                 # Entidad simple (Register / combos)
├── RoleDtos.cs             # ListItem, Detail, SpResult, PagedResult
└── RolesViewModel.cs
Services/Roles/
├── IRoleService.cs
└── RoleService.cs
Views/Roles/Index.cshtml
wwwroot/Public/CSS/Roles/roles.css
wwwroot/Public/JS/Roles/roles.js
```

### Usuarios

```
Controllers/Usuarios/UsuariosController.cs
Models/Usuarios/
├── UsuarioDtos.cs          # ListItem, Detail, SpResult, PagedResult, RoleOption
└── UsuariosViewModel.cs
Services/Usuarios/
├── IUsuarioService.cs
└── UsuarioService.cs
Views/Usuarios/Index.cshtml
wwwroot/Public/CSS/Usuarios/usuarios.css
wwwroot/Public/JS/Usuarios/usuarios.js
```

### Registro en `Program.cs`

```csharp
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
```

### DTOs en `ApplicationDbContext`

Registrados con `.HasNoKey()`:

- `RoleListItem`, `RoleDetail`, `RoleSpResult`
- `UsuarioListItem`, `UsuarioDetail`, `UsuarioSpResult`, `UsuarioRoleOption`

---

## Nomenclatura

### Prefijos CSS (BEM)

| Módulo   | Prefijo CSS | Clase raíz |
|----------|-------------|------------|
| Roles    | `rol-`      | `.rol`     |
| Usuarios | `usr-`      | `.usr`     |

### Variables JavaScript globales

| Módulo   | Variable global   |
|----------|-------------------|
| Roles    | `window.rolUrls`  |
| Usuarios | `window.usrUrls`  |

### Rutas de assets

```html
<link rel="stylesheet" href="@Url.Content("~/Public/CSS/Roles/roles.css")" asp-append-version="true" />
<script src="@Url.Content("~/Public/JS/Roles/roles.js")" asp-append-version="true"></script>
```

---

## Endpoints del controller

### RolesController

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

### UsuariosController

| Método | Acción           | Uso                          |
|--------|------------------|------------------------------|
| GET    | `Index`          | Vista principal              |
| GET    | `List`           | Grid activos                 |
| GET    | `Get`            | Detalle / editar             |
| GET    | `FkOptions`      | Roles activos para el select |
| POST   | `Create`         | Crear (password obligatoria) |
| POST   | `Update`         | Editar (password opcional)   |
| POST   | `DeletePhysical` | Eliminar permanente          |

No hay `ListInactive`, `DeleteLogic` ni `Restore` en Usuarios.

---

## Patrón de servicios (C#)

### Roles

Igual que los módulos de Configuración:

- **Create** → `RoleSpResult` (`success`, `message`, `id_role`)
- **Update / DeleteLogic / Restore / DeletePhysical** → `SpResult` (`Models/Users/SpResult.cs`)

### Usuarios

- **Create** → reutiliza `sp_user_create` → `UsuarioSpResult` (`success`, `message`, `id_user`)
- **Update / DeletePhysical** → `SpResult`
- **GetRoleOptions** → `UsuarioRoleOption` (solo `id_role`, `name`)

### Register y el select de roles

`UserService.GetActiveRolesAsync()` ejecuta `sp_role_list_select_active` y mapea a `UsuarioRoleOption` (no a `Role` directamente), porque el SP solo devuelve `id_role` y `name`. EF Core exige que las columnas del result set coincidan con el DTO usado en `SqlQueryRaw`.

---

## Comportamiento UI — Usuarios vs Roles

| Elemento UI              | Roles | Usuarios |
|--------------------------|-------|----------|
| Botón "Ver inactivos"    | Sí    | No       |
| Modal de inactivos       | Sí    | No       |
| Icono eliminar en activos| Desactivar (lógico) | Eliminar permanente |
| Icono en inactivos       | Restaurar / Purge | — |

En Usuarios, el botón de eliminar en el grid activo abre confirmación y llama directamente a `DeletePhysical`.

---

## Orden de ejecución SQL recomendado

```
1. SQL/Tablas Completas.sql
2. SQL/Login and Register - SP.sql     ← sp_user_create, sp_user_get_by_username
3. SQL/Alertas.sql
4. SQL/Espinoza.sql                    ← SPs de Roles, Usuarios y Configuración
```

Al ejecutar `Espinoza.sql`, el SP `sp_role_list_active` del archivo de Login queda **reemplazado** por la versión paginada del grid. Por eso existe `sp_role_list_select_active` para Register y combos.

---

## Referencias

- Tarjeta Configuración (mismo patrón CRUD): `Documentation/Tarjeta Configuracion.md`
- Estructura general: `Documentation/Estructura.md`
- Convenciones de nombres: `Documentation/Nomenclatura.md`
- Conexión a base de datos: `Documentation/Database.md`
