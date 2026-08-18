namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class InventarioFiltroReqDto
    {
        public int PageNumber { get; set; } = 1;
        public string? NombreProducto { get; set; }
        public string? CodigoProducto { get; set; }
        public long? IdAlmacen { get; set; }
        public string Orden { get; set; } = "DESC";
    }
}