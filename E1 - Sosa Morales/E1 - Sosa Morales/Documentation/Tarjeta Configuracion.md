# Tarjeta Configuración

Documentación del módulo **Configuración** del dashboard KMLLogistics: qué se implementó, estructura de archivos, nomenclatura, base de datos y endpoints.

---

## Resumen

La tarjeta **Configuración** agrupa cinco módulos CRUD de parámetros geográficos y de identificación. Cada uno tiene **archivos propios** (no hay controlador ni CSS/JS compartidos genéricos).

| Módulo            | Ruta URL          | Tabla SQL        | Estado   |
|-------------------|-------------------|------------------|----------|
| Tipos de documento| `/TiposDocumento` | `DocumentTypes`  | Completo |
| Países            | `/Countries`      | `Countries`      | Completo |
| Regiones          | `/Regiones`       | `Regions`        | Completo |
| Provincias        | `/Provincias`     | `Provinces`      | Completo |
| Distritos         | `/Distritos`      | `Districts`      | Completo |

Las pestañas entre módulos se definen en `Config/ModuleRegistry.cs` bajo la clave `"Configuracion"`.

---

## Funcionalidades implementadas

Todos los módulos de esta tarjeta comparten el mismo comportamiento:

- Listar registros **activos** con paginación (10 / 20 / 50).
- Buscador AJAX en tiempo real.
- **Ver inactivos** en modal con su propia paginación y búsqueda.
- Crear, editar y ver detalle (modal).
- **Eliminación lógica** (`status = 0`).
- **Restaurar** registros inactivos.
- **Eliminación física** solo sobre registros inactivos.
- Scroll vertical automático cuando hay más de 10 filas visibles.
- Notificaciones toast de éxito/error.

### Jerarquía de datos (FK)

```
Countries
  └── Regions (FK: id_country)
        └── Provinces (FK: id_region)
              └── Districts (FK: id_province)
```

Regiones, Provincias y Distritos incluyen un **select de FK** en el formulario (país, región o provincia padre, según el módulo).

---

## Ubicación SQL

### Tablas (DDL)

| Archivo | Ubicación |
|---------|-----------|
| Definición de tablas | `SQL/Tablas Completas.sql` |

Secciones relevantes:

- `DocumentTypes` — líneas ~4–12
- `Countries` — líneas ~17–24
- `Regions` — líneas ~26–36
- `Provinces` — líneas ~38–48
- `Districts` — líneas ~50–60

Campos comunes de auditoría: `created_at`, `updated_at`, `deleted_at`, `status` (1 = activo, 0 = inactivo).

### Stored Procedures (CRUD)

| Archivo | Ubicación |
|---------|-----------|
| Todos los SP de Configuración | `SQL/Espinoza.sql` |

Ejecutar **después** de `Tablas Completas.sql` y `Login and Register - SP.sql`.

Secciones dentro de `Espinoza.sql`:

| Sección           | Línea aprox. |
|-------------------|--------------|
| Document Types    | ~10          |
| Countries         | ~191         |
| Regions           | ~322         |
| Provinces         | ~468         |
| Districts         | ~617         |

---

## Stored Procedures por módulo

Patrón: `sp_{entidad_singular}_{funcion}`

### Tipos de documento (`document_type`)

| SP | Descripción |
|----|-------------|
| `sp_document_type_list_active` | Listado paginado de activos |
| `sp_document_type_list_inactive` | Listado paginado de inactivos |
| `sp_document_type_get_by_id` | Detalle por ID |
| `sp_document_type_create` | Crear (devuelve `success`, `message`, `id_document_type`) |
| `sp_document_type_update` | Actualizar |
| `sp_document_type_delete_logic` | Desactivar |
| `sp_document_type_restore` | Restaurar |
| `sp_document_type_delete_physical` | Eliminar permanente (solo inactivos) |

### Países (`country`)

