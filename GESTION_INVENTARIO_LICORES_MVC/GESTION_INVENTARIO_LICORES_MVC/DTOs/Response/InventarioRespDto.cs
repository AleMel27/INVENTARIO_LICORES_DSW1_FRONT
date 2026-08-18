namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class InventarioRespDto
    {

        public long IdInventario { get; set; }

        public ProductoResumenRespDto Producto { get; set; } = new();
        public AlmacenInventarioRespDto Almacen { get; set; } = new();

        public int StockActual { get; set; }

    }
}
