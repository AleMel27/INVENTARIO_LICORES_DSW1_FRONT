using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class DetalleCompraReqDto
    {

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Debe seleccionar un producto válido."
        )]
        public long IdProducto { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a 0."
        )]
        public int Cantidad { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "9999999999.99",
            ErrorMessage = "El costo unitario no puede ser negativo."
        )]
        public decimal CostoUnitario { get; set; }

    }
}
