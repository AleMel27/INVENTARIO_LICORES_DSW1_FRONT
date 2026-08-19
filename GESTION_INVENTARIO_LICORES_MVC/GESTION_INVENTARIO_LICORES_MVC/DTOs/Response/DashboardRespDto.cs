namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class DashboardRespDto
    {
        public int TotalProductos { get; set; }
        public int ProductosStockBajo { get; set; }
        public int TotalProveedores { get; set; }
        public decimal ValorInventario { get; set; }

        public List<DashboardAlertaStockRespDto> AlertasStock { get; set; } = new();
        public List<DashboardUltimaCompraRespDto> UltimasCompras { get; set; } = new();
    }
}