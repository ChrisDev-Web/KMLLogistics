using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Config;

public static class ModuleRegistry
{
    public static List<SidebarItem> SidebarItems { get; } =
    [
        new() { Key = "dashboard", Label = "Dashboard", Icon = "bi-speedometer2", Controller = "Dashboard", Action = "Index" },
        new() { Key = "clientes", Label = "Clientes", Icon = "bi-people", Controller = "Clientes", Action = "Index" },
        new() { Key = "proveedores", Label = "Proveedores", Icon = "bi-truck", Controller = "Proveedores", Action = "Index" },
        new() { Key = "productos", Label = "Productos", Icon = "bi-box-seam", Controller = "Productos", Action = "Index" },
        new() { Key = "compras", Label = "Compras", Icon = "bi-cart-plus", Controller = "OrdenesCompra", Action = "Index" },
        new() { Key = "ventas", Label = "Ventas", Icon = "bi-receipt", Controller = "ListaVentas", Action = "Index" },
        new() { Key = "inventario", Label = "Inventario", Icon = "bi-building", Controller = "Almacenes", Action = "Index" },
        new() { Key = "transferencias", Label = "Transferencias", Icon = "bi-arrow-left-right", Controller = "ListaTransferencias", Action = "Index" },
        new() { Key = "logistica", Label = "Logística", Icon = "bi-geo-alt", Controller = "Envios", Action = "Index" },
        new() { Key = "rrhh", Label = "RRHH", Icon = "bi-person-badge", Controller = "Empleados", Action = "Index" },
        new() { Key = "seguridad", Label = "Seguridad", Icon = "bi-shield-lock", Controller = "Roles", Action = "Index" }
    ];

    public static List<DashboardCard> DashboardCards { get; } =
    [
        new() { Key = "configuracion", Title = "Configuración", Description = "Parámetros generales", Footer = "12 ajustes", Category = "sistema", CategoryLabel = "Sistema", Icon = "bi-gear", IconColor = "#3b82f6", Controller = "TiposDocumento", DefaultAction = "Index" },
        new() { Key = "seguridad", Title = "Seguridad", Description = "Roles y permisos", Footer = "8 roles", Category = "sistema", CategoryLabel = "Sistema", Icon = "bi-shield-check", IconColor = "#8b5cf6", Controller = "Roles", DefaultAction = "Index" },
        new() { Key = "personas-terceros", Title = "Personas y Terceros", Description = "Directorio activo", Footer = "350 registros", Category = "gestion", CategoryLabel = "Gestión", Icon = "bi-people", IconColor = "#10b981", Controller = "Clientes", DefaultAction = "Index" },
        new() { Key = "rrhh", Title = "Recursos Humanos", Description = "Empleados y cargos", Footer = "45 empleados", Category = "gestion", CategoryLabel = "Gestión", Icon = "bi-person-badge", IconColor = "#06b6d4", Controller = "Empleados", DefaultAction = "Index" },
        new() { Key = "productos", Title = "Productos", Description = "Catálogo y marcas", Footer = "120 productos", Category = "comercial", CategoryLabel = "Comercial", Icon = "bi-box-seam", IconColor = "#ec4899", Controller = "Productos", DefaultAction = "Index" },
        new() { Key = "inventario", Title = "Inventario", Description = "Movimientos y kardex", Footer = "Hoy: 15", Category = "operaciones", CategoryLabel = "Operaciones", Icon = "bi-building", IconColor = "#3b82f6", Controller = "Almacenes", DefaultAction = "Index" },
        new() { Key = "compras", Title = "Compras", Description = "Órdenes de compra", Footer = "12 pendientes", Category = "operaciones", CategoryLabel = "Operaciones", Icon = "bi-cart-plus", IconColor = "#14b8a6", Controller = "OrdenesCompra", DefaultAction = "Index" },
        new() { Key = "transferencias", Title = "Transferencias", Description = "Movimientos entre almacenes", Footer = "5 activas", Category = "operaciones", CategoryLabel = "Operaciones", Icon = "bi-arrow-left-right", IconColor = "#6366f1", Controller = "ListaTransferencias", DefaultAction = "Index" },
        new() { Key = "logistica", Title = "Logística", Description = "Despachos y envíos", Footer = "8 en tránsito", Category = "operaciones", CategoryLabel = "Operaciones", Icon = "bi-geo-alt", IconColor = "#0ea5e9", Controller = "Envios", DefaultAction = "Index" },
        new() { Key = "ventas", Title = "Ventas", Description = "Totales e impuestos", Footer = "S/ 45,200", Category = "operaciones", CategoryLabel = "Operaciones", Icon = "bi-currency-dollar", IconColor = "#22c55e", Controller = "ListaVentas", DefaultAction = "Index" },
        new() { Key = "estadisticas", Title = "Estadísticas", Description = "Indicadores y reportes", Footer = "12 reportes", Category = "estadisticas", CategoryLabel = "Estadísticas", Icon = "bi-bar-chart-line", IconColor = "#6366f1", Controller = "Estadisticas", DefaultAction = "Index" }
    ];

