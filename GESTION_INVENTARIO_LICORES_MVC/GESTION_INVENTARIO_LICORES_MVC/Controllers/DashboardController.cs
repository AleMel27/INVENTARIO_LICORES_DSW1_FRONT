using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;
using GESTION_INVENTARIO_LICORES_MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GESTION_INVENTARIO_LICORES_MVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Validar si existe token JWT activo
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            var client = GetClient();

            try
            {
                // 2. Obtener datos de la API (manejando paginación cuando aplica)
                var productosTask = ObtenerTodosPaginadoAsync<ProductoRespDto>(client, "Producto");
                var proveedoresTask = ObtenerTodosPaginadoAsync<ProveedorRespDto>(client, "Proveedor");
                var comprasTask = ObtenerComprasAsync(client);

                await Task.WhenAll(productosTask, proveedoresTask, comprasTask);

                var productos = await productosTask;
                var proveedores = await proveedoresTask;
                var compras = await comprasTask;

                // 3. Mapear la información al DTO propio del Dashboard
                var dashboardDto = new DashboardRespDto
                {
                    TotalProductos = productos.Count(p => p.Estado),
                    TotalProveedores = proveedores.Count(p => p.Estado),
                    ProductosStockBajo = productos.Count(p => p.Estado /* && p.StockActual <= p.StockMinimo */),
                    ValorInventario = productos.Where(p => p.Estado).Sum(p => p.PrecioVenta),

                    AlertasStock = productos
                        .Where(p => p.Estado)
                        .Take(5)
                        .Select(p => new DashboardAlertaStockRespDto
                        {
                            Nombre = p.Nombre,
                            Categoria = p.Categoria?.Nombre ?? "Sin Categoría",
                            StockActual = 0,
                            StockMinimo = p.StockMinimo
                        })
                        .ToList(),

                    UltimasCompras = compras
                        .OrderByDescending(c => c.FechaCompra)
                        .Take(5)
                        .Select(c => new DashboardUltimaCompraRespDto
                        {
                            Codigo = c.NumeroComprobante,
                            Proveedor = c.Proveedor?.RazonSocial ?? "Sin Proveedor",
                            Fecha = c.FechaCompra,
                            Total = c.Total
                        })
                        .ToList()
                };

                return View(new DashboardViewModel { Datos = dashboardDto });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la información del Dashboard: " + ex.Message;
                return View(new DashboardViewModel());
            }
        }

        #region Métodos de Autenticación y Apoyo (JWT & HttpClient)

        private string? ObtenerToken()
        {
            // 1. Intentar obtener de la Sesión
            string? token = HttpContext.Session.GetString("Token");

            // 2. Si la sesión expiró/está vacía, rescatarlo de la Cookie de Autenticación
            if (string.IsNullOrWhiteSpace(token))
            {
                token = User.FindFirst("JWToken")?.Value;

                // Reponer en sesión para subsiguientes peticiones
                if (!string.IsNullOrWhiteSpace(token))
                {
                    HttpContext.Session.SetString("Token", token);
                }
            }

            return token;
        }

        private bool TieneToken()
        {
            return !string.IsNullOrWhiteSpace(ObtenerToken());
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("UrbanEyeApi");
            var token = ObtenerToken();

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private async Task<IActionResult> RedirigirALogin()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Error"] = "Su sesión ha expirado o no está autorizado. Inicie sesión nuevamente.";
            return RedirectToAction("Login", "Auth");
        }

        private static async Task<List<T>> ObtenerTodosPaginadoAsync<T>(HttpClient client, string endpoint)
        {
            var resultado = new List<T>();

            // Forzamos un pageSize alto para traer todos los registros de un solo golpe si la API lo permite,
            // o empezamos desde la página 1.
            var primera = await client.GetAsync($"{endpoint}?pageNumber=1&pageSize=100");
            if (!primera.IsSuccessStatusCode) return resultado;

            var jsonPrimera = await primera.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResultRespDto<T>>(jsonPrimera, _jsonOptions);
            if (paged == null || paged.Items == null) return resultado;

            resultado.AddRange(paged.Items);

            for (int pagina = 2; pagina <= paged.TotalPages; pagina++)
            {
                var response = await client.GetAsync($"{endpoint}?pageNumber={pagina}&pageSize=100");
                if (!response.IsSuccessStatusCode) continue;

                var jsonSig = await response.Content.ReadAsStringAsync();
                var siguiente = JsonSerializer.Deserialize<PagedResultRespDto<T>>(jsonSig, _jsonOptions);
                if (siguiente?.Items != null)
                {
                    resultado.AddRange(siguiente.Items);
                }
            }
            return resultado;
        }

        private static async Task<List<CompraRespDto>> ObtenerComprasAsync(HttpClient client)
        {
            var response = await client.GetAsync("Compra");
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();

            // Intenta deserializar si la respuesta es paginada o un listado directo
            try
            {
                var paged = JsonSerializer.Deserialize<PagedResultRespDto<CompraRespDto>>(json, _jsonOptions);
                if (paged?.Items != null && paged.Items.Any()) return paged.Items;
            }
            catch
            {
                // Si no es un PagedResultRespDto, intenta como List<CompraRespDto>
            }

            return JsonSerializer.Deserialize<List<CompraRespDto>>(json, _jsonOptions) ?? [];
        }

        #endregion
    }
}