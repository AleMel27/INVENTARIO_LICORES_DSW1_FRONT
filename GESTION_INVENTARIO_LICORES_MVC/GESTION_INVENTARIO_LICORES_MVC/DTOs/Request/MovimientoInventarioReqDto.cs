using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class MovimientoInventarioReqDto
    {


        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un producto válido."
        )]
        public long IdProducto { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un almacén válido."
        )]
        public long IdAlmacen { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un usuario válido."
        )]
        public long IdUsuario { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "La compra seleccionada no es válida."
        )]
        public long? IdCompra { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un tipo de movimiento válido."
        )]
        public long IdTipoMovimiento { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a 0."
        )]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(
            255,
            ErrorMessage = "El motivo no puede superar los 255 caracteres."
        )]
        public string Motivo { get; set; } = string.Empty;

        [StringLength(
            100,
            ErrorMessage = "La referencia no puede superar los 100 caracteres."
        )]
        public string? Referencia { get; set; }
    }
}
