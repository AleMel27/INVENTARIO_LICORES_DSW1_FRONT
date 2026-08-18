namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class ProductoRespDto
    {
        public long IdProducto { get; set; }

        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public int CapacidadMl { get; set; }
        public decimal GradoAlcoholico { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockMinimo { get; set; }

        public CategoriaResumenRespDto Categoria { get; set; } = new();
        public MarcaResumenRespDto Marca { get; set; } = new();

        public bool Estado { get; set; }
    }
}
