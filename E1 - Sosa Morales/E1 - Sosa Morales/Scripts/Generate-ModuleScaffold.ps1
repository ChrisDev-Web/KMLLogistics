$root = Split-Path -Parent $PSScriptRoot

$modules = @(
    @{ Name = "Roles"; Group = "Seguridad"; Label = "Roles"; Sidebar = "seguridad" },
    @{ Name = "Usuarios"; Group = "Seguridad"; Label = "Usuarios"; Sidebar = "seguridad" },
    @{ Name = "TiposDocumento"; Group = "Configuracion"; Label = "Tipos de documento"; Sidebar = "dashboard" },
    @{ Name = "Countries"; Group = "Configuracion"; Label = "Países"; Sidebar = "dashboard" },
    @{ Name = "Regiones"; Group = "Configuracion"; Label = "Regiones"; Sidebar = "dashboard" },
    @{ Name = "Provincias"; Group = "Configuracion"; Label = "Provincias"; Sidebar = "dashboard" },
    @{ Name = "Distritos"; Group = "Configuracion"; Label = "Distritos"; Sidebar = "dashboard" },
    @{ Name = "Cargos"; Group = "Organizacion"; Label = "Cargos"; Sidebar = "rrhh" },
    @{ Name = "Empleados"; Group = "Organizacion"; Label = "Empleados"; Sidebar = "rrhh" },
    @{ Name = "Clientes"; Group = "PersonasTerceros"; Label = "Clientes"; Sidebar = "clientes" },
    @{ Name = "Proveedores"; Group = "PersonasTerceros"; Label = "Proveedores"; Sidebar = "proveedores" },
    @{ Name = "Categorias"; Group = "Catalogo"; Label = "Categorías"; Sidebar = "productos" },
    @{ Name = "Marcas"; Group = "Catalogo"; Label = "Marcas"; Sidebar = "productos" },
    @{ Name = "Productos"; Group = "Catalogo"; Label = "Productos"; Sidebar = "productos" },
    @{ Name = "ProductoProveedores"; Group = "Catalogo"; Label = "Producto proveedores"; Sidebar = "productos" },
    @{ Name = "MarcasProveedor"; Group = "Catalogo"; Label = "Marcas de proveedor"; Sidebar = "productos" },
    @{ Name = "Almacenes"; Group = "Inventario"; Label = "Almacenes"; Sidebar = "inventario" },
    @{ Name = "DetalleAlmacen"; Group = "Inventario"; Label = "Detalle de almacén"; Sidebar = "inventario" },
    @{ Name = "TiposMovimiento"; Group = "Inventario"; Label = "Tipos de movimiento"; Sidebar = "inventario" },
    @{ Name = "MovimientosInventario"; Group = "Inventario"; Label = "Movimientos de inventario"; Sidebar = "inventario" },
    @{ Name = "EstadosCompra"; Group = "Compras"; Label = "Estados de compra"; Sidebar = "compras" },
    @{ Name = "OrdenesCompra"; Group = "Compras"; Label = "Compras"; Sidebar = "compras" },
    @{ Name = "DetalleCompra"; Group = "Compras"; Label = "Detalle de compra"; Sidebar = "compras" },
    @{ Name = "DetalleAlmacenCompra"; Group = "Compras"; Label = "Detalle almacén compra"; Sidebar = "compras" },
    @{ Name = "EstadosTransferencia"; Group = "Transferencias"; Label = "Estados de transferencia"; Sidebar = "transferencias" },
    @{ Name = "ListaTransferencias"; Group = "Transferencias"; Label = "Transferencias"; Sidebar = "transferencias" },
    @{ Name = "DetalleTransferencia"; Group = "Transferencias"; Label = "Detalle de transferencia"; Sidebar = "transferencias" },
    @{ Name = "EstadosVenta"; Group = "Ventas"; Label = "Estados de venta"; Sidebar = "ventas" },
    @{ Name = "ListaVentas"; Group = "Ventas"; Label = "Ventas"; Sidebar = "ventas" },
    @{ Name = "DetalleVenta"; Group = "Ventas"; Label = "Detalle de venta"; Sidebar = "ventas" },
    @{ Name = "Estadisticas"; Group = "Estadisticas"; Label = "Estadísticas"; Sidebar = "dashboard" },
    @{ Name = "Cajas"; Group = "Logistica"; Label = "Cajas"; Sidebar = "logistica" },
    @{ Name = "DetalleCaja"; Group = "Logistica"; Label = "Detalle de caja"; Sidebar = "logistica" },
    @{ Name = "TiposVehiculo"; Group = "Logistica"; Label = "Tipos de vehículo"; Sidebar = "logistica" },
    @{ Name = "Vehiculos"; Group = "Logistica"; Label = "Vehículos"; Sidebar = "logistica" },
    @{ Name = "EstadosEnvio"; Group = "Logistica"; Label = "Estados de envío"; Sidebar = "logistica" },
    @{ Name = "Envios"; Group = "Logistica"; Label = "Envíos"; Sidebar = "logistica" },
    @{ Name = "CajasEnvio"; Group = "Logistica"; Label = "Cajas de envío"; Sidebar = "logistica" },
    @{ Name = "VentasEnvio"; Group = "Logistica"; Label = "Ventas de envío"; Sidebar = "logistica" }
)

