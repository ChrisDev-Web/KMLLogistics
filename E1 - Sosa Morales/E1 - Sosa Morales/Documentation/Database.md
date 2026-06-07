# Configuración de base de datos

Guía para que cada integrante del equipo conecte el proyecto **KMLLogistics** a su propia instancia de SQL Server.

---

## Requisitos

- **SQL Server 2017** o superior (o SQL Server Express / LocalDB).
- Base de datos llamada **`KMLLogistics`** (o el nombre que configures, pero debes actualizarlo en todos los puntos indicados).
- Herramienta para ejecutar scripts: SSMS, Azure Data Studio o similar.

---

## Paso 1 — Crear la base de datos y tablas

Ejecutar los scripts SQL **en este orden** sobre tu instancia:

| Orden | Archivo | Ubicación en el proyecto |
|-------|---------|--------------------------|
| 1 | `Tablas Completas.sql` | `SQL/Tablas Completas.sql` |
| 2 | `Login and Register - SP.sql` | `SQL/Login and Register - SP.sql` |
| 3 | `Alertas.sql` | `SQL/Alertas.sql` |

Cada script comienza con:

```sql
USE KMLLogistics;
GO
```

Si tu base de datos tiene otro nombre, cambia `KMLLogistics` por el nombre que uses **en los tres archivos SQL** antes de ejecutarlos.

---

## Paso 2 — Configurar la cadena de conexión en la aplicación

### Archivo principal: `appsettings.json`

**Ubicación:** raíz del proyecto → `appsettings.json`

**Líneas a modificar: 2 y 3**

```json
{
  "ConnectionStrings": {
    "bd_ventas": "Server=.;Database=KMLLogistics;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },
```

#### Qué cambiar en `bd_ventas`

| Parámetro   | Qué poner | Ejemplo |
|-------------|-----------|---------|
| `Server`    | Instancia SQL Server | `.` (local), `localhost`, `.\SQLEXPRESS`, `MI-PC\SQLEXPRESS` |
| `Database`  | Nombre de tu BD | `KMLLogistics` |
| Autenticación | Windows o SQL | Ver ejemplos abajo |

#### Ejemplo — Windows Authentication (recomendado en desarrollo)

```json
"bd_ventas": "Server=.\\SQLEXPRESS;Database=KMLLogistics;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

#### Ejemplo — SQL Server Authentication (usuario y contraseña)

```json
"bd_ventas": "Server=.;Database=KMLLogistics;User Id=tu_usuario;Password=tu_contraseña;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

> **Importante:** No subas contraseñas reales al repositorio. Usa `appsettings.Development.json` local (ver siguiente sección) o variables de entorno.

---

### Archivo opcional: `appsettings.Development.json`

**Ubicación:** raíz del proyecto → `appsettings.Development.json`

Puedes **sobrescribir** la cadena solo en tu máquina sin afectar al equipo. Agrega o edita:

```json
{
  "ConnectionStrings": {
    "bd_ventas": "Server=TU_SERVIDOR;Database=KMLLogistics;User Id=TU_USUARIO;Password=TU_PASSWORD;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

ASP.NET Core carga `appsettings.Development.json` automáticamente cuando ejecutas en modo **Development**.

---

## Paso 3 — Dónde lee la app la conexión (código)

### `Program.cs` — línea 14

**Ubicación:** `Program.cs`

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("bd_ventas")));
```

La clave **`bd_ventas`** debe coincidir exactamente con el nombre en `appsettings.json`. No cambies esta línea salvo que renombres la clave en JSON.

### `Controllers/Home/HomeController.cs` — línea ~34

También usa la misma cadena para verificar conexión al iniciar Home:

```csharp
_configuration.GetConnectionString("bd_ventas")
```

### `Config/DatabaseBootstrapper.cs`

Usa la cadena para intentar crear la base de datos si no existe (solo desde Home/Index).

---

## Paso 4 — Verificar la conexión

1. Guarda los cambios en `appsettings.json` o `appsettings.Development.json`.
2. Ejecuta el proyecto (`dotnet run` o F5 en Visual Studio).
3. Navega a `/Login` o `/Dashboard` tras autenticarte.
4. Si hay error de conexión, revisa:
   - Que SQL Server esté **iniciado**.
   - Que la BD **KMLLogistics** exista.
   - Que los **SP** estén creados (scripts del Paso 1).
   - Que el usuario tenga permisos de lectura/escritura.

---

## Resumen rápido para tu compañero

```
1. Ejecutar SQL/Tablas Completas.sql
2. Ejecutar SQL/Login and Register - SP.sql
3. Ejecutar SQL/Alertas.sql
4. Abrir appsettings.json → línea 3 → cambiar Server y credenciales en "bd_ventas"
5. (Opcional) Poner credenciales solo locales en appsettings.Development.json
6. Correr la aplicación
```

---

## Nombre de la base de datos en el código

Algunas pantallas muestran el nombre **KMLLogistics** como texto fijo:

| Archivo | Uso |
|---------|-----|
| `Controllers/Home/HomeController.cs` | Mensaje de conexión en Home |
| Scripts `SQL/*.sql` | `USE KMLLogistics` |

Si tu equipo usa otro nombre de BD, actualiza también esos puntos para mantener coherencia.

---

## Seguridad — buenas prácticas

- No commitear usuarios ni contraseñas en `appsettings.json`.
- Usar `appsettings.Development.json` para credenciales locales (agregar al `.gitignore` si contiene secretos).
- En producción, usar **variables de entorno** o **User Secrets** de .NET:

```bash
dotnet user-secrets set "ConnectionStrings:bd_ventas" "Server=...;Database=KMLLogistics;..."
```

---

## User Secrets (alternativa avanzada)

En la raíz del proyecto:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:bd_ventas" "Server=.;Database=KMLLogistics;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

User Secrets sobrescribe `appsettings.json` solo en tu máquina y no se sube al repositorio.
