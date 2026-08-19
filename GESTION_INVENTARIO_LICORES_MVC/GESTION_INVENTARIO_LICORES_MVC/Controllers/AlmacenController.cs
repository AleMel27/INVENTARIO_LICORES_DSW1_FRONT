using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace GESTION_INVENTARIO_LICORES_MVC.Controllers
{
    public class AlmacenController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AlmacenController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            string? nombre = null,
            string? ubicacion = null,
            int? estado = null,
            string orden = "DESC"
        )
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            HttpClient client = CrearCliente();

            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            int estadoFinal = 0;

            if (Request.Query.ContainsKey("estado") && int.TryParse(Request.Query["estado"], out int estadoQuery))
            {
                estadoFinal = estadoQuery;
            }
            else if (estado.HasValue)
            {
                estadoFinal = estado.Value;
            }

            // Construcción dinámica de la Query String hacia la API
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"estado={estadoFinal}",
                $"orden={Uri.EscapeDataString(orden ?? "DESC")}"
            };

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                queryParams.Add($"nombre={Uri.EscapeDataString(nombre.Trim())}");
            }

            if (!string.IsNullOrWhiteSpace(ubicacion))
            {
                queryParams.Add($"ubicacion={Uri.EscapeDataString(ubicacion.Trim())}");
            }

            string queryString = string.Join("&", queryParams);
            HttpResponseMessage response = await client.GetAsync($"Almacen?{queryString}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            PaginatedRespDto<AlmacenRespDto>? respuesta =
                DeserializarContenido<PaginatedRespDto<AlmacenRespDto>>(content);

            if (!response.IsSuccessStatusCode || respuesta == null)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el listado de almacenes."
                );
                respuesta = new PaginatedRespDto<AlmacenRespDto> { PageNumber = pageNumber };
            }

            // Guardar valores en ViewData para que la vista preserve los campos cargados
            ViewData["CurrentNombre"] = nombre;
            ViewData["CurrentUbicacion"] = ubicacion;
            ViewData["CurrentEstado"] = estadoFinal;
            ViewData["CurrentOrden"] = string.Equals(orden, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

            return View(respuesta);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            HttpClient client = CrearCliente();

            HttpResponseMessage response = await client.GetAsync($"Almacen/{id}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el almacén solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener la información del almacén."
                );
                return RedirectToAction(nameof(Index));
            }

            AlmacenRespDto? almacen = DeserializarContenido<AlmacenRespDto>(content);

            if (almacen == null)
            {
                TempData["Error"] = "No se pudo interpretar el almacén recibido.";
                return RedirectToAction(nameof(Index));
            }

            return View(almacen);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlmacenReqCreateDto dto)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            HttpClient client = CrearCliente();
            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("Almacen", requestContent);
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo registrar el almacén."
                );
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Almacén registrado correctamente.");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            HttpClient client = CrearCliente();

            HttpResponseMessage response = await client.GetAsync($"Almacen/{id}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el almacén solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el almacén para editar."
                );
                return RedirectToAction(nameof(Index));
            }

            AlmacenRespDto? almacen = DeserializarContenido<AlmacenRespDto>(content);

            if (almacen == null)
            {
                TempData["Error"] = "No se pudo interpretar el almacén recibido.";
                return RedirectToAction(nameof(Index));
            }

            AlmacenUpdateReqDto dto = new AlmacenUpdateReqDto
            {
                Nombre = almacen.Nombre,
                Descripcion = almacen.Descripcion
            };

            ViewData["IdAlmacen"] = id;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AlmacenUpdateReqDto dto)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            if (!ModelState.IsValid)
            {
                ViewData["IdAlmacen"] = id;
                return View(dto);
            }

            HttpClient client = CrearCliente();
            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync($"Almacen/{id}", requestContent);
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el almacén solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo actualizar el almacén."
                );
                ViewData["IdAlmacen"] = id;
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Almacén actualizado correctamente.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long id, bool? estadoActual)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            HttpClient client = CrearCliente();

            bool nuevoEstado = true;
            if (estadoActual.HasValue)
            {
                nuevoEstado = !estadoActual.Value;
            }
            else
            {
                HttpResponseMessage getResp = await client.GetAsync($"Almacen/{id}");
                if (getResp.IsSuccessStatusCode)
                {
                    string getContent = await getResp.Content.ReadAsStringAsync();
                    AlmacenRespDto? alm = DeserializarContenido<AlmacenRespDto>(getContent);
                    if (alm != null)
                    {
                        nuevoEstado = !alm.Estado;
                    }
                }
            }

            string requestUri = $"Almacen/{id}/estado?estado={nuevoEstado.ToString().ToLower()}";

            HttpRequestMessage request = new HttpRequestMessage(
                new HttpMethod("PATCH"),
                requestUri
            );

            HttpResponseMessage response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo cambiar el estado del almacén."
                );
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = ObtenerMensaje(
                content,
                $"Almacén {(nuevoEstado ? "activado" : "desactivado")} correctamente."
            );

            return RedirectToAction(nameof(Index));
        }

        #region Métodos de Apoyo y Configuración del Token

        private string? ObtenerToken()
        {
            string? token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrWhiteSpace(token))
            {
                token = User.FindFirst("JWToken")?.Value;

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

        private HttpClient CrearCliente()
        {
            HttpClient client = _httpClientFactory.CreateClient("UrbanEyeApi");
            string? token = ObtenerToken();

            if (!string.IsNullOrWhiteSpace(token))
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

        private static T? DeserializarContenido<T>(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        private static string ObtenerMensaje(string content, string mensajePorDefecto)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return mensajePorDefecto;
            }

            try
            {
                JObject json = JObject.Parse(content);
                string? message = json["message"]?.ToString();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }

                string? title = json["title"]?.ToString();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    return title;
                }
            }
            catch (JsonException)
            {
                return mensajePorDefecto;
            }

            return mensajePorDefecto;
        }

        private static string ObtenerMensajeErrorHttp(
            HttpStatusCode statusCode,
            string content,
            string mensajePorDefecto
        )
        {
            string mensajeBackend = ObtenerMensaje(content, string.Empty);

            if (!string.IsNullOrWhiteSpace(mensajeBackend))
            {
                return mensajeBackend;
            }

            return statusCode switch
            {
                HttpStatusCode.BadRequest => "La solicitud enviada no es válida.",
                HttpStatusCode.Forbidden => "No tiene permisos para realizar esta operación.",
                HttpStatusCode.NotFound => "No se encontró el recurso solicitado.",
                HttpStatusCode.InternalServerError => "Ocurrió un error interno en el servidor.",
                _ => mensajePorDefecto
            };
        }
        #endregion
    }
}