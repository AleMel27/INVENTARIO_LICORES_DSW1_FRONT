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
    public class CompraController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public CompraController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CompraFiltroReqDto filtro)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            var client = GetClient();

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["pageNumber"] = filtro.PageNumber.ToString();
            query["orden"] = filtro.Orden;

            if (!string.IsNullOrWhiteSpace(filtro.Estado))
                query["estado"] = filtro.Estado;

            if (filtro.IdTipoComprobante is > 0)
                query["idTipoComprobante"] = filtro.IdTipoComprobante.ToString();

            if (filtro.IdAlmacen is > 0)
                query["idAlmacen"] = filtro.IdAlmacen.ToString();

            if (filtro.Fecha.HasValue)
                query["fecha"] = filtro.Fecha.Value.ToString("yyyy-MM-dd");

            if (!string.IsNullOrWhiteSpace(filtro.RazonSocial))
                query["razonSocial"] = filtro.RazonSocial;

            if (!string.IsNullOrWhiteSpace(filtro.NumeroComprobante))
                query["numeroComprobante"] = filtro.NumeroComprobante;

            var response = await client.GetAsync($"Compra?{query}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            var viewModel = new CompraIndexViewModel { Filtro = filtro };

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                viewModel.Resultado = JsonSerializer.Deserialize<PagedResultRespDto<CompraRespDto>>(json, _jsonOptions)
                                       ?? new PagedResultRespDto<CompraRespDto>();
            }
            else
            {
                TempData["Error"] = "No se pudo obtener el listado de compras.";
            }

            viewModel.TiposComprobante = await ObtenerTiposComprobanteAsync(client);
            viewModel.Almacenes = await ObtenerAlmacenesAsync(client);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            if (id <= 0) return NotFound();

            var client = GetClient();
            var response = await client.GetAsync($"Compra/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo encontrar la compra solicitada o expiró el acceso.";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var compraDetalle = JsonSerializer.Deserialize<CompraDetalleRespDto>(json, _jsonOptions);

            if (compraDetalle == null) return NotFound();

            return View(compraDetalle);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            var client = GetClient();
            var viewModel = await ConstruirViewModelCreateAsync(client);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompraReqDto compra)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            var client = GetClient();
            var token = ObtenerToken() ?? string.Empty;
            var idUsuario = GetUsuarioIdDesdeToken(token);

            // El form no envía IdUsuario, así que el binder lo deja en 0 y falla su [Range].
            // Lo quitamos del ModelState y lo asignamos nosotros desde el token.
            ModelState.Remove(nameof(compra.IdUsuario));

            if (idUsuario == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar al usuario logueado. Vuelve a iniciar sesión.");
            }
            else
            {
                compra.IdUsuario = idUsuario.Value;
            }

            if (!ModelState.IsValid)
            {
                var viewModel = await ConstruirViewModelCreateAsync(client, compra);
                return View(viewModel);
            }

            var content = new StringContent(
                JsonSerializer.Serialize(compra, _jsonOptions), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Compra", content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Compra registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"No se pudo registrar la compra: {errorBody}");

            var vmError = await ConstruirViewModelCreateAsync(client, compra);
            return View(vmError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(long id, string nuevoEstado)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            // Validamos que el estado esté dentro de los permitidos
            var estadosValidos = new[] { "PENDIENTE", "RECIBIDA", "CANCELADA", "ANULADA" };
            if (string.IsNullOrWhiteSpace(nuevoEstado) || !estadosValidos.Contains(nuevoEstado.ToUpper()))
            {
                TempData["Error"] = "El estado seleccionado no es válido.";
                return RedirectToAction(nameof(Index));
            }

            var client = GetClient();

            var payload = JsonSerializer.Serialize(new { estado = nuevoEstado.ToUpper() }, _jsonOptions);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"Compra/{id}/estado")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = $"El estado de la compra #{id} ha sido actualizado a {nuevoEstado.ToUpper()}.";
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"No se pudo cambiar el estado: {errorBody}";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Métodos de Apoyo y Configuración del Token

        private string? ObtenerToken()
        {
            // 1. Intentar obtener de la Sesión
            string? token = HttpContext.Session.GetString("Token");

            // 2. Si la sesión expiró/está vacía, rescatarlo del Claim de la Cookie
            if (string.IsNullOrWhiteSpace(token))
            {
                token = User.FindFirst("JWToken")?.Value;

                // Reponer en sesión
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

        // Decodifica el "sub" (idUsuario) del JWT, sin validar firma
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

        // Recorre todas las páginas de un endpoint paginado y devuelve la lista completa
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

        private async Task<CompraCreateViewModel> ConstruirViewModelCreateAsync(HttpClient client, CompraReqDto? compra = null)
        {
            return new CompraCreateViewModel
            {
                Compra = compra ?? new CompraReqDto(),
                Proveedores = await ObtenerTodosPaginadoAsync<ProveedorResumenRespDto>(client, "Proveedor"),
                TiposComprobante = await ObtenerTiposComprobanteAsync(client),
                Almacenes = await ObtenerTodosPaginadoAsync<AlmacenInventarioRespDto>(client, "Almacen"),
                Productos = await ObtenerTodosPaginadoAsync<ProductoResumenRespDto>(client, "Producto")
            };
        }

        private static async Task<List<TipoComprobanteRespDto>> ObtenerTiposComprobanteAsync(HttpClient client)
        {
            var response = await client.GetAsync("TipoComprobante");
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TipoComprobanteRespDto>>(json, _jsonOptions) ?? [];
        }

        private static async Task<List<AlmacenInventarioRespDto>> ObtenerAlmacenesAsync(HttpClient client)
        {
            var response = await client.GetAsync("Almacen");
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResultRespDto<AlmacenInventarioRespDto>>(json, _jsonOptions);
            return paged?.Items ?? [];
        }
        #endregion
    }
}