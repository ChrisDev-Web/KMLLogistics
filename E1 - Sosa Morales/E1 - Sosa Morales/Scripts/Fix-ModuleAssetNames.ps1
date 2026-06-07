$root = Split-Path -Parent $PSScriptRoot
$skip = @("Shared", "Home", "Dashboard", "Login", "Register", "LandingPage", "Perfil", "AlertasStock", "Catalogo", "Compras", "Configuracion", "Inventario", "Logistica", "Organizacion", "PersonasTerceros", "Seguridad", "Transferencias", "Ventas")

function Get-AssetName([string]$name) {
    return ($name -creplace '([a-z])([A-Z])', '$1-$2').ToLower()
}

$dirs = Get-ChildItem -Path (Join-Path $root "Controllers") -Directory
foreach ($dir in $dirs) {
    $name = $dir.Name
    if ($skip -contains $name) { continue }

    $assetName = Get-AssetName $name
    $cssDir = Join-Path $root "wwwroot\Public\CSS\$name"
    $jsDir = Join-Path $root "wwwroot\Public\JS\$name"
    $viewPath = Join-Path $root "Views\$name\Index.cshtml"

    if (Test-Path $cssDir) {
        foreach ($file in Get-ChildItem $cssDir -Filter "*.css") {
            $target = Join-Path $cssDir "$assetName.css"
            if ($file.FullName -ne $target) {
                if (Test-Path $target) { Remove-Item $file.FullName -Force }
                else { Rename-Item $file.FullName $target }
            }
        }
        if (-not (Test-Path (Join-Path $cssDir "$assetName.css"))) {
            Set-Content -Path (Join-Path $cssDir "$assetName.css") -Value "/* Modulo: $name */" -Encoding UTF8
        }
    }

    if (Test-Path $jsDir) {
        foreach ($file in Get-ChildItem $jsDir -Filter "*.js") {
            $target = Join-Path $jsDir "$assetName.js"
            if ($file.FullName -ne $target) {
                if (Test-Path $target) { Remove-Item $file.FullName -Force }
                else { Rename-Item $file.FullName $target }
            }
        }
        if (-not (Test-Path (Join-Path $jsDir "$assetName.js"))) {
            Set-Content -Path (Join-Path $jsDir "$assetName.js") -Value "// Modulo: $name`n(function () { 'use strict'; })();" -Encoding UTF8
        }
    }

    $viewContent = @"
@model E1___Sosa_Morales.Models.$name.${name}ViewModel
@{
    Layout = "_DashboardLayout";
    ViewBag.PageTitle = Model.Module.Title;
    ViewBag.SidebarActive = Model.Module.SidebarActive;
}

@section Styles {
    <link rel="stylesheet" href="~/Public/CSS/$name/$assetName.css" asp-append-version="true" />
}

<partial name="_ModuleTabs" model="Model.Module" />

<div class="module-placeholder">
    <h2>EN DESARROLLO</h2>
    <p>Modulo <strong>@Model.Module.Title</strong> - proximamente disponible.</p>
</div>

@section Scripts {
    <script src="~/Public/JS/$name/$assetName.js" asp-append-version="true"></script>
}
"@

    Set-Content -Path $viewPath -Value $viewContent -Encoding UTF8
}

Write-Host "Fixed $($dirs.Count) controller folders."
