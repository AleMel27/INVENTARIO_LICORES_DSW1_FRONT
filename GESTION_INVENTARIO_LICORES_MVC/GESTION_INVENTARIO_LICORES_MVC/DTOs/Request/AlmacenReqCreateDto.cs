using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class AlmacenReqCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(
           100,
           ErrorMessage = "El nombre no puede superar los 100 caracteres."
       )]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ubicación es obligatoria.")]
        [StringLength(
            200,
            ErrorMessage = "La ubicación no puede superar los 200 caracteres."
        )]
        public string Ubicacion { get; set; } = string.Empty;

        [StringLength(
            255,
            ErrorMessage = "La descripción no puede superar los 255 caracteres."
        )]
        public string? Descripcion { get; set; }

    }
}
