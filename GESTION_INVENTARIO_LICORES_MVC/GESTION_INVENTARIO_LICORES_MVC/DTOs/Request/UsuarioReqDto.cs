using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class UsuarioReqDto
    {



        [Range(
           1,
           long.MaxValue,
           ErrorMessage = "Debe seleccionar un rol válido."
       )]
        public long IdRol { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(
            100,
            ErrorMessage = "Los nombres no pueden superar los 100 caracteres."
        )]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(
            100,
            ErrorMessage = "Los apellidos no pueden superar los 100 caracteres."
        )]
        public string Apellidos { get; set; } = string.Empty;

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
            MinimumLength = 8,
            ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres."
        )]
        public string Password { get; set; } = string.Empty;





    }
}
