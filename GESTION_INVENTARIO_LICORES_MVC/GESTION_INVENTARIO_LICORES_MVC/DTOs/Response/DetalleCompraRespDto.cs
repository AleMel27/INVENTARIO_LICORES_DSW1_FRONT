namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class DetalleCompraRespDto
    {

        public long IdDetalleCompra { get; set; }

        public CompraResumenRespDto Compra { get; set; } = new();
        public ProductoResumenRespDto Producto { get; set; } = new();

        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Subtotal { get; set; }


    }
}
