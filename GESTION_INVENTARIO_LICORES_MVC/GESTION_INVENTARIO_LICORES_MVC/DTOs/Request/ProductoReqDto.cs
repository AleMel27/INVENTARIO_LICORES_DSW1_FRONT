using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class ProductoReqDto
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar una categoría válida."
        )]
        public long IdCategoria { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar una marca válida."
        )]
        public long IdMarca { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(
            50,
            ErrorMessage = "El código no puede superar los 50 caracteres."
        )]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(
            150,
            ErrorMessage = "El nombre no puede superar los 150 caracteres."
        )]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(
            500,
            ErrorMessage = "La descripción no puede superar los 500 caracteres."
        )]
        public string? Descripcion { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La capacidad debe ser mayor a 0."
        )]
        public int CapacidadMl { get; set; }

        [Range(
            typeof(decimal),
            "0.0",
            "100.0",
            ErrorMessage = "El grado alcohólico debe estar entre 0 y 100."
        )]
        public decimal GradoAlcoholico { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "9999999999.99",
            ErrorMessage = "El precio de venta no puede ser negativo."
        )]
        public decimal PrecioVenta { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "El stock mínimo no puede ser negativo."
        )]
        public int StockMinimo { get; set; }
    }
}
