using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace GESTION_INVENTARIO_LICORES_MVC.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            string? codigo = null,
            string? nombre = null,
            long? idCategoria = null,
            long? idMarca = null,
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

            if (!string.IsNullOrWhiteSpace(codigo))
            {
                queryParams.Add($"codigo={Uri.EscapeDataString(codigo.Trim())}");
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                queryParams.Add($"nombre={Uri.EscapeDataString(nombre.Trim())}");
            }

            if (idCategoria.HasValue && idCategoria.Value > 0)
            {
                queryParams.Add($"idCategoria={idCategoria.Value}");
            }

            if (idMarca.HasValue && idMarca.Value > 0)
            {
                queryParams.Add($"idMarca={idMarca.Value}");
            }

            string queryString = string.Join("&", queryParams);
            HttpResponseMessage response = await client.GetAsync($"Producto?{queryString}");
            string content = await response.Content.ReadAsStringAsync();

            PaginatedRespDto<ProductoRespDto>? respuesta =
                DeserializarContenido<PaginatedRespDto<ProductoRespDto>>(content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (!response.IsSuccessStatusCode || respuesta == null)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el listado de productos."
                );
                respuesta = new PaginatedRespDto<ProductoRespDto> { PageNumber = pageNumber };
            }

            // Cargar Categóras y Marcas para los ComboBoxes de Filtros
            await CargarCombosEnViewBagAsync(client, idCategoria, idMarca);

            ViewData["CurrentCodigo"] = codigo;
            ViewData["CurrentNombre"] = nombre;
            ViewData["CurrentIdCategoria"] = idCategoria;
            ViewData["CurrentIdMarca"] = idMarca;
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

            HttpResponseMessage response = await client.GetAsync($"Producto/{id}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el producto solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener la información del producto."
                );
                return RedirectToAction(nameof(Index));
            }

            ProductoRespDto? producto = DeserializarContenido<ProductoRespDto>(content);

            if (producto == null)
            {
                TempData["Error"] = "No se pudo interpretar el producto recibido.";
                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            HttpClient client = CrearCliente();

            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            await CargarCombosEnViewBagAsync(client);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoReqDto dto)
        {
            HttpClient client = CrearCliente();

            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            if (!ModelState.IsValid)
            {
                await CargarCombosEnViewBagAsync(client, dto.IdCategoria, dto.IdMarca);
                return View(dto);
            }

            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("Producto", requestContent);
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
                    "No se pudo registrar el producto."
                );
                await CargarCombosEnViewBagAsync(client, dto.IdCategoria, dto.IdMarca);
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Producto registrado correctamente.");
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

            HttpResponseMessage response = await client.GetAsync($"Producto/{id}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el producto solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo obtener el producto para editar."
                );
                return RedirectToAction(nameof(Index));
            }

            ProductoRespDto? producto = DeserializarContenido<ProductoRespDto>(content);

            if (producto == null)
            {
                TempData["Error"] = "No se pudo interpretar el producto recibido.";
                return RedirectToAction(nameof(Index));
            }

            ProductoUpdateReqDto dto = new ProductoUpdateReqDto
            {
                IdCategoria = producto.Categoria?.IdCategoria ?? 0,
                IdMarca = producto.Marca?.IdMarca ?? 0,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                CapacidadMl = producto.CapacidadMl,
                GradoAlcoholico = producto.GradoAlcoholico,
                PrecioVenta = producto.PrecioVenta,
                StockMinimo = producto.StockMinimo
            };

            await CargarCombosEnViewBagAsync(client, dto.IdCategoria, dto.IdMarca);
            ViewData["IdProducto"] = id;
            ViewData["CodigoProducto"] = producto.Codigo;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, ProductoUpdateReqDto dto)
        {
            HttpClient client = CrearCliente();

            if (!TieneToken())
            {
                return RedirigirALogin();
            }

            if (!ModelState.IsValid)
            {
                await CargarCombosEnViewBagAsync(client, dto.IdCategoria, dto.IdMarca);
                ViewData["IdProducto"] = id;
                return View(dto);
            }

            string json = JsonConvert.SerializeObject(dto);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync($"Producto/{id}", requestContent);
            string content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirALogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = ObtenerMensaje(content, "No se encontró el producto solicitado.");
                return RedirectToAction(nameof(Index));
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = ObtenerMensajeErrorHttp(
                    response.StatusCode,
                    content,
                    "No se pudo actualizar el producto."
                );
                await CargarCombosEnViewBagAsync(client, dto.IdCategoria, dto.IdMarca);
                ViewData["IdProducto"] = id;
                return View(dto);
            }

            TempData["Success"] = ObtenerMensaje(content, "Producto actualizado correctamente.");
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
                HttpResponseMessage getResp = await client.GetAsync($"Producto/{id}");
                if (getResp.IsSuccessStatusCode)
                {
                    string getContent = await getResp.Content.ReadAsStringAsync();
                    ProductoRespDto? prod = DeserializarContenido<ProductoRespDto>(getContent);
                    if (prod != null)
                    {
                        nuevoEstado = !prod.Estado;
                    }
                }
            }

            string requestUri = $"Producto/{id}/estado?estado={nuevoEstado.ToString().ToLower()}";

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
                    "No se pudo cambiar el estado del producto."
                );
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = ObtenerMensaje(
                content,
                $"Producto {(nuevoEstado ? "activado" : "desactivado")} correctamente."
            );

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombosEnViewBagAsync(HttpClient client, long? selectedCategoriaId = null, long? selectedMarcaId = null)
        {
            // Cargar Categorías (Solo Activas para selección)
            try
            {
                HttpResponseMessage catResponse = await client.GetAsync("Categoria?estado=0");
                if (catResponse.IsSuccessStatusCode)
                {
                    string catContent = await catResponse.Content.ReadAsStringAsync();
                    var paginatedCats = DeserializarContenido<PaginatedRespDto<CategoriaResumenRespDto>>(catContent);
                    var listCats = paginatedCats?.Items ?? new List<CategoriaResumenRespDto>();
                    ViewBag.Categorias = new SelectList(listCats, "IdCategoria", "Nombre", selectedCategoriaId);
                }
                else
                {
                    ViewBag.Categorias = new SelectList(new List<CategoriaResumenRespDto>(), "IdCategoria", "Nombre");
                }
            }
            catch
            {
                ViewBag.Categorias = new SelectList(new List<CategoriaResumenRespDto>(), "IdCategoria", "Nombre");
            }

            // Cargar Marcas (Solo Activas para selección)
            try
            {
                HttpResponseMessage marcaResponse = await client.GetAsync("Marca?estado=0");
                if (marcaResponse.IsSuccessStatusCode)
                {
                    string marcaContent = await marcaResponse.Content.ReadAsStringAsync();
                    var paginatedMarcas = DeserializarContenido<PaginatedRespDto<MarcaResumenRespDto>>(marcaContent);
                    var listMarcas = paginatedMarcas?.Items ?? new List<MarcaResumenRespDto>();
                    ViewBag.Marcas = new SelectList(listMarcas, "IdMarca", "Nombre", selectedMarcaId);
                }
                else
                {
                    ViewBag.Marcas = new SelectList(new List<MarcaResumenRespDto>(), "IdMarca", "Nombre");
                }
            }
            catch
            {
                ViewBag.Marcas = new SelectList(new List<MarcaResumenRespDto>(), "IdMarca", "Nombre");
            }
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