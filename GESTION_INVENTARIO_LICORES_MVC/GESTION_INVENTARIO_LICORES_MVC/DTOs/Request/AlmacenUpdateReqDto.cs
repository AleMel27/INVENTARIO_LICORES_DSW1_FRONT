using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class AlmacenUpdateReqDto
    {

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(
            100,
            ErrorMessage = "El nombre no puede superar los 100 caracteres."
        )]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(
            255,
            ErrorMessage = "La descripción no puede superar los 255 caracteres."
        )]
        public string? Descripcion { get; set; }
    }


}
