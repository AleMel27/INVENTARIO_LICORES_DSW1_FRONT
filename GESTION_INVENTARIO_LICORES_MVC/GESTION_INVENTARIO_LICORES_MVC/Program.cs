using GESTION_INVENTARIO_LICORES_MVC.Services; // Asegúrate de incluir el namespace de tus servicios

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registra la API base e inyecta la interfaz IAuthApiService asociada a su implementación
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>("UrbanEyeApi", client =>
{
    client.BaseAddress = new Uri("https://api.urbaneyepe.site/api/");
});

// Configuración de Caché y Sesión
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

app.UseSession();

// Middleware de prueba para inyectar token por defecto
app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Session.GetString("Token")))
    {
        context.Session.SetString(
            "Token",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiZW1haWwiOiJhZG1pbkBjZW8ubGljb3Jlcy5wZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFETUlOIiwianRpIjoiMjM1OWVmODktZTJjMC00YTFmLWJiNDctYjJkZjNkMGMwYjc4IiwibmJmIjoxNzg3MDcxMzIzLCJleHAiOjE3ODcwNzQ5MjMsImlzcyI6IkdFU1RJT05fSU5WRU5UQVJJT19MSUNPUkVTX0FQSSIsImF1ZCI6IkdFU1RJT05fSU5WRU5UQVJJT19MSUNPUkVTX0NMSUVOVCJ9.NQBLplf2ETtBX4qykXXENmy39Y_E7oTWni0cULCruVE"
        );
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();