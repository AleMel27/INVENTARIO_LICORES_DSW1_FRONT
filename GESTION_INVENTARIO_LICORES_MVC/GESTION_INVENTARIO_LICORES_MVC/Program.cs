var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("UrbanEyeApi", client =>
{
    client.BaseAddress = new Uri("https://api.urbaneyepe.site/api/");
});
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(
        context.Session.GetString("Token")))
    {
        context.Session.SetString(
            "Token",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiZW1haWwiOiJhZG1pbkBsaWNvcmVzLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFETUlOIiwianRpIjoiNTZiOTQxNzUtYTNmMC00YmVjLTkxNGEtZTVhNDMxNDM5NWI1IiwibmJmIjoxNzg3MDI5NzUxLCJleHAiOjE3ODcwMzMzNTEsImlzcyI6IkdFU1RJT05fSU5WRU5UQVJJT19MSUNPUkVTX0FQSSIsImF1ZCI6IkdFU1RJT05fSU5WRU5UQVJJT19MSUNPUkVTX0NMSUVOVCJ9.Xcj7LWc6QX5efqIbFhBTHIRDrh_mZKXpNIphl-nBZ3c"
        );
    }

    await next();
});


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Categoria}/{action=Index}/{id?}");

app.Run();