| SP | Descripción |
|----|-------------|
| `sp_country_list_active` | Listado paginado de activos |
| `sp_country_list_inactive` | Listado paginado de inactivos |
| `sp_country_get_by_id` | Detalle por ID |
| `sp_country_create` | Crear |
| `sp_country_update` | Actualizar |
| `sp_country_delete_logic` | Desactivar |
| `sp_country_restore` | Restaurar |
| `sp_country_delete_physical` | Eliminar permanente |

### Regiones (`region`)

| SP | Descripción |
|----|-------------|
| `sp_region_country_list_active` | Países activos para combo del formulario |
| `sp_region_list_active` | Listado paginado de activos |
| `sp_region_list_inactive` | Listado paginado de inactivos |
| `sp_region_get_by_id` | Detalle por ID (incluye nombre del país) |
| `sp_region_create` | Crear |
| `sp_region_update` | Actualizar |
| `sp_region_delete_logic` | Desactivar |
| `sp_region_restore` | Restaurar |
| `sp_region_delete_physical` | Eliminar permanente |

### Provincias (`province`)

| SP | Descripción |
|----|-------------|
| `sp_province_region_list_active` | Regiones activas para combo |
| `sp_province_list_active` | Listado paginado de activos |
| `sp_province_list_inactive` | Listado paginado de inactivos |
| `sp_province_get_by_id` | Detalle por ID |
| `sp_province_create` | Crear |
| `sp_province_update` | Actualizar |
| `sp_province_delete_logic` | Desactivar |
| `sp_province_restore` | Restaurar |
| `sp_province_delete_physical` | Eliminar permanente |

### Distritos (`district`)

| SP | Descripción |
|----|-------------|
| `sp_district_province_list_active` | Provincias activas para combo |
| `sp_district_list_active` | Listado paginado de activos |
| `sp_district_list_inactive` | Listado paginado de inactivos |
| `sp_district_get_by_id` | Detalle por ID |
| `sp_district_create` | Crear |
| `sp_district_update` | Actualizar |
| `sp_district_delete_logic` | Desactivar |
| `sp_district_restore` | Restaurar |
| `sp_district_delete_physical` | Eliminar permanente |

---

## Estructura de archivos por módulo

Cada módulo sigue el mismo patrón de carpetas. Ejemplo con **Países**:

```
Controllers/Countries/CountriesController.cs
Models/Countries/
├── CountriesViewModel.cs
└── CountryDtos.cs          # ListItem, Detail, SpResult, PagedResult, FkOption (si aplica)
Services/Countries/
├── ICountryService.cs
└── CountryService.cs
Views/Countries/Index.cshtml
wwwroot/Public/CSS/Countries/countries.css
wwwroot/Public/JS/Countries/countries.js
```

### Mapa completo de la tarjeta

| Módulo           | Controller              | Models                         | Service                         | Vista                    | CSS                         | JS                           |
|------------------|-------------------------|--------------------------------|---------------------------------|--------------------------|-----------------------------|------------------------------|
| TiposDocumento   | `TiposDocumentoController` | `Models/TiposDocumento/`    | `Services/TiposDocumento/`      | `Views/TiposDocumento/`  | `tipos-documento.css`       | `tipos-documento.js`         |
| Countries        | `CountriesController`   | `Models/Countries/`            | `Services/Countries/`           | `Views/Countries/`       | `countries.css`             | `countries.js`               |
| Regiones         | `RegionesController`    | `Models/Regiones/`             | `Services/Regiones/`            | `Views/Regiones/`        | `regiones.css`              | `regiones.js`                |
| Provincias       | `ProvinciasController`  | `Models/Provincias/`           | `Services/Provincias/`          | `Views/Provincias/`      | `provincias.css`            | `provincias.js`              |
| Distritos        | `DistritosController`   | `Models/Distritos/`            | `Services/Distritos/`           | `Views/Distritos/`       | `distritos.css`               | `distritos.js`               |

### Registro en `Program.cs`

```csharp
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IProvinceService, ProvinceService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
```

### DTOs en `ApplicationDbContext`

