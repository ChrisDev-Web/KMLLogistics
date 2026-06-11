using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Services.AlertasStock;
using E1___Sosa_Morales.Services.Almacenes;
using E1___Sosa_Morales.Services.Cargos;
using E1___Sosa_Morales.Services.Categorias;
using E1___Sosa_Morales.Services.Clientes;
using E1___Sosa_Morales.Services.Countries;
using E1___Sosa_Morales.Services.DetalleAlmacen;
using E1___Sosa_Morales.Services.DetalleAlmacenCompra;
using E1___Sosa_Morales.Services.DetalleCompra;
using E1___Sosa_Morales.Services.DetalleVenta;
using E1___Sosa_Morales.Services.EstadosVenta;
using E1___Sosa_Morales.Services.ListaVentas;
using E1___Sosa_Morales.Services.DetalleTransferencia;
using E1___Sosa_Morales.Services.Distritos;
using E1___Sosa_Morales.Services.Empleados;
using E1___Sosa_Morales.Services.EstadosCompra;
using E1___Sosa_Morales.Services.EstadosTransferencia;
using E1___Sosa_Morales.Services.ListaTransferencias;
using E1___Sosa_Morales.Services.Marcas;
using E1___Sosa_Morales.Services.MovimientosInventario;
using E1___Sosa_Morales.Services.OrdenesCompra;
using E1___Sosa_Morales.Services.Perfil;
using E1___Sosa_Morales.Services.Proveedores;
using E1___Sosa_Morales.Services.Provincias;
using E1___Sosa_Morales.Services.Regiones;
using E1___Sosa_Morales.Services.Roles;
using E1___Sosa_Morales.Services.TiposDocumento;
using E1___Sosa_Morales.Services.TiposMovimiento;
using E1___Sosa_Morales.Services.Users;
using E1___Sosa_Morales.Services.Usuarios;
using Microsoft.AspNetCore.Authentication.Cookies;
using E1___Sosa_Morales.Services.TiposVehiculo;
using E1___Sosa_Morales.Services.Vehiculos;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);



builder.Logging.ClearProviders();

builder.Logging.AddConsole();



builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
builder.Services.AddDbContext<ApplicationDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("bd_ventas")));
builder.Services.AddScoped<ITiposVehiculoService, TiposVehiculoService>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IStockAlertService, StockAlertService>();

builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();

builder.Services.AddScoped<E1___Sosa_Morales.Services.ProductoProveedores.IPrpService,
                           E1___Sosa_Morales.Services.ProductoProveedores.PrpService>();

builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddScoped<ICategoriaService, CategoriaService>();

builder.Services.AddScoped<ISupplierService, SupplierService>();

builder.Services.AddScoped<E1___Sosa_Morales.Services.MarcasProveedor.ISbrService,
                           E1___Sosa_Morales.Services.MarcasProveedor.SbrService>();

builder.Services.AddScoped<IJobPositionService, JobPositionService>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
// A�ade esto junto a tus otros servicios
builder.Services.AddScoped<E1___Sosa_Morales.Services.Productos.IProductoService, E1___Sosa_Morales.Services.Productos.ProductoService>();

builder.Services.AddScoped<IStatusTransferService, StatusTransferService>();

builder.Services.AddScoped<ITransferService, TransferService>();

builder.Services.AddScoped<ITransferDetailService, TransferDetailService>();

builder.Services.AddScoped<IPurchaseStatusService, PurchaseStatusService>();

builder.Services.AddScoped<IPurchaseService, PurchaseService>();

builder.Services.AddScoped<IPurchaseDetailService, PurchaseDetailService>();

builder.Services.AddScoped<IPurchaseWarehouseDetailService, PurchaseWarehouseDetailService>();

builder.Services.AddScoped<IWarehouseService, WarehouseService>();

builder.Services.AddScoped<IWarehouseDetailService, WarehouseDetailService>();

builder.Services.AddScoped<IMovementTypeService, MovementTypeService>();

builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();

builder.Services.AddScoped<ICountryService, CountryService>();

builder.Services.AddScoped<IRegionService, RegionService>();

builder.Services.AddScoped<IProvinceService, ProvinceService>();

builder.Services.AddScoped<IDistrictService, DistrictService>();


builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddScoped<IMarcaService, MarcaService>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IPerfilService, PerfilService>();

builder.Services.AddScoped<ISaleStatusService, SaleStatusService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISaleDetailService, SaleDetailService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>

    {

        options.LoginPath = "/Login";

        options.LogoutPath = "/Login/Logout";

        options.AccessDeniedPath = "/Login";

        options.ExpireTimeSpan = TimeSpan.FromHours(8);

    });



builder.Services.AddAuthorization();



var app = builder.Build();



if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();

}



app.UseHttpsRedirection();

app.UseStaticFiles();



var faviconPath = Path.Combine(app.Environment.WebRootPath, "Public", "Images", "Logo - KMLLogistics.png");



IResult ServeFavicon(HttpContext ctx)

{

    if (!File.Exists(faviconPath))

        return Results.NotFound();



    ctx.Response.Headers.CacheControl = "public,max-age=31536000,immutable";

    return Results.File(faviconPath, "image/png");

}



app.MapGet("/favicon.png", ServeFavicon);

app.MapGet("/favicon.ico", ServeFavicon);



app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();



app.MapControllerRoute(

    name: "default",

    pattern: "{controller=LandingPage}/{action=Index}/{id?}");



app.Run();

