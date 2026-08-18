using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class MarcaReqDto
    {


        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(
            100,
            ErrorMessage = "El nombre no puede superar los 100 caracteres."
        )]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(
            50,
            ErrorMessage = "El país de origen no puede superar los 50 caracteres."
        )]
        public string? PaisOrigen { get; set; }


    }
}
