using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace GESTION_INVENTARIO_LICORES_MVC.Controllers
{
    public class MarcaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MarcaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            string? nombre = null,
            string? paisOrigen = null,
            int? estado = null,
            string orden = "DESC"
        )
        {
            HttpClient client = CrearCliente();

            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            int estadoFinal = 0; // 0 = Activos por defecto

            if (Request.Query.ContainsKey("estado") && int.TryParse(Request.Query["estado"], out int estadoQuery))
            {
                estadoFinal = estadoQuery;
            }
            else if (estado.HasValue)
            {
                estadoFinal = estado.Value;
            }

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

            if (!string.IsNullOrWhiteSpace(paisOrigen))
            {
                queryParams.Add($"paisOrigen={Uri.EscapeDataString(paisOrigen.Trim())}");
            }

            string queryString = string.Join("&", queryParams);
            HttpResponseMessage response = await client.GetAsync($"Marca?{queryString}");
            string content = await response.Content.ReadAsStringAsync();

            PaginatedRespDto<MarcaRespDto>? respuesta =
                DeserializarContenido<PaginatedRespDto<MarcaRespDto>>(content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (!response.IsSuccessStatusCode || respuesta == null)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el listado de marcas."
                );
                respuesta = new PaginatedRespDto<MarcaRespDto> { PageNumber = pageNumber };
            }

            ViewData["CurrentNombre"] = nombre;
            ViewData["CurrentPaisOrigen"] = paisOrigen;
            ViewData["CurrentEstado"] = estadoFinal;
            ViewData["CurrentOrden"] = string.Equals(orden, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

            return View(respuesta);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            HttpClient client = CrearCliente();

            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            HttpResponseMessage response = await client.GetAsync($"Marca/{id}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró la marca solicitada.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener la información de la marca."
                );
                return RedirectToAction(nameof(Index));
            }

            MarcaRespDto? marca = DeserializarContenido<MarcaRespDto>(content);

            if (marca == null)
            {
                TempData["Error"] = "No se pudo interpretar la marca recibida.";
                return RedirectToAction(nameof(Index));
            }

            return View(marca);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarcaReqDto dto)
        {
            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            HttpClient client = CrearCliente();
            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("Marca", requestContent);
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo registrar la marca."
                );
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Marca registrada correctamente.");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            HttpClient client = CrearCliente();

            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            HttpResponseMessage response = await client.GetAsync($"Marca/{id}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró la marca solicitada.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener la marca para editar."
                );
                return RedirectToAction(nameof(Index));
            }

            MarcaRespDto? marca = DeserializarContenido<MarcaRespDto>(content);

            if (marca == null)
            {
                TempData["Error"] = "No se pudo interpretar la marca recibida.";
                return RedirectToAction(nameof(Index));
            }

            MarcaUpdateReqDto dto = new MarcaUpdateReqDto
            {
                Nombre = marca.Nombre,
                PaisOrigen = marca.PaisOrigen
            };

            ViewData["IdMarca"] = id;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, MarcaUpdateReqDto dto)
        {
            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            if (!ModelState.IsValid)
            {
                ViewData["IdMarca"] = id;
                return View(dto);
            }

            HttpClient client = CrearCliente();
            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync($"Marca/{id}", requestContent);
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró la marca solicitada.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo actualizar la marca."
                );
                ViewData["IdMarca"] = id;
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Marca actualizada correctamente.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long id, bool? estadoActual)
        {
            HttpClient client = CrearCliente();

            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            bool nuevoEstado = true;
            if (estadoActual.HasValue)
            {
                nuevoEstado = !estadoActual.Value;
            }
            else
            {
                HttpResponseMessage getResp = await client.GetAsync($"Marca/{id}");
                if (getResp.IsSuccessStatusCode)
                {
                    string getContent = await getResp.Content.ReadAsStringAsync();
                    MarcaRespDto? marca = DeserializarContenido<MarcaRespDto>(getContent);
                    if (marca != null)
                    {
                        nuevoEstado = !marca.Estado;
                    }
                }
            }

            string requestUri = $"Marca/{id}/estado?estado={nuevoEstado.ToString().ToLower()}";

            HttpRequestMessage request = new HttpRequestMessage(
                new HttpMethod("PATCH"),
                requestUri
            );

            HttpResponseMessage response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo cambiar el estado de la marca."
                );
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = ObtenerMensaje(
                content,
                $"Marca {(nuevoEstado ? "activada" : "desactivada")} correctamente."
            );

            return RedirectToAction(nameof(Index));
        }

        private HttpClient CrearCliente()
        {
            HttpClient client = _httpClientFactory.CreateClient("UrbanEyeApi");
            string? token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private bool TieneToken()
        {
            string? token = HttpContext.Session.GetString("Token");
            return !string.IsNullOrWhiteSpace(token);
        }

        private IActionResult RedirigirALogin()
        {
            TempData["Error"] = "Debe iniciar sesión para continuar.";
            return RedirectToAction("Index", "Home");
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
    }
}