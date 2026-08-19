namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class DashboardUltimaCompraRespDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }
}