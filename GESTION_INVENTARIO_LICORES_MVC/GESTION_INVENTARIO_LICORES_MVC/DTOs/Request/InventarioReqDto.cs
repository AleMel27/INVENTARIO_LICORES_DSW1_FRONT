using System.ComponentModel.DataAnnotations;

namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class InventarioReqDto
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

    }
}
