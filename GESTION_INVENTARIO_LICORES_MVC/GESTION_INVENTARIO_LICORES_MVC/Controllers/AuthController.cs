using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GESTION_INVENTARIO_LICORES_MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthApiService _authApiService;

        public AuthController(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginReqDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _authApiService.LoginAsync(model);

            if (resultado is null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas o cuenta inactiva.");
                return View(model);
            }

            HttpContext.Session.SetString("Token", resultado.Token);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, resultado.Usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, $"{resultado.Usuario.Nombres} {resultado.Usuario.Apellidos}"),
                new Claim(ClaimTypes.Email, resultado.Usuario.Correo),
                new Claim(ClaimTypes.Role, resultado.Usuario.Rol.Nombre),
                new Claim("JWToken", resultado.Token)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = resultado.Expiracion
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Auth");
        }
    }
}