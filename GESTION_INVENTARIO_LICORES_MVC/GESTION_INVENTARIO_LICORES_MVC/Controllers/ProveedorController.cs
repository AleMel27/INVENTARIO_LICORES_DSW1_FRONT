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
    public class ProveedorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProveedorController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            // string? ruc = null,
            // string? razonSocial = null,
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

            // if (!string.IsNullOrWhiteSpace(ruc))
            // {
            //     queryParams.Add($"ruc={Uri.EscapeDataString(ruc.Trim())}");
            // }

            // if (!string.IsNullOrWhiteSpace(razonSocial))
            // {
            //     queryParams.Add($"razonSocial={Uri.EscapeDataString(razonSocial.Trim())}");
            // }

            string queryString = string.Join("&", queryParams);
            HttpResponseMessage response = await client.GetAsync($"Proveedor?{queryString}");
            string content = await response.Content.ReadAsStringAsync();

            PaginatedRespDto<ProveedorRespDto>? respuesta =
                DeserializarContenido<PaginatedRespDto<ProveedorRespDto>>(content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (!response.IsSuccessStatusCode || respuesta == null)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el listado de proveedores."
                );
                respuesta = new PaginatedRespDto<ProveedorRespDto> { PageNumber = pageNumber };
            }

            // ViewData["CurrentRuc"] = ruc;
            // ViewData["CurrentRazonSocial"] = razonSocial;
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

            HttpResponseMessage response = await client.GetAsync($"Proveedor/{id}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el proveedor solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener la información del proveedor."
                );
                return RedirectToAction(nameof(Index));
            }

            ProveedorRespDto? proveedor = DeserializarContenido<ProveedorRespDto>(content);

            if (proveedor == null)
            {
                TempData["Error"] = "No se pudo interpretar el proveedor recibido.";
                return RedirectToAction(nameof(Index));
            }

            return View(proveedor);
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
        public async Task<IActionResult> Create(ProveedorReqDto dto)
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

            HttpResponseMessage response = await client.PostAsync("Proveedor", requestContent);
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
                    "No se pudo registrar el proveedor."
                );
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Proveedor registrado correctamente.");
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

            HttpResponseMessage response = await client.GetAsync($"Proveedor/{id}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el proveedor solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el proveedor para editar."
                );
                return RedirectToAction(nameof(Index));
            }

            ProveedorRespDto? proveedor = DeserializarContenido<ProveedorRespDto>(content);

            if (proveedor == null)
            {
                TempData["Error"] = "No se pudo interpretar el proveedor recibido.";
                return RedirectToAction(nameof(Index));
            }

            ProveedorUpdateReqDto dto = new ProveedorUpdateReqDto
            {
                Ruc = proveedor.Ruc,
                RazonSocial = proveedor.RazonSocial,
                Telefono = proveedor.Telefono,
                Correo = proveedor.Correo,
                Direccion = proveedor.Direccion
            };

            ViewData["IdProveedor"] = id;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, ProveedorUpdateReqDto dto)
        {
            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            if (!ModelState.IsValid)
            {
                ViewData["IdProveedor"] = id;
                return View(dto);
            }

            HttpClient client = CrearCliente();
            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync($"Proveedor/{id}", requestContent);
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el proveedor solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo actualizar el proveedor."
                );
                ViewData["IdProveedor"] = id;
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Proveedor actualizado correctamente.");
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
                HttpResponseMessage getResp = await client.GetAsync($"Proveedor/{id}");
                if (getResp.IsSuccessStatusCode)
                {
                    string getContent = await getResp.Content.ReadAsStringAsync();
                    ProveedorRespDto? prov = DeserializarContenido<ProveedorRespDto>(getContent);
                    if (prov != null)
                    {
                        nuevoEstado = !prov.Estado;
                    }
                }
            }

            string requestUri = $"Proveedor/{id}/estado?estado={nuevoEstado.ToString().ToLower()}";

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
                    "No se pudo cambiar el estado del proveedor."
                );
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = ObtenerMensaje(
                content,
                $"Proveedor {(nuevoEstado ? "activado" : "desactivado")} correctamente."
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