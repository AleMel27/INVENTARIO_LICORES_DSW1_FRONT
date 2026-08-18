using Microsoft.AspNetCore.Mvc;

namespace GESTION_INVENTARIO_LICORES_MVC.Controllers
{
    public class AlmacenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
