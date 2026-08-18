namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class MovimientoInventarioRespDto
    {


        public long IdMovimiento { get; set; }

        public ProductoResumenRespDto Producto { get; set; } = new();
        public AlmacenMovimientoRespDto Almacen { get; set; } = new();
        public UsuarioResumenRespDto Usuario { get; set; } = new();

        public CompraResumenRespDto? Compra { get; set; }

        public TipoMovimientoRespDto TipoMovimiento { get; set; } = new();

        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }
        public int StockPosterior { get; set; }

        public string Motivo { get; set; } = string.Empty;
        public string? Referencia { get; set; }

        public DateTime FechaMovimiento { get; set; }

    }
}
