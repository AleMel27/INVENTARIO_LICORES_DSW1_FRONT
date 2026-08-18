namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Request
{
    public class CompraFiltroReqDto
    {
        public int PageNumber { get; set; } = 1;
        public string? Estado { get; set; }
        public long? IdTipoComprobante { get; set; }
        public long? IdAlmacen { get; set; }
        public DateTime? Fecha { get; set; }
        public string? RazonSocial { get; set; }
        public string? NumeroComprobante { get; set; }
        public string Orden { get; set; } = "DESC";
    }
}