using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace GESTION_INVENTARIO_LICORES_MVC.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class UsuarioController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UsuarioController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            string? nombres = null,
            string? apellidos = null,
            long? idRol = null,
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

            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"estado={estadoFinal}",
                $"orden={Uri.EscapeDataString(orden ?? "DESC")}"
            };

            if (!string.IsNullOrWhiteSpace(nombres))
            {
                queryParams.Add($"nombres={Uri.EscapeDataString(nombres.Trim())}");
            }

            if (!string.IsNullOrWhiteSpace(apellidos))
            {
                queryParams.Add($"apellidos={Uri.EscapeDataString(apellidos.Trim())}");
            }

            if (idRol.HasValue && idRol.Value > 0)
            {
                queryParams.Add($"idRol={idRol.Value}");
            }

            string queryString = string.Join("&", queryParams);
            HttpResponseMessage response = await client.GetAsync($"Usuario?{queryString}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            string content = await response.Content.ReadAsStringAsync();

            PaginatedRespDto<UsuarioRespDto>? respuesta =
                DeserializarContenido<PaginatedRespDto<UsuarioRespDto>>(content);

            if (!response.IsSuccessStatusCode || respuesta == null)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el listado de usuarios."
                );
                respuesta = new PaginatedRespDto<UsuarioRespDto> { PageNumber = pageNumber };
            }

            // Cargar los roles para el ComboBox de Filtros
            await CargarRolesEnViewBagAsync(client, idRol);

            ViewData["CurrentNombres"] = nombres;
            ViewData["CurrentApellidos"] = apellidos;
            ViewData["CurrentIdRol"] = idRol;
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

            HttpResponseMessage response = await client.GetAsync($"Usuario/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el usuario solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener la información del usuario."
                );
                return RedirectToAction(nameof(Index));
            }

            UsuarioRespDto? usuario = DeserializarContenido<UsuarioRespDto>(content);

            if (usuario == null)
            {
                TempData["Error"] = "No se pudo interpretar el usuario recibido.";
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            HttpClient client = CrearCliente();

            await CargarRolesEnViewBagAsync(client);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioReqDto dto)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            HttpClient client = CrearCliente();

            if (!ModelState.IsValid)
            {
                await CargarRolesEnViewBagAsync(client, dto.IdRol);
                return View(dto);
            }

            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("Usuario", requestContent);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo registrar el usuario."
                );
                await CargarRolesEnViewBagAsync(client, dto.IdRol);
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Usuario registrado correctamente.");
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

            HttpResponseMessage response = await client.GetAsync($"Usuario/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el usuario solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el usuario para editar."
                );
                return RedirectToAction(nameof(Index));
            }

            UsuarioRespDto? usuario = DeserializarContenido<UsuarioRespDto>(content);

            if (usuario == null)
            {
                TempData["Error"] = "No se pudo interpretar el usuario recibido.";
                return RedirectToAction(nameof(Index));
            }

            UsuarioUpdateReqDto dto = new UsuarioUpdateReqDto
            {
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Correo = usuario.Correo,
                IdRol = usuario.Rol?.IdRol ?? 0
            };

            await CargarRolesEnViewBagAsync(client, dto.IdRol);
            ViewData["IdUsuario"] = id;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, UsuarioUpdateReqDto dto)
        {
            if (!TieneToken())
            {
                return await RedirigirALogin();
            }

            HttpClient client = CrearCliente();

            if (!ModelState.IsValid)
            {
                await CargarRolesEnViewBagAsync(client, dto.IdRol);
                ViewData["IdUsuario"] = id;
                return View(dto);
            }

            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync($"Usuario/{id}", requestContent);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el usuario solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo actualizar el usuario."
                );
                await CargarRolesEnViewBagAsync(client, dto.IdRol);
                ViewData["IdUsuario"] = id;
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Usuario actualizado correctamente.");
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
                HttpResponseMessage getResp = await client.GetAsync($"Usuario/{id}");
                if (getResp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return await RedirigirALogin();
                }

                if (getResp.IsSuccessStatusCode)
                {
                    string getContent = await getResp.Content.ReadAsStringAsync();
                    UsuarioRespDto? user = DeserializarContenido<UsuarioRespDto>(getContent);
                    if (user != null)
                    {
                        nuevoEstado = !user.Estado;
                    }
                }
            }

            string requestUri = $"Usuario/{id}/estado?estado={nuevoEstado.ToString().ToLower()}";

            HttpRequestMessage request = new HttpRequestMessage(
                new HttpMethod("PATCH"),
                requestUri
            );

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RedirigirALogin();
            }

            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo cambiar el estado del usuario."
                );
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = ObtenerMensaje(
                content,
                $"Usuario {(nuevoEstado ? "activado" : "desactivado")} correctamente."
            );

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarRolesEnViewBagAsync(HttpClient client, long? selectedIdRol = null)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("Rol");
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    List<RolRespDto>? roles = DeserializarContenido<List<RolRespDto>>(content);

                    if (roles != null)
                    {
                        ViewBag.Roles = new SelectList(roles, "IdRol", "Nombre", selectedIdRol);
                        return;
                    }
                }
            }
            catch
            {
                // Manejo silencioso en caso de fallback
            }

            ViewBag.Roles = new SelectList(new List<RolRespDto>(), "IdRol", "Nombre");
        }

        #region Métodos de Apoyo y Configuración del Token

        private string? ObtenerToken()
        {
            // 1. Intentar obtener de la Sesión
            string? token = HttpContext.Session.GetString("Token");

            // 2. Si la sesión expiró/está vacía, rescatarlo de la Claim o Cookie de Autenticación
            if (string.IsNullOrWhiteSpace(token))
            {
                token = User.FindFirst("JWToken")?.Value ?? Request.Cookies["JWToken"];

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
            // Limpiar la sesión y desautenticar la Cookie para evitar rebotes
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