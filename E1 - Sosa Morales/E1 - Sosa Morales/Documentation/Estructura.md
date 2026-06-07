# Estructura del proyecto KMLLogistics

Este documento describe cómo está organizado el proyecto **E1 - Sosa Morales** (ASP.NET Core 8 MVC). La regla principal es: **cada módulo o pantalla tiene su propia carpeta** en Model, Controller, View, CSS y JS.

---

## Raíz del proyecto

```
E1 - Sosa Morales/
├── Config/                 # Configuración de módulos del dashboard
├── Controllers/            # Controladores (una carpeta por módulo)
├── Data/                   # DbContext y acceso a datos
├── Documentation/          # Documentación del equipo
├── Models/                 # ViewModels y entidades (una carpeta por dominio)
├── Services/               # Lógica de negocio / llamadas a SP
├── SQL/                    # Scripts de base de datos
├── ViewComponents/         # Componentes reutilizables de vista
├── Views/                  # Vistas Razor
├── wwwroot/Public/         # Archivos estáticos propios (CSS, JS, imágenes)
├── Program.cs              # Arranque y configuración de la app
└── appsettings.json        # Cadena de conexión y configuración
```

---

## Convención por módulo

Cada funcionalidad sigue el mismo patrón. Ejemplo con **Países**:

| Capa        | Ubicación                                              | Archivo principal              |
|-------------|--------------------------------------------------------|--------------------------------|
| Controller  | `Controllers/Countries/`                               | `CountriesController.cs`       |
| Model       | `Models/Countries/`                                    | `CountriesViewModel.cs`        |
| View        | `Views/Countries/`                                     | `Index.cshtml`                 |
| CSS         | `wwwroot/Public/CSS/Countries/`                        | `countries.css`                |
| JS          | `wwwroot/Public/JS/Countries/`                         | `countries.js`                 |
| Ruta URL    | —                                                      | `/Countries`                   |

### Reglas de nombres

- **Carpeta:** PascalCase (`Regiones`, `TiposDocumento`, `AlertasStock`).
- **Controller:** `{Nombre}Controller.cs` dentro de `Controllers/{Nombre}/`.
- **ViewModel:** `{Nombre}ViewModel.cs` dentro de `Models/{Nombre}/`.
- **Vista:** siempre `Views/{Nombre}/Index.cshtml`.
- **CSS/JS:** nombre en kebab-case derivado del módulo (`tipos-documento.css`, `alertas-stock.js`).
- **Namespace C#:** `E1___Sosa_Morales.Controllers.{Nombre}`, `E1___Sosa_Morales.Models.{Nombre}`.

### Referencia en la vista

```html
@section Styles {
    <link rel="stylesheet" href="~/Public/CSS/Countries/countries.css" asp-append-version="true" />
}

@section Scripts {
    <script src="~/Public/JS/Countries/countries.js" asp-append-version="true"></script>
}
```

---

## Ubicación de CSS y JS

Todos los estilos y scripts propios del sistema viven bajo:

```
wwwroot/Public/
├── CSS/
│   ├── AlertasStock/alertas-stock.css
│   ├── Dashboard/dashboard.css
│   ├── LandingPage/landing.css
│   ├── Login/login.css
│   ├── Register/register.css
│   ├── Site/site.css
│   ├── Countries/countries.css
│   ├── Regiones/regiones.css
│   └── ... (un archivo por módulo)
├── JS/
│   ├── AlertasStock/alertas-stock.js
│   ├── Dashboard/dashboard.js
│   ├── Site/site.js
│   ├── Countries/countries.js
│   └── ... (un archivo por módulo)
└── Images/
    └── Logo - KMLLogistics.png
```

**No** se usan las carpetas antiguas `wwwroot/css/` ni `wwwroot/js/` para código del proyecto.

### CSS y JS compartidos

| Archivo              | Uso                                              |
|----------------------|--------------------------------------------------|
| `Dashboard/dashboard.css` | Layout del panel: sidebar, header, tarjetas |
| `Dashboard/dashboard.js`  | Navegación AJAX del dashboard, filtros      |
| `Site/site.css` / `site.js` | Layout genérico (Home, Privacy)          |
| `LandingPage/landing.css`   | Página de inicio pública                  |

---

## Controllers

```
Controllers/
├── Login/LoginController.cs              # /Login, /Login/Logout
├── Register/RegisterController.cs        # /Register
├── LandingPage/LandingPageController.cs  # /
├── Dashboard/DashboardController.cs      # /Dashboard
├── AlertasStock/AlertasStockController.cs
├── Perfil/PerfilController.cs
├── Home/HomeController.cs
│
├── Roles/RolesController.cs              # Módulos del ERP (cada uno → Index)
├── Usuarios/UsuariosController.cs
├── TiposDocumento/TiposDocumentoController.cs
├── Countries/CountriesController.cs
├── Regiones/RegionesController.cs
├── Provincias/ProvinciasController.cs
├── Distritos/DistritosController.cs
├── Cargos/CargosController.cs
├── Empleados/EmpleadosController.cs
├── Clientes/ClientesController.cs
├── Proveedores/ProveedoresController.cs
├── Categorias/CategoriasController.cs
├── Marcas/MarcasController.cs
├── Productos/ProductosController.cs
├── ProductoProveedores/ProductoProveedoresController.cs
├── MarcasProveedor/MarcasProveedorController.cs
├── Almacenes/AlmacenesController.cs
├── DetalleAlmacen/DetalleAlmacenController.cs
├── TiposMovimiento/TiposMovimientoController.cs
├── MovimientosInventario/MovimientosInventarioController.cs
├── EstadosCompra/EstadosCompraController.cs
├── OrdenesCompra/OrdenesCompraController.cs
├── DetalleCompra/DetalleCompraController.cs
├── DetalleAlmacenCompra/DetalleAlmacenCompraController.cs
├── EstadosTransferencia/EstadosTransferenciaController.cs
├── ListaTransferencias/ListaTransferenciasController.cs
├── DetalleTransferencia/DetalleTransferenciaController.cs
├── EstadosVenta/EstadosVentaController.cs
├── ListaVentas/ListaVentasController.cs
├── DetalleVenta/DetalleVentaController.cs
├── Estadisticas/EstadisticasController.cs
├── Cajas/CajasController.cs
├── DetalleCaja/DetalleCajaController.cs
├── TiposVehiculo/TiposVehiculoController.cs
├── Vehiculos/VehiculosController.cs
├── EstadosEnvio/EstadosEnvioController.cs
├── Envios/EnviosController.cs
├── CajasEnvio/CajasEnvioController.cs
└── VentasEnvio/VentasEnvioController.cs
```

