using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class LoginReqDto
    {

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(
           ErrorMessage = "El correo electrónico no tiene un formato válido."
       )]
        [StringLength(
           150,
           ErrorMessage = "El correo no puede superar los 150 caracteres."
       )]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(
            100,
            ErrorMessage = "La contraseña no puede superar los 100 caracteres."
        )]
        public string Password { get; set; } = string.Empty;



    }
}
