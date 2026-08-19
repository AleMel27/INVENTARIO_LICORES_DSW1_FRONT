using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;
using GESTION_INVENTARIO_LICORES_MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace GESTION_INVENTARIO_LICORES_MVC.Controllers
{
    public class InventarioController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public InventarioController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(InventarioFiltroReqDto filtro)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            var client = GetClient();

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["pageNumber"] = filtro.PageNumber.ToString();
            query["orden"] = filtro.Orden;

            if (!string.IsNullOrWhiteSpace(filtro.NombreProducto))
                query["nombreProducto"] = filtro.NombreProducto;

            if (!string.IsNullOrWhiteSpace(filtro.CodigoProducto))
                query["codigoProducto"] = filtro.CodigoProducto;

            if (filtro.IdAlmacen is > 0)
                query["idAlmacen"] = filtro.IdAlmacen.ToString();

            var response = await client.GetAsync($"Inventario?{query}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            var viewModel = new InventarioIndexViewModel { Filtro = filtro };

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                viewModel.Resultado = JsonSerializer.Deserialize<PagedResultRespDto<InventarioRespDto>>(json, _jsonOptions)
                                       ?? new PagedResultRespDto<InventarioRespDto>();
            }
            else
            {
                TempData["Error"] = "No se pudo consultar el inventario.";
            }

            viewModel.Almacenes = await ObtenerAlmacenesAsync(client);
            viewModel.Productos = await ObtenerTodosPaginadoAsync<ProductoResumenRespDto>(client, "Producto");
            viewModel.TiposMovimiento = await ObtenerTiposMovimientoAsync(client);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventarioReqDto request)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Verifique los datos ingresados para la asignación de inventario.";
                return RedirectToAction(nameof(Index));
            }

            var client = GetClient();
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Inventario", content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Producto asignado al almacén correctamente.";
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Error al asignar producto: {errorBody}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajustar(long idInventario, AjusteInventarioReqDto ajuste)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            var client = GetClient();
            var token = ObtenerToken() ?? string.Empty;
            var idUsuario = GetUsuarioIdDesdeToken(token);

            ModelState.Remove(nameof(ajuste.IdUsuario));

            if (idUsuario == null)
            {
                TempData["Error"] = "Sesión no válida para realizar la operación.";
                return RedirectToAction(nameof(Index));
            }

            ajuste.IdUsuario = idUsuario.Value;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revise los campos enviados para el ajuste.";
                return RedirectToAction(nameof(Index));
            }

            var content = new StringContent(
                JsonSerializer.Serialize(ajuste, _jsonOptions), Encoding.UTF8, "application/json");

            // Envío PATCH a /api/Inventario/{idInventario}/ajuste según Swagger
            var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), $"Inventario/{idInventario}/ajuste")
            {
                Content = content
            };

            var response = await client.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Ajuste de inventario realizado con éxito.";
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"No se pudo realizar el ajuste: {errorBody}";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Métodos de Apoyo y Configuración del Token

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
            // Limpiar la sesión y desautenticar la Cookie para evitar rebotes hacia Home/Index
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Error"] = "Su sesión ha expirado o no está autorizado. Inicie sesión nuevamente.";
            return RedirectToAction("Login", "Auth");
        }

        private static long? GetUsuarioIdDesdeToken(string token)
        {
            try
            {
                var partes = token.Split('.');
                if (partes.Length < 2) return null;

                var payload = partes[1].Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var bytes = Convert.FromBase64String(payload);
                using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(bytes));

                if (doc.RootElement.TryGetProperty("sub", out var sub) &&
                    long.TryParse(sub.GetString(), out var idUsuario))
                {
                    return idUsuario;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<List<T>> ObtenerTodosPaginadoAsync<T>(HttpClient client, string endpoint)
        {
            var resultado = new List<T>();

            var primera = await client.GetAsync($"{endpoint}?pageNumber=1");
            if (!primera.IsSuccessStatusCode) return resultado;

            var paged = JsonSerializer.Deserialize<PagedResultRespDto<T>>(
                await primera.Content.ReadAsStringAsync(), _jsonOptions);
            if (paged == null) return resultado;

            resultado.AddRange(paged.Items);

            for (int pagina = 2; pagina <= paged.TotalPages; pagina++)
            {
                var response = await client.GetAsync($"{endpoint}?pageNumber={pagina}");
                if (!response.IsSuccessStatusCode) continue;

                var siguiente = JsonSerializer.Deserialize<PagedResultRespDto<T>>(
                    await response.Content.ReadAsStringAsync(), _jsonOptions);
                if (siguiente != null) resultado.AddRange(siguiente.Items);
            }

            return resultado;
        }

        private static async Task<List<AlmacenInventarioRespDto>> ObtenerAlmacenesAsync(HttpClient client)
        {
            var response = await client.GetAsync("Almacen");
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResultRespDto<AlmacenInventarioRespDto>>(json, _jsonOptions);
            return paged?.Items ?? [];
        }

        private static async Task<List<TipoMovimientoRespDto>> ObtenerTiposMovimientoAsync(HttpClient client)
        {
            var response = await client.GetAsync("TipoMovimiento");
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TipoMovimientoRespDto>>(json, _jsonOptions) ?? [];
        }

        #endregion
    }
}