Los módulos del dashboard usan `ModuleRegistry` (`Config/ModuleRegistry.cs`) para definir pestañas y tarjetas.

---

## Models

```
Models/
├── Users/              # Login, Register (User, LoginViewModel, RegisterViewModel, SpResult)
├── Roles/              # Role (entidad), RolesViewModel (módulo)
├── Dashboard/          # Tarjetas, tabs, sidebar (DashboardModels.cs)
├── AlertasStock/       # Alertas de stock (varios ViewModels + DTOs de SP)
├── Perfil/             # Mi perfil
├── Home/               # Home
├── Error/              # Página de error
│
└── {Modulo}/           # Un ViewModel por módulo: {Modulo}ViewModel.cs
```

---

## Services

```
Services/
├── Users/
│   ├── IUserService.cs
│   └── UserService.cs          # Login, Register, roles
└── AlertasStock/
    ├── IStockAlertService.cs
    └── StockAlertService.cs    # Listado, filtros, reenvío de alertas
```

Los servicios llaman a **Stored Procedures** mediante `ApplicationDbContext.Database.SqlQueryRaw` o `ExecuteSqlRawAsync`.

---

## Views

```
Views/
├── LandingPage/Index.cshtml        # Página pública
├── Login/
│   ├── Index.cshtml
│   └── _Layout.cshtml              # Layout propio de login
├── Register/
│   ├── Index.cshtml
│   └── _Layout.cshtml              # Layout propio de register
├── Dashboard/Index.cshtml          # Panel principal con tarjetas
├── AlertasStock/Index.cshtml       # Centro de alertas de stock
├── Perfil/Index.cshtml
│
├── {Modulo}/Index.cshtml           # Una vista por módulo
│
└── Shared/
    ├── _DashboardLayout.cshtml     # Layout del panel (sidebar + header)
    ├── _DashboardHeader.cshtml     # Campana, usuario
    ├── _Sidebar.cshtml             # Menú lateral
    ├── _LandingLayout.cshtml       # Layout landing
    ├── _ModuleTabs.cshtml          # Pestañas entre módulos relacionados
    ├── _BrowserTab.cshtml          # Título y favicon
    └── Components/
        └── StockAlertCount/        # Badge de alertas en el header
```

### Layouts por tipo de pantalla

| Pantalla              | Layout                          |
|-----------------------|---------------------------------|
| Landing               | `_LandingLayout.cshtml`         |
| Login                 | `Views/Login/_Layout.cshtml`    |
| Register              | `Views/Register/_Layout.cshtml` |
| Dashboard y módulos   | `_DashboardLayout.cshtml`       |
| Home / Privacy        | `_Layout.cshtml`                |

---

## Config y otros

| Archivo                    | Función                                                |
|----------------------------|--------------------------------------------------------|
| `Config/ModuleRegistry.cs` | Tarjetas del dashboard, sidebar, pestañas por módulo   |
| `Config/DatabaseBootstrapper.cs` | Creación de BD si no existe (Home)              |
| `Data/ApplicationDbContext.cs`   | DbContext EF Core (SPs sin tablas mapeadas)     |
| `ViewComponents/StockAlertCountViewComponent.cs` | Contador campana header        |

---

## SQL

```
SQL/
├── Tablas Completas.sql       # DDL de todas las tablas
├── Login and Register - SP.sql
└── Alertas.sql                # StockAlerts, min_stock, SPs de alertas
```

Orden sugerido de ejecución:

1. `Tablas Completas.sql`
2. `Login and Register - SP.sql`
3. `Alertas.sql`

---

## Navegación del dashboard

- El **sidebar** y las **tarjetas** del dashboard enlazan a cada módulo (`/Productos`, `/Clientes`, etc.).
- La navegación interna usa **AJAX** (`dashboard.js`): solo se actualiza `#dashboard-content`.
- Los CSS y JS de cada página se inyectan automáticamente al navegar por AJAX (ver `dashboard.js` → `injectPageStyles`, `runPageScripts`).

---

## Cómo agregar un módulo nuevo

1. Crear carpeta en `Models/{Nombre}/` con `{Nombre}ViewModel.cs`.
2. Crear `Controllers/{Nombre}/{Nombre}Controller.cs` con acción `Index()`.
3. Crear `Views/{Nombre}/Index.cshtml` con `_DashboardLayout`.
4. Crear `wwwroot/Public/CSS/{Nombre}/{nombre-kebab}.css`.
5. Crear `wwwroot/Public/JS/{Nombre}/{nombre-kebab}.js`.
6. Registrar pestañas/tarjetas en `Config/ModuleRegistry.cs` si aplica.
7. Crear SPs en `SQL/` y servicio si requiere base de datos.

El script `Scripts/Generate-ModuleScaffold.ps1` puede generar la estructura base de módulos.
