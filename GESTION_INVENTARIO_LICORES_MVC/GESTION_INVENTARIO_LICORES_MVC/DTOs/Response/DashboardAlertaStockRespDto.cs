namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class DashboardAlertaStockRespDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }
}