foreach ($m in $modules) {
    $name = $m.Name
    $group = $m.Group
    $label = $m.Label
    $sidebar = $m.Sidebar
    $cssFile = ($name -creplace '([a-z])([A-Z])', '$1-$2').ToLower()

    $modelDir = Join-Path $root "Models\$name"
    $controllerDir = Join-Path $root "Controllers\$name"
    $viewDir = Join-Path $root "Views\$name"
    $cssDir = Join-Path $root "wwwroot\Public\CSS\$name"
    $jsDir = Join-Path $root "wwwroot\Public\JS\$name"

    New-Item -ItemType Directory -Force -Path $modelDir, $controllerDir, $viewDir, $cssDir, $jsDir | Out-Null

    $modelPath = Join-Path $modelDir "${name}ViewModel.cs"
    if (-not (Test-Path $modelPath)) {
        @"
using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.$name;

public class ${name}ViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}
"@ | Set-Content -Path $modelPath -Encoding UTF8
    }

    $controllerPath = Join-Path $controllerDir "${name}Controller.cs"
    @"
using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.$name;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.$name;

[Authorize]
public class ${name}Controller : Controller
{
    public IActionResult Index()
    {
        var model = new ${name}ViewModel
        {
            Module = ModuleRegistry.BuildModuleView("$group", "$name", "$sidebar")
        };

        return View(model);
    }
}
"@ | Set-Content -Path $controllerPath -Encoding UTF8

    $viewPath = Join-Path $viewDir "Index.cshtml"
    @"
@model E1___Sosa_Morales.Models.$name.${name}ViewModel
@{
    Layout = "_DashboardLayout";
    ViewBag.PageTitle = Model.Module.Title;
    ViewBag.SidebarActive = Model.Module.SidebarActive;
}

@section Styles {
    <link rel="stylesheet" href="~/Public/CSS/$name/$cssFile.css" asp-append-version="true" />
}

<partial name="_ModuleTabs" model="Model.Module" />

<div class="module-placeholder">
    <h2>EN DESARROLLO</h2>
    <p>Modulo <strong>@Model.Module.Title</strong> - proximamente disponible.</p>
</div>

@section Scripts {
    <script src="~/Public/JS/$name/$cssFile.js" asp-append-version="true"></script>
}
"@ | Set-Content -Path $viewPath -Encoding UTF8

    $cssPath = Join-Path $cssDir "$cssFile.css"
    if (-not (Test-Path $cssPath)) {
        @"
/* Módulo: $label */
"@ | Set-Content -Path $cssPath -Encoding UTF8
    }

    $jsPath = Join-Path $jsDir "$cssFile.js"
    if (-not (Test-Path $jsPath)) {
        @"
// Módulo: $label
(function () {
    'use strict';
})();
"@ | Set-Content -Path $jsPath -Encoding UTF8
    }
}

Write-Host "Generated $($modules.Count) modules."
