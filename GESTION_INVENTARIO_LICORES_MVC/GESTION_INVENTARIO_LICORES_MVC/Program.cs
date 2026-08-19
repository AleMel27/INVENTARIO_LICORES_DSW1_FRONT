using GESTION_INVENTARIO_LICORES_MVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios MVC (Controladores con Vistas)
builder.Services.AddControllersWithViews();

// 2. Registración de HttpClients:
// A) Cliente nombrado "UrbanEyeApi" para controladores como CategoriaController que usan IHttpClientFactory
builder.Services.AddHttpClient("UrbanEyeApi", client =>
{
    client.BaseAddress = new Uri("https://api.urbaneyepe.site/api/");
});

// B) Cliente tipado para AuthApiService usado por AuthController
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.urbaneyepe.site/api/");
});

// 3. Configurar Autenticación basada en Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/Error";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// 4. Configurar Caché y Sesión (Aquí se guardará el token de forma dinámica)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. Middlewares de Sesión y Seguridad (En el orden estricto requerido)
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 6. Ruta predeterminada hacia la pantalla de Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();