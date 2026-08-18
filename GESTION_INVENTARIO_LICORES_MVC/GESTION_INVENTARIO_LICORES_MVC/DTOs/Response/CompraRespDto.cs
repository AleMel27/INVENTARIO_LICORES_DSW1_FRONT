namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class CompraRespDto
    {
        public long IdCompra { get; set; }

        public ProveedorResumenRespDto Proveedor { get; set; } = new();
        public UsuarioResumenRespDto Usuario { get; set; } = new();
        public TipoComprobanteRespDto TipoComprobante { get; set; } = new();
        public AlmacenInventarioRespDto Almacen { get; set; } = new();

        public DateTime FechaCompra { get; set; }

        public string NumeroComprobante { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}
