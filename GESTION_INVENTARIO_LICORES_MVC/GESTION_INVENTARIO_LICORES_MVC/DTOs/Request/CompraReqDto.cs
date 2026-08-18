using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class CompraReqDto
    {

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un proveedor válido."
        )]
        public long IdProveedor { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un usuario válido."
        )]
        public long IdUsuario { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un tipo de comprobante válido."
        )]
        public long IdTipoComprobante { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un almacén válido."
        )]
        public long IdAlmacen { get; set; }

        [Required(ErrorMessage = "El número de comprobante es obligatorio.")]
        [StringLength(
            50,
            ErrorMessage = "El número de comprobante no puede superar los 50 caracteres."
        )]
        public string NumeroComprobante { get; set; } = string.Empty;

        [StringLength(
            500,
            ErrorMessage = "La observación no puede superar los 500 caracteres."
        )]
        public string? Observacion { get; set; }

        [Required(ErrorMessage = "Debe agregar los detalles de la compra.")]
        [MinLength(
            1,
            ErrorMessage = "Debe agregar al menos un producto a la compra."
        )]
        public List<DetalleCompraReqDto> Detalles { get; set; } = [];


    }
}