Cada `{Modulo}ListItem`, `{Modulo}Detail`, `{Modulo}SpResult` y `{Modulo}FkOption` se registra con `.HasNoKey()` en `Data/ApplicationDbContext.cs`.

---

## Nomenclatura

### Prefijos CSS (BEM)

Cada módulo usa un prefijo único para evitar conflictos entre estilos:

| Módulo           | Prefijo CSS | Clase raíz en HTML |
|------------------|-------------|--------------------|
| TiposDocumento   | `tdoc-`     | `.tdoc`            |
| Countries        | `ctry-`     | `.ctry`            |
| Regiones         | `reg-`      | `.reg`             |
| Provincias       | `prov-`     | `.prov`            |
| Distritos        | `dist-`     | `.dist`            |

Modales: `{prefijo}-modal`, botones: `{prefijo}__btn`, tablas: `{prefijo}__table`.

### Variables JavaScript globales

Definidas en `@section Scripts` de cada `Index.cshtml`:

| Módulo           | Variable global   |
|------------------|-------------------|
| TiposDocumento   | `window.tdocUrls` |
| Countries        | `window.ctryUrls` |
| Regiones         | `window.regUrls`  |
| Provincias       | `window.provUrls` |
| Distritos        | `window.distUrls` |

En el JS siempre usar `urls()` que retorna esa variable (nunca olvidar los paréntesis: `urls().fkOptions`).

### Rutas de assets en vistas

Usar rutas absolutas con `@Url.Content`:

```html
<link rel="stylesheet" href="@Url.Content("~/Public/CSS/Countries/countries.css")" asp-append-version="true" />
<script src="@Url.Content("~/Public/JS/Countries/countries.js")" asp-append-version="true"></script>
```

---

## Endpoints del controller (patrón común)

Cada controller expone acciones AJAX bajo `[Authorize]`:

| Método | Acción           | Uso                          |
|--------|------------------|------------------------------|
| GET    | `List`           | Grid de activos              |
| GET    | `ListInactive`   | Modal de inactivos           |
| GET    | `Get`            | Detalle / cargar formulario  |
| GET    | `FkOptions`      | Combo padre (Regiones, Provincias, Distritos) |
| POST   | `Create`         | Crear registro               |
| POST   | `Update`         | Editar registro              |
| POST   | `DeleteLogic`    | Desactivar                   |
| POST   | `Restore`        | Restaurar inactivo           |
| POST   | `DeletePhysical` | Eliminar permanente          |

Las acciones POST llevan `[ValidateAntiForgeryToken]`. El token se envía en el body (`__RequestVerificationToken`) y en el header `RequestVerificationToken` (configurado en `Program.cs`).

---

## Patrón de servicios (C#)

### Listados paginados

Los SP de listado devuelven `total_count` con `COUNT(*) OVER()`. El servicio arma un objeto paginado:

```csharp
public class DocumentTypePagedResult
{
    public List<object> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
```

### Resultados de SP mutables

| Operación              | DTO de respuesta        | Columnas esperadas        |
|------------------------|-------------------------|---------------------------|
| Create                 | `{Modulo}SpResult`      | `success`, `message`, `id_*` |
| Update / Delete / Restore / Purge | `SpResult` (`Models/Users/SpResult.cs`) | `success`, `message` |

> **Importante:** los DTOs de listado **no** incluyen `created_at` ni `updated_at` si el SP activo/inactivo devuelve columnas distintas. El detalle sí las incluye vía `GetById`.

---

## Orden de ejecución SQL recomendado

```
1. SQL/Tablas Completas.sql
2. SQL/Login and Register - SP.sql
3. SQL/Alertas.sql
4. SQL/Espinoza.sql          ← SPs de Configuración (y Seguridad)
```

---

## Referencias

- Estructura general del proyecto: `Documentation/Estructura.md`
- Convenciones de nombres: `Documentation/Nomenclatura.md`
- Conexión a base de datos: `Documentation/Database.md`