    public static Dictionary<string, List<ModuleTab>> ModuleTabs { get; } = new()
    {
        ["Seguridad"] =
        [
            new() { Label = "Roles", Controller = "Roles", Action = "Index" },
            new() { Label = "Usuarios", Controller = "Usuarios", Action = "Index" }
        ],
        ["Configuracion"] =
        [
            new() { Label = "Tipos de documento", Controller = "TiposDocumento", Action = "Index" },
            new() { Label = "Países", Controller = "Countries", Action = "Index" },
            new() { Label = "Regiones", Controller = "Regiones", Action = "Index" },
            new() { Label = "Provincias", Controller = "Provincias", Action = "Index" },
            new() { Label = "Distritos", Controller = "Distritos", Action = "Index" }
        ],
        ["Organizacion"] =
        [
            new() { Label = "Cargos", Controller = "Cargos", Action = "Index" },
            new() { Label = "Empleados", Controller = "Empleados", Action = "Index" }
        ],
        ["PersonasTerceros"] =
        [
            new() { Label = "Clientes", Controller = "Clientes", Action = "Index" },
            new() { Label = "Proveedores", Controller = "Proveedores", Action = "Index" }
        ],
        ["Catalogo"] =
        [
            new() { Label = "Categorías", Controller = "Categorias", Action = "Index" },
            new() { Label = "Marcas", Controller = "Marcas", Action = "Index" },
            new() { Label = "Productos", Controller = "Productos", Action = "Index" },
            new() { Label = "Producto proveedores", Controller = "ProductoProveedores", Action = "Index" },
            new() { Label = "Marcas de proveedor", Controller = "MarcasProveedor", Action = "Index" }
        ],
        ["Inventario"] =
        [
            new() { Label = "Almacenes", Controller = "Almacenes", Action = "Index" },
            new() { Label = "Detalle de almacén", Controller = "DetalleAlmacen", Action = "Index" },
            new() { Label = "Tipos de movimiento", Controller = "TiposMovimiento", Action = "Index" },
            new() { Label = "Movimientos de inventario", Controller = "MovimientosInventario", Action = "Index" }
        ],
        ["Compras"] =
        [
            new() { Label = "Estados de compra", Controller = "EstadosCompra", Action = "Index" },
            new() { Label = "Compras", Controller = "OrdenesCompra", Action = "Index" },
            new() { Label = "Detalle de compra", Controller = "DetalleCompra", Action = "Index" },
            new() { Label = "Detalle almacén compra", Controller = "DetalleAlmacenCompra", Action = "Index" }
        ],
        ["Transferencias"] =
        [
            new() { Label = "Estados de transferencia", Controller = "EstadosTransferencia", Action = "Index" },
            new() { Label = "Transferencias", Controller = "ListaTransferencias", Action = "Index" },
            new() { Label = "Detalle de transferencia", Controller = "DetalleTransferencia", Action = "Index" }
        ],
        ["Ventas"] =
        [
            new() { Label = "Estados de venta", Controller = "EstadosVenta", Action = "Index" },
            new() { Label = "Ventas", Controller = "ListaVentas", Action = "Index" },
            new() { Label = "Detalle de venta", Controller = "DetalleVenta", Action = "Index" }
        ],
        ["Estadisticas"] =
        [
            new() { Label = "Estadísticas", Controller = "Estadisticas", Action = "Index" }
        ],
        ["Logistica"] =
        [
            new() { Label = "Cajas", Controller = "Cajas", Action = "Index" },
            new() { Label = "Detalle de caja", Controller = "DetalleCaja", Action = "Index" },
            new() { Label = "Tipos de vehículo", Controller = "TiposVehiculo", Action = "Index" },
            new() { Label = "Vehículos", Controller = "Vehiculos", Action = "Index" },
            new() { Label = "Estados de envío", Controller = "EstadosEnvio", Action = "Index" },
            new() { Label = "Envíos", Controller = "Envios", Action = "Index" },
            new() { Label = "Cajas de envío", Controller = "CajasEnvio", Action = "Index" },
            new() { Label = "Ventas de envío", Controller = "VentasEnvio", Action = "Index" }
        ]
    };

    public static ModuleViewModel BuildModuleView(string groupKey, string controllerName, string sidebarActive)
    {
        var tabs = ModuleTabs.GetValueOrDefault(groupKey, []);
        var currentTab = tabs.FirstOrDefault(t =>
            string.Equals(t.Controller, controllerName, StringComparison.OrdinalIgnoreCase));

        return new ModuleViewModel
        {
            Title = currentTab?.Label ?? controllerName,
            CurrentController = controllerName,
            CurrentTab = currentTab?.Action ?? "Index",
            Tabs = tabs,
            SidebarActive = sidebarActive
        };
    }
}
