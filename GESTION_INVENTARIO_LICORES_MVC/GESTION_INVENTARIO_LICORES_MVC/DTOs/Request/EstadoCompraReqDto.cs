using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class EstadoCompraReqDto
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [RegularExpression(
           "^(RECIBIDA|CANCELADA)$",
           ErrorMessage = "El estado solamente puede ser RECIBIDA o CANCELADA."
       )]
        public string Estado { get; set; } = string.Empty;

    }
}
