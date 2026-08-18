using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class ProveedorUpdateReqDto
    {

        [Required(ErrorMessage = "El RUC es obligatorio.")]
        [RegularExpression(
           @"^[0-9]{11}$",
           ErrorMessage = "El RUC debe contener exactamente 11 dígitos."
       )]
        public string Ruc { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(
            150,
            ErrorMessage = "La razón social no puede superar los 150 caracteres."
        )]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(
            20,
            ErrorMessage = "El teléfono no puede superar los 20 caracteres."
        )]
        public string? Telefono { get; set; }

        [EmailAddress(
            ErrorMessage = "El correo electrónico no tiene un formato válido."
        )]
        [StringLength(
            150,
            ErrorMessage = "El correo no puede superar los 150 caracteres."
        )]
        public string? Correo { get; set; }

        [StringLength(
            255,
            ErrorMessage = "La dirección no puede superar los 255 caracteres."
        )]
        public string? Direccion { get; set; }

    }
}
